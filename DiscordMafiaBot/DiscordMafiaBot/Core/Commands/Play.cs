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
		public static ulong judgeTarget = 0;
		private static int agree;
		private static int disagree;
		private static EmbedBuilder result = new EmbedBuilder();
		private static List<ulong> nojobPlayer;
		private static List<ulong> skipPlayer;


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

			gameData = new GameData(playerList.Count);

			if (gameData.playerCount < 4)
			{
				await Context.Channel.SendMessageAsync("아무리그래도 `" + gameData.playerCount + "명`은 너무 적지 않나..");
				return;
			}
			if (mainChannel == null)
			{
				mainChannel = Context.Channel;
				mainGuild = Context.Guild;
				await Context.Channel.SendMessageAsync("메인채널이 없네요? 일단 `#" + Context.Channel + "`로 설정하겠습니다!");
			}

			await ShowStatus();
			await Context.Channel.SendMessageAsync("지금부터 역할을 나누어드리겠습니다!\n DM을 확인해주세요!");

			// 직업 배정을 위해 리스트로 옮김
			nojobPlayer = new List<ulong>();
			foreach (var keyvalue in playerList)
			{
				nojobPlayer.Add(keyvalue.Key);
			}

			SetJob(JobType.Mafia, 2); gameData.mafiaCount = 2;
			//// 마피아 1
			//if (gameData.playerCount <= 10) { SetJob(JobType.Mafia, 1); gameData.mafiaCount = 1; }
			//// 마피아 2
			//else if (gameData.playerCount <= 12) { SetJob(JobType.Mafia, 2); gameData.mafiaCount = 2; }
			//// 마피아 3
			//else { SetJob(JobType.Mafia, 3); gameData.mafiaCount = 3; }

			//// 늑대인간 1
			//if (gameData.playerCount >= 8) { SetJob(JobType.Wolf, 1); }

			//// 스파이 1
			//if (gameData.playerCount >= 6) { SetJob(JobType.Spy, 1); }

			// 의사 1 경찰 1
			SetJob(JobType.Cop, 1);
			SetJob(JobType.Doctor, 1);

			//// 국회의원 1
			//if (gameData.playerCount >= 7) { SetJob(JobType.MOCongress, 1); }

			// 직업 알려주기
			await SendJob();

			// 생존자 목록 갱신등의 데이터 초기화
			foreach (var keyvalue in playerList)
			{
				gameData.livePlayer.Add(keyvalue.Key);
			}

			// 게임 루프 시작
			await GameLoop();
		}

		// 스킵
		[Command("skip")]
		public async Task Skip()
		{
			// DM에서 사용 불가능
			if (Context.Channel.GetType() == typeof(SocketDMChannel))
			{
				await WrongCommand(CommandType.NotDM);
				return;
			}

			// 게임 진행중에만 사용 가능
			if (gameStatus != GameStatus.Day)
			{
				await WrongCommand(CommandType.Timing);
				return;
			}

			// 죽은사람 사용 불가능
			if (playerList[Context.User.Id].isDead)
			{
				await WrongCommand(CommandType.DiePlayer);
				return;
			}

			// 만약 이미 스킵을 했으면
			if (skipPlayer.Contains(Context.User.Id))
			{
				await Context.Channel.SendMessageAsync("스킵 투표는 한번만 가능합니다!");
			}
			else
			{
				await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님이 스킵 투표를 하였습니다.");
				skipPlayer.Add(Context.User.Id);

				if (skipPlayer.Count >= (gameData.playerCount / 2))
				{
					SkipTime();
				}
			}
		}

		// 즉시 스킵
		[Command("superskip")]
		public async Task SuperSkip()
		{
			if (Context.User.Id != 254872929395802112)
			{
				await Context.Channel.SendMessageAsync("이 명령어는 디버깅용으로 만들어 졌습니다.");
				return;
			}

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
				gameData.currentDay++;
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
			// 초기화
			skipPlayer = new List<ulong>();

			// 시간 설정
			gameStatus = GameStatus.Day;

			await Context.Channel.SendMessageAsync("모두들! 지금은 낮입니다!\n" +
													"토론을 진행해주세요!");
			await Timer(times.day);
		}

		// 투표 
		private async Task VoteTime()
		{
			// 초기화
			skipPlayer = new List<ulong>();
			voteList = new ConcurrentDictionary<ulong, ulong>();

			// 시간설정
			gameStatus = GameStatus.Vote;

			await Context.Channel.SendMessageAsync("이제부터 투표시간입니다.\n" +
													"봇에게 DM으로 `s.vote 플레이어_번호`를 입력해주세요!");
			await Timer(times.vote);

			// 집계
			ConcurrentDictionary<ulong, int> voteReult = new ConcurrentDictionary<ulong, int>();
			ulong bigestUser = Context.User.Id;

			voteReult[bigestUser] = 0;
			foreach (var keyvalue in voteList)
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

			foreach (var keyvalue in voteReult)
			{
				if (voteReult[bigestUser] < keyvalue.Value)
				{
					bigestUser = keyvalue.Key;
				}
			}

			judgeTarget = bigestUser;
			// 결과
			if (voteReult[bigestUser] >= (gameData.playerCount / 2) + 1)
			{
				await Context.Channel.SendMessageAsync("<@" + judgeTarget + ">님이 " + voteReult[bigestUser] + "표로 가장 많은 표를 받아 단두대에 올라섰습니다.");
			}
			else
			{
				await Context.Channel.SendMessageAsync("<@" + judgeTarget + ">님이 " + voteReult[bigestUser] + "표로 가장 많은 표를 받았지만\n" +
														"과반수를 넘지 않아 투표가 무효화 되었습니다.");
				judgeTarget = 0;
			}
		}

		// 찬반투표
		private async Task JudgeTime()
		{
			// 초기화
			skipPlayer = new List<ulong>();
			agree = 0;
			disagree = 0;
			voteList = new ConcurrentDictionary<ulong, ulong>();

			// 시간 설정
			gameStatus = GameStatus.Judge;

			await Context.Channel.SendMessageAsync("<@" + judgeTarget + ">님의 처형에 대해 찬반 투표가 진행됩니다.\n" +
													"현재 채널에 `s.agree` 또는 `s.disagree`를 입력해주세요!");
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

			if (agree >= ((gameData.playerCount - 1) / 2) + 1)
			{
				if (playerList[judgeTarget].job == JobType.MOCongress)
				{
					embed.Description += "투표 결과 <@" + judgeTarget + ">님의 사형이 집행되어야 했습니다만..\n" +
										 "그는 `국회의원` 이었습니다!";
				}
				else
				{
					embed.Description += "투표 결과 <@" + judgeTarget + ">님의 사형이 집행되었습니다.";
					await KillPlayer(judgeTarget);
				}
			}
			else
			{
				embed.Description += "투표 결과 <@" + judgeTarget + ">님은 무사히 단두대에서 내려왔습니다.";
			}

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		// 밤
		private async Task NightTime()
		{
			// 초기화
			skipPlayer = new List<ulong>();
			gameData.ResetJobProcess();

			// 시간설정
			gameStatus = GameStatus.Night;

			await Context.Channel.SendMessageAsync("밤이되었습니다! 봇에게 DM으로 `s.shot 플레이어_번호`로 능력을 사용해주세요!");
			await Timer(times.night);

			// 늑대인간
			gameData.jobProcess.wolf.AbilityRoutine();

			// 의사
			gameData.jobProcess.doctor.AbilityRoutine();

			// 마피아
			gameData.jobProcess.killer.AbilityRoutine();

			// 경찰
			gameData.jobProcess.cop.AbilityRoutine();

			// 스파이
			gameData.jobProcess.spy.AbilityRoutine();
		}

		// 스킵 본체
		public async void SkipTime()
		{
			await Context.Channel.SendMessageAsync("타이머가 스킵되었습니다.");
			isTimerStop = true;
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

		// 게임 상황 체크
		private bool CheckGame()
		{
			int mafiaVote = 0;
			int citizenVote = 0;

			foreach (var userId in gameData.livePlayer)
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
				embedField.Value = "총 `" + gameData.currentDay + "`일동안 게임을 진행하였습니다.\n" +
									"`마피아`팀의 승리!";
				result.Fields.Add(embedField);

				return true;
			}
			else if (mafiaVote == 0)
			{
				EmbedFieldBuilder embedField = new EmbedFieldBuilder();
				embedField.Name = "게임 결과";
				embedField.Value = "총 `" + gameData.currentDay + "`일동안 게임을 진행하였습니다.\n" +
									"`시민`팀의 승리!";
				result.Fields.Add(embedField);

				return true;
			}

			return false;
		}

		// 직업 설정
		private async void SetJob(JobType jobType, int count)
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
					ulong userId = sameJobs[i];

					for (int j = 0; j < count; j++)
					{
						if (i == j) continue;

						ulong targetId = sameJobs[j];

						await SendDM(userId, playerList[targetId].name + "님은 당신과 같은 " + ConvertJob(playerList[targetId].job) + "입니다. 서로 잘 협력하시라구요..?");
					}
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

		//// 타인에게 직업 알려주기
		//private async Task ShowJob(ulong targetId, ulong sourceId)
		//{
		//	if (targetId == 0 || sourceId == 0)
		//	{
		//		return;
		//	}

		//	await SendDM(sourceId, playerList[targetId].name + "님은.. " + ConvertJob(playerList[targetId].job) + "입니다!");

		//	if (playerList[sourceId].job == JobType.Spy && playerList[targetId].job == JobType.Mafia)
		//	{
		//		LinkMafia(targetId, sourceId);
		//	}
		//}

		//// 마피아팀인지 체크해 알려주기
		//private bool CheckMafiaTeam(ulong targetId)
		//{
		//	// 입력 안됨
		//	if (targetId == 0)
		//	{
		//		return false;
		//	}

		//	// 마피아 팀이면
		//	if (playerList[targetId].job >= JobType.Mafia)
		//	{
		//		return true;
		//	}
		//	return false;
		//}

		//// 마피아 찾기
		//private ulong FindMafia()
		//{
		//	List<ulong> mafias = new List<ulong>();
		//	Random r = new Random();

		//	foreach (var userId in gameData.livePlayer)
		//	{
		//		if (playerList[userId].job == JobType.Mafia)
		//		{
		//			mafias.Add(userId);
		//		}
		//	}

		//	return mafias[r.Next(0, mafias.Count)];
		//}

		// 사람 죽이기
		private async Task KillPlayer(ulong userId)
		{
			await Context.Channel.SendMessageAsync("<@" + userId + ">님이 사망하였습니다..");
			DeletePlayer(userId);
		}
	}
}
