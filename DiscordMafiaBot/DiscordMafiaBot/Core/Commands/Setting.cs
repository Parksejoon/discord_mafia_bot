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
			mainChannel = Context.Channel;

			await Context.Channel.SendMessageAsync("메인 채널이 `#" + mainChannel + "` 채널로 설정되었습니다.");
		}

		[Command("pick"), Summary("Pick command")]
		public async Task Pick([Remainder]string input = null)
		{
			if (Context.Channel.GetType() == typeof(SocketDMChannel))
			{
				await mainChannel.SendMessageAsync("앙 기모띠!");
			}
		}
	}
}
