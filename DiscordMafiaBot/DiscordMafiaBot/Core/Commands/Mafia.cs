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
	public class PlayerList : ModuleBase<SocketCommandContext>
	{
		[Command("join"), Summary("Mafia join command")]
		public async Task Join([Remainder]string input = null)
		{
			if (input != null)
			{
				ulong userId = ConvertUserId(input);

				if (userId == 0)
				{
					await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 명령어를 잘못 치신것 같은데요?");
				}
				else
				{
					await JoinPlayer(userId);
				}
			}
			else
			{
				await JoinPlayer(Context.User.Id);
			}
		}

		[Command("list"), Summary("Mafia show list command")]
		private async Task ShowList()
		{
			EmbedBuilder embed = new EmbedBuilder();
			int num = 1;

			embed.WithColor(252, 138, 136);

			foreach (var keyValuePair in Program.playerList)
			{
				embed.Description += num.ToString() + ". <@" + keyValuePair.Key + ">\n";
				num++;
			}

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		private async Task JoinPlayer(ulong userId)
		{
			if (!Program.playerList.ContainsKey(userId))
			{
				Player player = new Player();

				Program.playerList.TryAdd(userId, player);

				await Context.Channel.SendMessageAsync("<@" + userId + ">님을 리스트 등록에 **성공**하였습니다.");
			}
			else
			{
				await Context.Channel.SendMessageAsync("<@" + userId + ">님은 **이미 등록되어** 있습니다.");
			}
		}

		private ulong ConvertUserId(string input)
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
