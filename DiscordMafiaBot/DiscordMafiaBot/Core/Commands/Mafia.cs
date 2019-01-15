using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Discord;
using Discord.Commands;

namespace DiscordMafiaBot.Core.Commands
{
	public class Mafia : ModuleBase<SocketCommandContext>
	{
		[Command("join"), Summary("Mafia join command")]
		public async Task Join([Remainder]string input = "")
		{
			if (input != "")
			{
				await JoinPlayer(input, 0);
			}
			else
			{
				await JoinPlayer(Context.User.Username, Context.User.Id);

			}
		}

		private async Task JoinPlayer(string playerName, ulong userId)
		{
			Key key = new Key(playerName);

			if (!Program.playerList.ContainsKey(key))
			{
				Player player = new Player();
				player.userId = userId;

				Program.playerList.TryAdd(key, player);
				

				await Context.Channel.SendMessageAsync(playerName + "님을 리스트 등록에 **성공**하였습니다.");
			}
			else
			{
				await Context.Channel.SendMessageAsync(playerName + "님은 **이미 등록되어** 있습니다.");
			}
			
		}

		[Command("show list"), Summary("Mafia show list command")]
		private async Task ShowList()
		{
			EmbedBuilder embed = new EmbedBuilder();
			int num = 1;

			embed.WithColor(252, 138, 136);

			foreach (var keyValuePair in Program.playerList)
			{
				embed.Description += num.ToString() + ". " + keyValuePair.Key.name + "\n";
				num++;
			}

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}
	}
}
