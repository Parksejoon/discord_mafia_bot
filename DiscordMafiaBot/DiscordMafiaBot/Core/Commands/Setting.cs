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
				await WrongCommandNotDM();
				return;
			}

			// 레디 상태에서만 사용 가능
			if (gameStatus != GameStatus.Ready)
			{
				await WrongCommandTiming();
				return;
			}

			mainChannel = Context.Channel;
			mainGuild = Context.Guild;

			await Context.Channel.SendMessageAsync("메인 채널이 `#" + mainChannel + "` 채널로 설정되었습니다.");
		}
	}
}
