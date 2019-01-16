using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Discord;
using Discord.Commands;

namespace DiscordMafiaBot.Core.Commands
{
	public class Player
	{
		public string job = null;
	}

	public class Mafia : ModuleBase<SocketCommandContext>
	{
		public static ConcurrentDictionary<ulong, Player> playerList = new ConcurrentDictionary<ulong, Player>();

		protected async Task WrongCommand()
		{
			await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 명령어를 잘못 치신것 같은데요?");
		}
	}
}
