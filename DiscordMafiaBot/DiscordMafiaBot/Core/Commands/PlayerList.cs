using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordMafiaBot.Core.Commands
{
	public class PlayerList : Mafia
	{
		// 참여 명령어 s.join / s.join @player
		[Command("join"), Summary("Mafia join command")]
		public async Task Join([Remainder]string input = null)
		{
			// DM에서 사용 불가능
			if (Context.Channel.GetType() == typeof(SocketDMChannel))
			{
				await WrongCommandNotDM();
				return;
			}

			// 레디 상태일때만 사용 가능
			if (gameStatus != GameStatus.Ready)
			{
				await WrongCommandTiming();
				return;
			}

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
					if (Context.Guild.GetUser(userId).IsBot)
					{
						await WrongCommandBot();
						return;
					}

					await JoinPlayer(userId);
				}
			}
			else
			{
				await JoinPlayer(Context.User.Id);
			}

			await ShowStatus();
		}

		// 참며 해제 명령어 s.out / s.out @player
		[Command("out"), Summary("Mafia leave command")]
		public async Task Leave([Remainder]string input = null)
		{
			// DM에서 사용 불가능
			if (Context.Channel.GetType() == typeof(SocketDMChannel))
			{
				await WrongCommandNotDM();
				return;
			}

			// 레디 상태일때만 사용 가능
			if (gameStatus != GameStatus.Ready)
			{
				await WrongCommandTiming();
				return;
			}

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
					if (Context.Guild.GetUser(userId).IsBot)
					{
						await WrongCommandBot();
						return;
					}

					await LeavePlayer(userId);
				}
			}
			else
			{
				await LeavePlayer(Context.User.Id);
			}

			await ShowStatus();
		}

		// 플레이어 참여
		private async Task JoinPlayer(ulong userId)
		{
			//if (Context.Guild.GetUser(userId).Status == UserStatus.Offline)
			//{
			//	await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 쉴사람은 쉬자구요?");
			//	return;
			//}

			if (!playerList.ContainsKey(userId))
			{
				Player player = new Player();
				playerList.TryAdd(userId, player);

				await Context.Channel.SendMessageAsync("<@" + userId + ">님을 성공적으로 등록시켰습니다!");
			}
			else
			{
				await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 두번 등록은 안된다구요?");
			}
		}

		// 플레이어 탈퇴
		private async Task LeavePlayer(ulong userId)
		{
			if (playerList.ContainsKey(userId))
			{
				Player player = new Player();
				playerList.TryRemove(userId, out player);

				await Context.Channel.SendMessageAsync("<@" + userId + ">님을 리스트에서 삭제했습니다!");
			}
			else
			{

				await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 리스트에도 없는사람을 어쩌실려구요!");
			}
		}
	}
}
