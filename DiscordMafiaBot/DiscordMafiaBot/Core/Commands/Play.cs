using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordMafiaBot.Core.Commands
{
	public class Play : Mafia
	{
		static public ulong judgeTarget = 0;
		static public int mafiaCount;
		static private int agree;
		static private int disagree;
		static private EmbedBuilder result = new EmbedBuilder();
		static private List<ulong> nojobPlayer;

		// 시작 명령어
		[Command("start")]
		public async Task Start()
		{
			// DM에서 사용 불가능
			if (Context.Channel.GetType() == typeof(SocketDMChannel))
			{
				await WrongCommand(CommandType.NotDM);
				return;
			}

			// 레디 상태일때만 사용 가능
			if (gameStatus != GameStatus.Ready)
			{
				await WrongCommand(CommandType.Timing);
				return;
			}

			playerCount = playerList.Count;

			if (playerCount < 4)
			{
				await Context.Channel.SendMessageAsync("아무리그래도 `" + playerCount + "명`은 너무 적지 않나..");
				return;
			}
			if (mainChannel == null)
			{
				mainChannel = Context.Channel;
				await Context.Channel.SendMessageAsync("메인채널이 없네요? 일단 `#" + Context.Channel + "`로 설정하겠습니다!");
			}

			await ShowStatus();
			await Context.Channel.SendMessageAsync("지금부터 역할을 나누어드리겠습니다! DM을 확인해주세요!");

			// 직업 배정을 위해 리스트로 옮김
			nojobPlayer = new List<ulong>();
			foreach (var keyvalue in playerList)
			{
				nojobPlayer.Add(keyvalue.Key);
			}

			// 마피아 1
			if (playerCount <= 7) { SetJob(JobType.Mafia, 1); mafiaCount = 1; }
			// 마피아 2
			else if (playerCount <= 12) { SetJob(JobType.Mafia, 2); mafiaCount = 2; }
			// 마피아 3
			else { SetJob(JobType.Mafia, 3); mafiaCount = 3; }

			// 늑대인간 1
			if (playerCount >= 8) { SetJob(JobType.Wolf, 1); }

			// 스파이 1
			if (playerCount >= 6) { SetJob(JobType.Spy, 1); }

			// 의사 1 경찰 1
			SetJob(JobType.Cop, 1);
			SetJob(JobType.Doctor, 1);

			// 국회의원 1
			if (playerCount >= 7) { SetJob(JobType.MOCongress, 1); }

			// 직업 알려주기
			await SendJob();

			// 생존자 목록 갱신등의 데이터 초기화
			wolfLink = false;
			currentDay = 0;
			livePlayer = new List<ulong>();
			foreach (var keyvalue in playerList)
			{
				livePlayer.Add(keyvalue.Key);
			}

			// 게임 루프 시작
			await GameLoop();
		}

		// 스킵
		[Command("skip")]
		public async Task Skip()
		{
			isTimerStop = true;
		}

		// 찬성
		[Command("agree")]
		public async Task Agree()
		{
			// DM에서 사용 불가능
			if (Context.Channel.GetType() == typeof(SocketDMChannel))
			{
				await WrongCommand(CommandType.NotDM);
				return;
			}

			// 죽은사람이 사용
			if (playerList[Context.User.Id].isDead)
			{
				await WrongCommand(CommandType.DiePlayer);
				return;
			}

			// 잘못된 명령어 타이밍
			if (gameStatus != GameStatus.Judge)
			{
				await WrongCommand(CommandType.Timing);
			}

			// 자기 자신에대해 투표
			if (Play.judgeTarget == Context.User.Id)
			{
				await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 살려고 발버두치는 모습이 아주 보기 좋군요..");
				return;
			}
			await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님 `찬성`!");
			voteList[Context.User.Id] = 1;
		}

		// 반대
		[Command("disagree")]
		public async Task Disagree()
		{
			// DM에서 사용 불가능
			if (Context.Channel.GetType() == typeof(SocketDMChannel))
			{
				await WrongCommand(CommandType.NotDM);
				return;
			}

			// 죽은사람이 사용
			if (playerList[Context.User.Id].isDead)
			{
				await WrongCommand(CommandType.DiePlayer);
				return;
			}

			// 잘못된 명령어 타이밍
			if (gameStatus != GameStatus.Judge)
			{
				await WrongCommand(CommandType.Timing);
			}

			// 자기 자신에대해 투표
			if (Play.judgeTarget == Context.User.Id)
			{
				await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 살려고 발버두치는 모습이 아주 보기 좋군요..");
				return;
			}

			await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님 `반대`!");
			voteList[Context.User.Id] = 2;
		}

		// 게임 루프
		private async Task GameLoop()
		{
			while (true)
			{
				if (CheckGame()) { break; }
				currentDay++;
				await DayTime();
				await VoteTime();
				if (judgeTarget != 0) { await JudgeTime(); }
				if (CheckGame()) { break; }
				await NightTime();
			}

			await EndGame();
		}
		
		// 낮
		private async Task DayTime()
		{
			gameStatus = GameStatus.Day;

			await Context.Channel.SendMessageAsync("모두들! 지금은 낮입니다! 토론을 진행해주세요!");
			await Timer(times.day);
		}

		// 투표 
		private async Task VoteTime()
		{
			// 초기화
			voteList = new ConcurrentDictionary<ulong, ulong>();

			// 시간설정
			gameStatus = GameStatus.Vote;

			await Context.Channel.SendMessageAsync("이제부터 투표시간입니다. 봇에게 DM으로 `s.vote 플레이어_번호`를 입력해주세요!");
			await Timer(times.vote);
			
			// 집계
			ConcurrentDictionary<ulong, int> voteReult = new ConcurrentDictionary<ulong, int>();
			ulong bigestUser = Context.User.Id;

			voteReult[bigestUser] = 0;
			foreach(var keyvalue in voteList)
			{
				if (keyvalue.Value != 0)
				{
					int voteCount = 1;

					if (playerList[keyvalue.Key].job == JobType.MOCongress)
					{
						voteCount = 2;
					}

					if (voteReult.ContainsKey(keyvalue.Value))
					{
						voteReult[keyvalue.Value] += voteCount;
					}
					else
					{
						voteReult.TryAdd(keyvalue.Value, voteCount);
					}
				}
			}
			
			foreach(var keyvalue in voteReult)
			{
				if (voteReult[bigestUser] < keyvalue.Value)
				{
					bigestUser = keyvalue.Key;
				}
			}

			judgeTarget = bigestUser;
			// 결과
			if (voteReult[bigestUser] >= (playerCount / 2) + 1)
			{
				await Context.Channel.SendMessageAsync("<@" + judgeTarget + ">님이 " + voteReult[bigestUser] + "표로 가장 많은 표를 받아 단두대에 올라섰습니다.");
			}
			else
			{
				await Context.Channel.SendMessageAsync("<@" + judgeTarget + ">님이 " + voteReult[bigestUser] + "표로 가장 많은 표를 받았지만 과반수를 넘지 않아 투표가 무효화 되었습니다.");
				judgeTarget = 0;
			}
		}

		// 찬반투표
		private async Task JudgeTime()
		{
			// 초기화
			agree = 0;
			disagree = 0;
			voteList = new ConcurrentDictionary<ulong, ulong>();

			// 시간 설정
			gameStatus = GameStatus.Judge;

			await Context.Channel.SendMessageAsync("<@" + judgeTarget + ">님의 처형에 대해 찬반 투표가 진행됩니다. 현재 채널에 `s.agree` 또는 `s.disagree`를 입력해주세요!");
			await Timer(times.judge);

			EmbedBuilder embed = new EmbedBuilder();

			// 집계
			foreach (var keyvalue in voteList)
			{
				if (keyvalue.Value == 1)
				{
					agree++;
				}
				if (keyvalue.Value == 2)
				{
					disagree++;
				}
			}

			// 결과
			embed.WithColor(color);
			embed.Description += "찬성 `" + agree + "`표\n";
			embed.Description += "반대 `" + disagree + "`표\n";

			if (agree >= ((playerCount - 1) / 2) + 1)
			{
				if (playerList[judgeTarget].job == JobType.MOCongress)
				{
					embed.Description += "투표 결과 <@" + judgeTarget + ">님의 사형이 집행되어야 했습니다만..\n" +
										 "그는 `국회의원` 이었습니다!";
				}
				else
				{
					embed.Description += "투표 결과 <@" + judgeTarget + ">님의 사형이 집행되었습니다.";
					await KillPlayer(judgeTarget, false);
				}
			}
			else
			{
				embed.Description += "투표 결과 <@" + judgeTarget + ">님은 무사히 단두대에서 내려왔습니다.";
			}

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		// 게임 상황 체크
		private bool CheckGame()
		{
			int mafiaVote = 0;
			int citizenVote = 0;

			foreach (var userId in livePlayer)
			{
				if (playerList[userId].job >= JobType.Mafia)
				{
					mafiaVote++;
				}
				else
				{
					citizenVote++;
				}
			}

			if (mafiaVote >= citizenVote)
			{
				EmbedFieldBuilder embedField = new EmbedFieldBuilder();
				embedField.Name = "게임 결과";
				embedField.Value = "총 `" + currentDay + "`일동안 게임을 진행하였습니다.\n" +
									"`마피아`팀의 승리!";
				result.Fields.Add(embedField);

				return true;
			}

			if (mafiaVote == 0)
			{
				EmbedFieldBuilder embedField = new EmbedFieldBuilder();
				embedField.Name = "게임 결과";
				embedField.Value = "총 `" + currentDay + "`일동안 게임을 진행하였습니다.\n" +
									"`시민`팀의 승리!";
				result.Fields.Add(embedField);

				return true;
			}

			return false;
		}

		// 밤
		private async Task NightTime()
		{
			// 초기화
			currentScene = new Scene();

			// 시간설정
			gameStatus = GameStatus.Night;

			await Context.Channel.SendMessageAsync("밤이되었습니다! 봇에게 DM으로 `s.shot 플레이어_번호`로 능력을 사용해주세요!");
			await Timer(times.night);
			
			// 늑대인간
			if (mafiaCount == 0 && wolfLink)
			{
				await KillPlayer(currentScene.wolfPick.Value, true);
			}

			// 마피아
			if (mafiaCount > 0)
			{
				await KillPlayer(currentScene.mafiaKill, true);
			}

			// 경찰
			if (currentScene.copCheck.Key == 0 || currentScene.copCheck.Value == 0) { }
			else
			{
				if (CheckMafiaTeam(currentScene.copCheck.Value))
				{
					await SendDM(currentScene.copCheck.Key, (playerList[currentScene.copCheck.Value].name + "님은 마피아팀이었습니다!"));
				}
				else
				{
					await SendDM(currentScene.copCheck.Key, (playerList[currentScene.copCheck.Value].name + "님은 마피아팀이 아니었습니다.."));
				}
			}

			// 스파이
			await ShowJob(currentScene.spyCheck.Value, currentScene.spyCheck.Key);
		}

		// 게임 종료
		private async Task EndGame()
		{
			// 결과창
			EmbedFieldBuilder embedField = new EmbedFieldBuilder();

			embedField.Name = "그들의 직업은..?";
			foreach (var keyvalue in playerList)
			{
				embedField.Value += ConvertJob(keyvalue.Value.job) + " <@" + keyvalue.Key + "> " + "\n";
			}
			result.Fields.Add(embedField);

			result.WithColor(color);
			await Context.Channel.SendMessageAsync("", false, result.Build());
			
			// 초기화
			playerList = new ConcurrentDictionary<ulong, Player>();
			gameStatus = GameStatus.Ready;
		}

		// 직업 설정
		private void SetJob(JobType jobType, int count)
		{
			Random r = new Random();
			int nojob = nojobPlayer.Count;
			List<ulong> sameJobs = new List<ulong>();

			for (int i = 0; i < count; i++)
			{
				int index = r.Next(0, nojob);
				ulong userId = nojobPlayer[index];

				// 직업 할당
				playerList[userId].job = jobType;
				sameJobs.Add(userId);

				// 직업 할당된사람 제외
				nojob--;
				nojobPlayer.RemoveAt(index);
			}

			if (jobType == JobType.Mafia && count > 1)
			{
				for (int i = 0; i < count; i++)
				{
					LinkMafia(sameJobs[i], sameJobs[(i + 1) % count]);
				}
			}
		}

		// 직업 알려줌
		private async Task SendJob()
		{
			foreach (var keyvalue in playerList)
			{
				await SendDM(keyvalue.Key, "<@" + keyvalue.Key + ">님! 당신은 " + ConvertJob(keyvalue.Value.job) + "입니다. 승리가 아니면 죽음을!");
			}
		}

		// 타인에게 직업 알려주기
		private async Task ShowJob(ulong targetId, ulong sourceId)
		{
			if (targetId == 0 || sourceId == 0)
			{
				return;
			}

			await SendDM(sourceId, playerList[targetId].name + "님은.. " + ConvertJob(playerList[targetId].job) + "입니다!");

			if (playerList[sourceId].job == JobType.Spy && playerList[targetId].job == JobType.Mafia)
			{
				LinkMafia(targetId, sourceId);
			}
		}

		// 접선
		private async void LinkMafia(ulong mafiaId, ulong otherId)
		{
			await SendDM(otherId, playerList[mafiaId].name + "님이 " + ConvertJob(playerList[mafiaId].job) + "인게 확인되어 접선을 성공했습니다! 이제부터 개인 DM으로 메세지를 주고받을 수 있습니다.");
			await SendDM(mafiaId, playerList[otherId].name + "님이 " + ConvertJob(playerList[otherId].job) + "직업으로 접선을 하였습니다! 이제부터 개인 DM으로 메세지를 주고받을 수 있습니다.");
		}

		// 마피아팀인지 체크해 알려주기
		private bool CheckMafiaTeam(ulong targetId)
		{
			// 입력 안됨
			if (targetId == 0)
			{
				return false;
			}

			// 마피아 팀이면
			if (playerList[targetId].job >= JobType.Mafia)
			{
				return true;
			}
			return false;
		}

		// 마피아 찾기
		private ulong FindMafia()
		{
			List<ulong> mafias = new List<ulong>();
			Random r = new Random();

			foreach (var userId in livePlayer)
			{
				if (playerList[userId].job == JobType.Mafia)
				{
					mafias.Add(userId);
				}
			}

			return mafias[r.Next(0, mafias.Count)];
		}

		// 사람 죽이기
		private async Task KillPlayer(ulong userId, bool isMafia)
		{
			// 마피아에 의한 살인인지
			if (isMafia)
			{
				// 의사가 살렸는지
				if (currentScene.mafiaKill == currentScene.doctorSaver || userId == 0)
				{
					await NobodyDead();
				}
				// 늑대인간을 지목했는지
				else if (mafiaCount != 0 && playerList[currentScene.mafiaKill].job == JobType.Wolf)
				{
					LinkMafia(FindMafia(), currentScene.mafiaKill);
					wolfLink = true;
					await NobodyDead();
				}
				else
				{
					// 늑대인간이 지목한사람인지
					if (mafiaCount != 0 && currentScene.wolfPick.Value == userId)
					{
						LinkMafia(FindMafia(), currentScene.wolfPick.Key);
						wolfLink = true;
					}

					await Context.Channel.SendMessageAsync("어젯밤, 누군가에 의해 <@" + userId + ">님이 살해당하였습니다..");
					DeletePlayer(userId);

					//if (userId == 254872929395802112 && currentDay == 1)
					//{
					//	await Context.Channel.SendMessageAsync("아니근데 개발자를 퍼블시키는건 좀 아니지 않나..");
					//}
				}
			}
			else
			{
				await Context.Channel.SendMessageAsync("<@" + userId + ">님이 사망하였습니다..");
				DeletePlayer(userId);
			}

			playerCount--;
		}

		// 아무도 안죽음
		private async Task NobodyDead()
		{
			await Context.Channel.SendMessageAsync("어젯밤, 기적적으로 아무도 죽지 않았습니다!");
		}

		// 플레이어 삭제
		private void DeletePlayer(ulong userId)
		{
			playerList[userId].isDead = true;
			livePlayer.RemoveAt(livePlayer.IndexOf(userId));

			if (playerList[userId].job == JobType.Mafia)
			{
				mafiaCount--;
			}
		}
	}
}
