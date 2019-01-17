using System;
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
		[Command("help"), Summary("Show commands")]
		public async Task Help([Remainder]string Input = "None")
		{
			EmbedBuilder embed = new EmbedBuilder();
			EmbedFieldBuilder embedField;
			embed.WithColor(color);

			embedField = new EmbedFieldBuilder();
			embedField.Name = "게임 설정 명령어";
			embedField.Value = "`s.join / s.join @USER`\n" +
								"`s.out / s.out @USER`\n" +
								"`s.status`";
			embed.Fields.Add(embedField);

			embedField = new EmbedFieldBuilder();
			embedField.Name = "게임 플레이 명령어";
			embedField.Value = "`s.start`\n" +
								"`s.agree / s.disagree`\n" +
								"`s.live`\n" +
								"`s.skip`";
			embed.Fields.Add(embedField);

			embedField = new EmbedFieldBuilder();
			embedField.Name = "DM 명령어";
			embedField.Value = "`s.shot PLAYER_NUM`\n" +
								"`s.vote PLAYER_NUM`\n";
			embed.Fields.Add(embedField);

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}
	}
}
