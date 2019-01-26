using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordMafiaBot.Core.Commands
{
	public class DirectCommands : Mafia
	{
		// 직업 능력 사용
		[Command("shot"), Summary("Use job ability command")]
		public async Task Shot([Remainder]string input = null)
		{
			// DM에서만 사용 가능
			if (Context.Channel.GetType() != typeof(SocketDMChannel))
			{
				await WrongCommand(CommandType.DM);
				return;
			}

			// 밤에만 사용 가능
			if (gameStatus != GameStatus.Night)
			{
				await WrongCommand(CommandType.Timing);
				return;
			}

			ulong userId = Context.User.Id;
			ulong targetId;

			// 죽은사람이 사용
			if (playerList[userId].isDead)
			{
				await WrongCommand(CommandType.DiePlayer);
				return;
			}

			// 설명용 embed 생성
			EmbedBuilder embed = new EmbedBuilder();

			if (input == null)
			{
				targetId = Context.User.Id;
			}
			else
			{
				// 잘못된 입력
				if (int.Parse(input) > gameData.livePlayer.Count)
				{
					await WrongCommand(CommandType.Original);
					return;
				}

				targetId = gameData.livePlayer[int.Parse(input) - 1];
				
				// 죽은사람 지목
				if (playerList[targetId].isDead)
				{
					await WrongCommand(CommandType.ChooseDiePlayer);
					return;
				}
			}

			embed.WithColor(color);
			embed.Description = "이 명령어는 @player가 아닌 s.live에 나타난 `유저의 번호`로 대상을 지정합니다.\n" +
								"만약 `대상을 지정하지 않은` 경우 `자신에게 능력이 사용`되니 주의하세요!\n";
			await Context.Channel.SendMessageAsync("", false, embed.Build());

			JobType jobType = playerList[Context.User.Id].job;
			UserData userData = new UserData();

			userData.userId = userId;
			userData.targetId = targetId;

			switch (jobType)
			{
				case JobType.Citizen:
					await Context.Channel.SendMessageAsync("시민은 빠져있어, 뒤지기 싫으면");
					break;
				case JobType.Cop:
					await Context.Channel.SendMessageAsync(playerList[targetId].name + "님을 조사하기로 결정했습니다. \n\"뭔가.. 냄세가 나는데..?\"");

					// 대상 조사
					gameData.jobProcess.cop.AddUserData(userData);

					break;
				case JobType.Doctor:
					await Context.Channel.SendMessageAsync(playerList[targetId].name + "님을 살리기로 결정하였습니다. \n\"엉뚱한사람을 도왔다간.. 어떻게될지 몰라..\"");

					// 대상 살림
					gameData.jobProcess.doctor.AddUserData(userData);

					break;
				case JobType.MOCongress:
					await Context.Channel.SendMessageAsync("국회의원님, 역시 빡대가리라는걸 인증하시는군요!");
					break;
				case JobType.Mafia:
					await Context.Channel.SendMessageAsync(playerList[targetId].name + "님을 향해 총구를 겨눕니다.. \n\"예전부터.. 맘에 안들었어..\"");

					// 대상 죽임
					gameData.jobProcess.killer.AddUserData(userData);

					break;
				case JobType.Wolf:
					if (gameData.jobProcess.wolf.wolfLinked.Contains(targetId) && gameData.mafiaCount == 0)
					{
						await Context.Channel.SendMessageAsync(playerList[targetId].name + "님을 물어 뜯기로 결정했습니다! \n\"으르릉..\"");

						// 대상 죽임
						gameData.jobProcess.killer.AddUserData(userData);

					}
					else if (gameData.jobProcess.wolf.wolfLinked.Contains(targetId))
					{
						await Context.Channel.SendMessageAsync(playerList[targetId].name + "이미 접선 했잖아요! \n\"멍!\"");
					}
					else
					{
						await Context.Channel.SendMessageAsync(playerList[targetId].name + "님을 조사했습니다. \n\"킁킁\"");

						// 대상 조사
						gameData.jobProcess.wolf.AddUserData(userData);

					}
					break;
				case JobType.Spy:
					await Context.Channel.SendMessageAsync(playerList[targetId].name + "님의 집을 조사하기로 결정했습니다. \n\"이 집에는 누가살고있을까..?\"");

					// 대상 조사
					gameData.jobProcess.spy.AddUserData(userData);

					break;
				default:
					await Context.Channel.SendMessageAsync("이 메세지가 보이면 좆된겁니다. 개발자에게 문의하세요.");
					break;
			}
		}

		// 투표
		[Command("vote"), Summary("Vote command")]
		public async Task Vote([Remainder]string input = null)
		{
			// DM에서만 사용 가능
			if (Context.Channel.GetType() != typeof(SocketDMChannel))
			{
				await WrongCommand(CommandType.DM);
				return;
			}

			// 밤에만 사용 가능
			if (gameStatus != GameStatus.Vote)
			{
				await WrongCommand(CommandType.Timing);
				return;
			}

			// 죽은 플레이어
			if (playerList[Context.User.Id].isDead)
			{
				await WrongCommand(CommandType.DiePlayer);
				return;
			}
			
			// 입력 없을시
			if (input == null)
			{
				await WrongCommand(CommandType.Original);
				return;
			}

			// 잘못된 입력
			if (int.Parse(input) > gameData.livePlayer.Count)
			{
				await WrongCommand(CommandType.Original);
				return;
			}


			voteList[Context.User.Id] = gameData.livePlayer[int.Parse(input) - 1];
			await Context.Channel.SendMessageAsync(playerList[gameData.livePlayer[int.Parse(input) - 1]].name + "님을 투표하셨습니다!");
		}
	}
}
