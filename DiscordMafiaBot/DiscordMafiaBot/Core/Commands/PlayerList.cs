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
				await WrongCommand(CommandType.NotDM);
				return;
			}

			// 레디 상태일때만 사용 가능
			if (gameStatus != GameStatus.Ready)
			{
				await WrongCommand(CommandType.Timing);
				return;
			}

			// 디버깅이면
			if (debug)
			{
				await JoinPlayer(254872929395802112);
				await JoinPlayer(535203194121355284);
				await JoinPlayer(534781542380077066);
				await JoinPlayer(535203279827763251);
				await JoinPlayer(535405557620670466);

				return;
			}

			if (input != null)
			{
				ulong userId = ConvertUserId(input);

				// 유저 아이디가 올바르지 않음
				if (userId == 0)
				{
					await WrongCommand(CommandType.Original);
					return;
				}
				else
				{
					// 고른 유저가 봇임
					if (Context.Guild.GetUser(userId).IsBot)
					{
						await WrongCommand(CommandType.ChooseBot);
						return;
					}

					await JoinPlayer(userId);
				}
			}
			else
			{
				await JoinPlayer(Context.User.Id);
			}
		}

		// 참며 해제 명령어 s.out / s.out @player
		[Command("out"), Summary("Mafia leave command")]
		public async Task Leave([Remainder]string input = null)
		{
			// DM에서 사용 불가능
			if (Context.Channel.GetType() == typeof(SocketDMChannel))
			{
				await WrongCommand(CommandType.NotDM);
				return;
			}

			// 레디 상태일때만 사용 가능
			if (gameStatus != GameStatus.Ready)
			{
				await WrongCommand(CommandType.Timing);
				return;
			}

			if (input != null)
			{
				ulong userId = ConvertUserId(input);

				// 유저 아이디가 올바르지 않음
				if (userId == 0)
				{
					await WrongCommand(CommandType.Original);
					return;
				}
				else
				{
					// 고른 유저가 봇임
					if (Context.Guild.GetUser(userId).IsBot)
					{
						await WrongCommand(CommandType.ChooseBot);
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
			if (Context.Guild.GetUser(userId).Status == UserStatus.Offline)
			{
				await Context.Channel.SendMessageAsync("<@" + Context.User.Id + ">님! 쉴사람은 쉬자구요?");
				return;
			}

			if (!playerList.ContainsKey(userId))
			{
				Player player = new Player();
				player.name = Context.Guild.GetUser(userId).Username;
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
