using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

namespace DiscordMafiaBot.Core.Commands
{
	public class HelloWorld : ModuleBase<SocketCommandContext>
	{
		[Command("hello"), Alias("helloworld", "world"), Summary("Hello World command")]
		public async Task Sjutein()
		{
			await Context.Channel.SendMessageAsync("Hello world");
		}
	}
}
