using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordMafiaBot.Core.Commands
{
	public class Setting : Mafia
	{
		[Command("setch"), Summary("Setting channel command")]
		public async Task SetChannel([Remainder]string input = null)
		{
			// DM에서 사용 불가능
			if (Context.Channel.GetType() == typeof(SocketDMChannel))
			{
				await WrongCommand(CommandType.NotDM);
				return;
			}

			// 레디 상태에서만 사용 가능
			if (gameStatus != GameStatus.Ready)
			{
				await WrongCommand(CommandType.Timing);
				return;
			}

			mainChannel = Context.Channel;
			mainGuild = Context.Guild;

			await Context.Channel.SendMessageAsync("메인 채널이 `#" + mainChannel + "` 채널로 설정되었습니다.");
		}

		[Command("settime"), Summary("Setting times command")]
		public async Task SetTimes([Remainder]string input = null)
		{
			string[] command = input.Split(" ");
			int time = int.Parse(command[1]);

			switch (command[0])
			{
				case "day":
					times.day = time;
					await Context.Channel.SendMessageAsync("`낮`시간이 `" + ConvertTime(time) + "`으로 변경되었습니다.");
					break;
				case "vote":
					times.vote = time;
					await Context.Channel.SendMessageAsync("`투표`시간이 `" + ConvertTime(time) + "`으로 변경되었습니다.");
					break;
				case "judge":
					times.judge = time;
					await Context.Channel.SendMessageAsync("`찬반 투표`시간이 `" + ConvertTime(time) + "`으로 변경되었습니다.");
					break;
				case "night":
					times.night = time;
					await Context.Channel.SendMessageAsync("`밤`시간이 `" + ConvertTime(time) + "`으로 변경되었습니다.");
					break;
				default:
					break;
			}
		}
	}
}
