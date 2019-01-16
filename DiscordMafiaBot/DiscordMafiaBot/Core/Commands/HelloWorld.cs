﻿using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Rest;
using Discord.Commands;

namespace DiscordMafiaBot.Core.Commands
{
	public class HelloWorld : Mafia
	{
		[Command("start")]
		public async Task TimerStart()
		{
			isTimerRunning = true;
			await Timer(10);
		}

		[Command("stop")]
		public async Task TimerStop()
		{
			isTimerRunning = false;
		}

		[Command("embed"), Summary("Embed test command")]
		public async Task Embed([Remainder]string Input = "None")
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithAuthor("Test embed", Context.User.GetAvatarUrl());
			embed.WithColor(40, 0, 10);
			embed.WithFooter("The footer of the embed", Context.Guild.Owner.GetAvatarUrl());
			embed.Description = "This is a **dummy** description, with a cool link.\n" +
								"[This is my favorite wesite](https://www.naver.com/)";
			embed.AddField("User input:", Input, true);

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}
	}
}
