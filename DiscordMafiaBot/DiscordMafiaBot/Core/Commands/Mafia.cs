using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Discord;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordMafiaBot.Core.Commands
{
	public class Times
	{
		public int day = 300;
		public int vote = 60;
		public int judge = 60;
		public int night = 120;
	}


	public class Player
	{
		public JobType job = 0;
		public string ability = null;
		public bool isDead = false;
	}

	public enum JobType
	{
		Citizen = 0,
		Cop = 1,
		Doctor = 2,
		MOCongress = 3,
		Mafia = 10,
		Wolf = 11,
		Spy = 12
	}

	public enum GameStatus
	{
		Ready,
		Day,
		Vote,
		Judge,
		Night
	}

	public class Mafia : ModuleBase<SocketCommandContext>
	{
		public static ConcurrentDictionary<ulong, Player> playerList = new ConcurrentDictionary<ulong, Player>();
		public static Color color = new Color(252, 138, 136);		// 시그니처 컬러
		public static GameStatus gameStatus = GameStatus.Ready;     // 게임 상태
		public static Times times = new Times();					// 시간 설정
		public static ISocketMessageChannel mainChannel;			// 메인 채널
		public static SocketGuild mainGuild;						// 메인 그룹
		public static bool isTimerStop = false;						// 타이머 정지 플래그
		
		// 상태 확인 명령어 s.state
		[Command("status"), Summary("Mafia show state command")]
		public async Task ShowStatus()
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithColor(color);

			// 현재 게임 진행 상태 및 채널 상태
			EmbedFieldBuilder embedField = new EmbedFieldBuilder();
			if (mainChannel == null)
			{
				embedField.Name = "설정된 메인 채널이 없어요..";
			}
			else
			{
				embedField.Name = "현재 설정된 메인 채널은 `#" + mainChannel + "` 입니다.";
			}
			embedField.Value = "게임 대기중..";
			embed.Fields.Add(embedField);

			// 현재 플레이어 목록
			embedField = new EmbedFieldBuilder();
			embedField.Name = "플레이어 목록";

			int num = 1;
			foreach (var keyValuePair in playerList)
			{
				embedField.Value += num.ToString() + ". <@" + keyValuePair.Key + ">\n";
				num++;
			}
			if (num == 1)
			{
				embedField.Value = "아무도 없는디요?";
			}
			embed.Fields.Add(embedField);

			// 메세지 출력
			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		// 잘못된 명령어
		protected async Task WrongCommand()
		{
			await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 명령어를 잘못 치신것 같은데요?");
		}

		// 잘못된 명령어 - 봇을 대상으로 지정
		protected async Task WrongCommandBot()
		{
			await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 마피아를 봇전으로 하는건 슬프지 않을까요?");
		}

		// 잘못된 명령어 - 적절하지 않은 시기의 명령어
		protected async Task WrongCommandTiming()
		{
			await Context.Channel.SendFileAsync("<@" + Context.User.Id + ">님! 지금 쓰기에는 조금 부적절한것 같은데요?");
		}

		// 잘못된 명령어 - 채널 전용 명령어
		protected async Task WrongCommandNotDM()
		{
			await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 그 명령어는 DM에서 치면 안되요!");
		}

		// 잘못된 명령어 - 잘못된 채널에서 명령어
		protected async Task WrongCommandDM()
		{
			await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 그 명령어는 DM에서만 사용 가능해요!");
		}

		// DM보내기
		protected async Task SendDM(ulong userId, string message)
		{
			await Context.Guild.GetUser(userId).SendMessageAsync(message);
		}

		// 타이머
		protected async Task Timer(int time)
		{
			RestUserMessage message = await Context.Channel.SendMessageAsync("Timer");
			isTimerStop = false;

			while (!isTimerStop && time > 0)
			{
				await message.ModifyAsync(m => m.Content = "남은 시간 `" + ConvertTime(time) + "`");
				time--;
				await Task.Delay(1000);
			}
			await message.ModifyAsync(m => m.Content = "남은 시간 `" + ConvertTime(0) + "`");
		}

		// 시간형태로 숫자 변환
		protected string ConvertTime(int time)
		{
			string stringTime = "";
			int m = time / 60;
			int s = time % 60;

			if (m >= 1)
			{
				stringTime += m.ToString();
				stringTime += ":";
			}
			if (s < 10)
			{
				stringTime += "0";
			}
			stringTime += s.ToString();

			return stringTime;
		}

		// @USER 을 userId로 변환
		protected ulong ConvertUserId(string input)
		{
			if (input.Length != 21)
			{
				return 0;
			}

			string userId = input.Substring(2, 18);

			return Convert.ToUInt64(userId);
		}

		// JobType을 String으로 변환
		protected string ConvertJob(JobType jobType)
		{
			string returnValue = "";

			switch (jobType)
			{
				case JobType.Citizen:
					returnValue = ":stuck_out_tongue_winking_eye: `시민`";
					break;
				case JobType.Cop:
					returnValue = ":cop: `경찰`";
					break;
				case JobType.Doctor:
					returnValue = ":ambulance: `의사`";
					break;
				case JobType.MOCongress:
					returnValue = ":moneybag: `국회의원`";
					break;
				case JobType.Mafia:
					returnValue = ":gun: `마피아`";
					break;
				case JobType.Wolf:
					returnValue = ":wolf: `늑대인간`";
					break;
				case JobType.Spy:
					returnValue = ":spy: `스파이`";
					break;
				default:
					returnValue = "직업이 없는디요?";
					break;
			}

			return returnValue;
		}
	}
}
