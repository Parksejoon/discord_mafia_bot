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
	public class Player
	{
		public string job = null;
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

	public class Mafia : ModuleBase<SocketCommandContext>
	{
		public static ConcurrentDictionary<ulong, Player> playerList = new ConcurrentDictionary<ulong, Player>();
		public static ISocketMessageChannel mainChannel;
		public static bool isTimerRunning = true;
		
		// 상태 확인 명령어 s.state
		[Command("status"), Summary("Mafia show state command")]
		public async Task ShowStatus()
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithColor(252, 138, 136);

			// 현재 게임 진행 상태 및 채널 상태
			EmbedFieldBuilder embedField = new EmbedFieldBuilder();
			if (mainChannel == null)
			{
				embedField.Name = "설정된 메인 채널이 없습니다.";
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
			await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 봇전은 롤에서나 하세요!");
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

			while (isTimerRunning && time > 0)
			{
				await message.ModifyAsync(m => m.Content = "남은 시간 `" + ConvertTime(time) + "`");
				time--;
				await Task.Delay(1000);
			}
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
	}
}
