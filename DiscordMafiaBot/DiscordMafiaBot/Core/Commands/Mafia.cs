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
	public class Scene
	{
		public ulong doctorSaver = 0;
		public ulong mafiaKill = 0;
		public KeyValuePair<ulong, ulong> wolfPick = new KeyValuePair<ulong, ulong>(0, 0);		// key = 늑대 value = 늑대가 고른 사람
		public KeyValuePair<ulong, ulong> copCheck = new KeyValuePair<ulong, ulong>(0, 0);      // key = 경찰 value = 경찰 지목자
		public KeyValuePair<ulong, ulong> spyCheck = new KeyValuePair<ulong, ulong>(0, 0);		// key = 스파이 value = 스파이 지목자
	}

	public class Times
	{
		public int day = 300;
		public int vote = 300;//60;
		public int judge = 300;//60;
		public int night = 300;//180;
	}
	
	public class Player
	{
		public JobType job = 0;
		public string name = "이게보인다면 좆된거에요 개발자한테 말하세요";
		public string ability = null;
		public bool isDead = false;
	}

	public enum CommandType
	{
		Original,
		ChooseBot,
		Timing,
		NotDM,
		DM,
		ChooseDiePlayer,
		DiePlayer
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
		public static ConcurrentDictionary<ulong, ulong> voteList;	// 투표 리스트 <투표한 플레이어, key가 투표한 플레이어>
		public static Color color = new Color(252, 138, 136);		// 시그니처 컬러
		public static GameStatus gameStatus = GameStatus.Ready;     // 게임 상태
		public static Times times = new Times();					// 시간 설정
		public static ISocketMessageChannel mainChannel;            // 메인 채널
		public static SocketGuild mainGuild;						// 메인 길드
		public static bool isTimerStop = false;                     // 타이머 정지 플래그
		public static Scene currentScene;                           // 현재 씬
		public static GameData gameData;							// 게임 데이터들

		// 상태 확인 명령어
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

			// 시간
			embedField = new EmbedFieldBuilder();
			embedField.Name = "시간 설정";

			embedField.Value += "**낮**\n`" + ConvertTime(times.day) + "`\n\n";
			embedField.Value += "**투표**\n`" + ConvertTime(times.vote) + "`\n\n";
			embedField.Value += "**찬반 투표**\n`" + ConvertTime(times.judge) + "`\n\n";
			embedField.Value += "**밤**\n`" + ConvertTime(times.night) + "`\n\n";

			embed.Fields.Add(embedField);

			// 현재 플레이어 목록
			embedField = new EmbedFieldBuilder();
			embedField.Name = "플레이어 목록";
			
			foreach (var keyValuePair in playerList)
			{
				embedField.Value += "<@" + keyValuePair.Key + "> ";
			}
			embed.Fields.Add(embedField);

			// 메세지 출력
			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		// 생존자 확인 명령어
		[Command("live")]
		public async Task LivePlayerList()
		{
			// 게임 플레이중에만 사용 가능
			if (gameStatus == GameStatus.Ready)
			{
				await WrongCommand(CommandType.Timing);
				return;
			}

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithColor(color);

			EmbedFieldBuilder embedField = new EmbedFieldBuilder();
			embedField.Name = "살아있는 플레이어 목록\n" +
								"플레이어_번호. @player";
			int num = 1;

			foreach (var player in gameData.livePlayer)
			{
				Console.WriteLine(player);
				embedField.Value += num.ToString() + ". <@" + player + ">\n";
				num++;
			}
			embed.Fields.Add(embedField);

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		// 잘못된 명령어
		protected async Task WrongCommand(CommandType commandType)
		{
			switch (commandType)
			{	
				case CommandType.Original:
					// 잘못된 명령어 - 일반
					await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 명령어를 잘못 치신것 같은데요?");
					break;
				case CommandType.ChooseBot:
					// 잘못된 명령어 - 봇을 대상으로 지정
					await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 마피아를 봇전으로 하는건 슬프지 않을까요?");
					break;
				case CommandType.Timing:
					// 잘못된 명령어 - 적절하지 않은 시기의 명령어
					await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 지금 쓰기에는 조금 부적절한것 같은데요?");
					break;
				case CommandType.NotDM:
					// 잘못된 명령어 - 채널 전용 명령어
					await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 그 명령어는 DM에서 치면 안되요!");
					break;
				case CommandType.DM:
					// 잘못된 명령어 - 잘못된 채널에서 명령
					await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 그 명령어는 DM에서만 사용 가능해요!");
					break;
				case CommandType.ChooseDiePlayer:
					// 잘못된 명령어 - 죽은사람 지목
					await Context.Channel.SendMessageAsync("그분은 죽은사람이에요..");
					break;
				case CommandType.DiePlayer:
					// 잘못된 명령어 - 죽은사람
					await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 죽었으면좀 가만히 있으라구요!");
					break;
				default:
					break;
			}
		}

		// DM보내기
		public async Task SendDM(ulong userId, string message)
		{
			if (userId == 0)
			{
				return;
			}

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
