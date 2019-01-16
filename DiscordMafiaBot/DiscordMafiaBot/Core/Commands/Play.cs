using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordMafiaBot.Core.Commands
{
	public class Play : Mafia
	{
		private List<ulong> remainPlayer = new List<ulong>();

		// 시작 명령어
		[Command("start")]
		public async Task Start()
		{
			// DM에서 사용 불가능
			if (Context.Channel.GetType() == typeof(SocketDMChannel))
			{
				await WrongCommandNotDM();
				return;
			}

			// 레디 상태일때만 사용 가능
			if (gameStatus != GameStatus.Ready)
			{
				await WrongCommandTiming();
				return;
			}

			int playerCount = playerList.Count;

			if (playerCount < 4)
			{
				await Context.Channel.SendMessageAsync("아무리그래도 `" + playerCount + "명`은 너무 적지 않나..");
				return;
			}
			if (mainChannel == null)
			{
				await Context.Channel.SendMessageAsync("메인채널이 없네요? 일단 `#" + Context.Channel + "`로 설정하겠습니다!");
			}

			// 리스트로 옮김
			KeyValuePair<ulong, Player>[] arr;
			arr = playerList.ToArray();

			foreach (var d in arr)
			{
				remainPlayer.Add(d.Key);
			}
			
			// 마피아 1
			if (playerCount <= 7)		{ SetJob(JobType.Mafia, 1); }
			// 마피아 2
			else if (playerCount <= 12)	{ SetJob(JobType.Mafia, 2); }
			// 마피아 3
			else						{ SetJob(JobType.Mafia, 3); }

			// 늑대인간 1
			if (playerCount >= 8)		{ SetJob(JobType.Wolf, 1); }

			// 스파이 1
			if (playerCount >= 6)		{ SetJob(JobType.Spy, 1); }

			// 의사 1 경찰 1
			SetJob(JobType.Cop, 1);
			SetJob(JobType.Doctor, 1);

			// 국회의원 1
			if (playerCount >= 7)		{ SetJob(JobType.MOCongress, 1); }

			// 직업 알려주기
			await SendJob();
			// 게임 루프 시작
			await GameLoop();
		}

		// 게임 루프
		private async Task GameLoop()
		{
			while (true)
			{
				Check();
				Day();
				Vote();
				Judge();
				Check();
				Night();
			}
		}
		
		// 낮
		private async Task Day()
		{
			gameStatus = GameStatus.Day;
		}

		// 투표 
		private async Task Vote()
		{
			gameStatus = GameStatus.Vote;
		}

		// 찬반투표
		private async Task Judge()
		{
			gameStatus = GameStatus.Judge;
		}

		// 게임 상황 체크
		private async Task Check()
		{
			int mafiaVote = 0;
			int citizenVote = 0;

		}

		// 밤
		private async Task Night()
		{
			gameStatus = GameStatus.Night;
		}

		// 직업 설정
		private void SetJob(JobType jobType, int count)
		{
			Random r = new Random();
			int remain = remainPlayer.Count;

			for (int i = 0; i < count; i++)
			{
				int index = r.Next(0, remain - 1);
				ulong userId = remainPlayer[index];

				// 직업 할당
				playerList[userId].job = jobType;

				// 직업 할당된사람 제외
				remain--;
				remainPlayer.RemoveAt(index);
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
	}
}
