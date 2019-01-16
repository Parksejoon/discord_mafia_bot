using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

namespace DiscordMafiaBot.Core.Commands
{
	public class PlayerList : Mafia
	{
		// 참여 명령어 s.join / s.join @player
		[Command("join"), Summary("Mafia join command")]
		public async Task Join([Remainder]string input = null)
		{
			if (input != null)
			{
				ulong userId = ConvertUserId(input);

				if (userId == 0)
				{
					await WrongCommand();
					return;
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

			await ShowList();
		}

		// 참며 해제 명령어 s.out / s.out @player
		[Command("out"), Summary("Mafia leave command")]
		public async Task leave([Remainder]string input = null)
		{
			if (input != null)
			{
				ulong userId = ConvertUserId(input);

				if (userId == 0)
				{
					await WrongCommand();
					return;
				}
				else
				{
					await LeavePlayer(userId);
				}
			}
			else
			{
				await LeavePlayer(Context.User.Id);
			}

			await ShowList();
		}

		// 리스트 확인 명령어 s.list
		[Command("list"), Summary("Mafia show list command")]
		private async Task ShowList()
		{
			EmbedBuilder embed = new EmbedBuilder();
			int num = 1;

			embed.WithColor(252, 138, 136);

			foreach (var keyValuePair in playerList)
			{
				embed.Description += num.ToString() + ". <@" + keyValuePair.Key + ">\n";
				num++;
			}

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		// 플레이어 참여
		private async Task JoinPlayer(ulong userId)
		{
			if (!playerList.ContainsKey(userId))
			{
				Player player = new Player();
				playerList.TryAdd(userId, player);

				await Context.Channel.SendMessageAsync("<@" + userId + ">님을 리스트 등록에 **성공**하였습니다.");
			}
			else
			{
				await Context.Channel.SendMessageAsync("<@" + userId + ">님은 **이미 등록되어** 있습니다.");
			}
		}

		// 플레이어 탈퇴
		private async Task LeavePlayer(ulong userId)
		{
			if (playerList.ContainsKey(userId))
			{
				Player player = new Player();
				playerList.TryRemove(userId, out player);

				await Context.Channel.SendMessageAsync("<@" + userId + ">님을 리스트에서 **삭제**하였습니다.");
			}
			else
			{

				await Context.Channel.SendMessageAsync("<@" + userId + ">님은 **등록되어있지 않습니다.** ");
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
