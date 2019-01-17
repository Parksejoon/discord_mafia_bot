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

			// 죽은사람이 사용
			if (playerList[Context.User.Id].isDead)
			{
				await WrongCommand(CommandType.DiePlayer);
				return;
			}

			// 설명용 embed 생성
			EmbedBuilder embed = new EmbedBuilder();
			ulong userId;

			if (input == null)
			{
				userId = Context.User.Id;
			}
			else
			{
				userId = livePlayer[int.Parse(input) - 1];

				// 죽은사람 지목
				if (playerList[userId].isDead)
				{
					await WrongCommand(CommandType.ChooseDiePlayer);
					return;
				}
			}

			embed.WithColor(color);
			embed.Description = "이 명령어는 @player가 아닌 s.status에 나타난 `유저의 번호`로 대상을 지정합니다.\n" +
								"만약 `대상을 지정하지 않은 경우 능력이 사용되지 않으니` 주의하세요!\n" +
								"(대상을 지정하지 않아도 되는 능력 제외)";
			await Context.Channel.SendMessageAsync("", false, embed.Build());

			JobType jobType = playerList[Context.User.Id].job;

			switch (jobType)
			{
				case JobType.Citizen:
					await Context.Channel.SendMessageAsync("어.. 시민이 무슨 능력이 있었나요?");
					break;
				case JobType.Cop:
					await Context.Channel.SendMessageAsync(playerList[userId].name + "님을 조사했습니다. 결과는 낮에 나오게 됩니다.");
					currentScene.copCheck = new KeyValuePair<ulong, ulong>(Context.User.Id, userId);
					break;
				case JobType.Doctor:
					await Context.Channel.SendMessageAsync(playerList[userId].name + "님을 살렸습니다. 과연 어떤 결과가 나올까요?");
					currentScene.doctorSaver = userId;
					break;
				case JobType.MOCongress:
					await Context.Channel.SendMessageAsync("국회의원은 가만히 있어도 되요..");
					break;
				case JobType.Mafia:
					await Context.Channel.SendMessageAsync(playerList[userId].name + "님을 향해 총구를 겨눕니다!");
					currentScene.mafiaKill = userId;
					break;
				case JobType.Wolf:
					if (wolfLink && Play.mafiaCount == 0)
					{
						await Context.Channel.SendMessageAsync(playerList[userId].name + "님을 물어 뜯기로 결정했습니다! 으르릉..");
						currentScene.wolfPick = new KeyValuePair<ulong, ulong>(Context.User.Id, userId);
					}
					else if (wolfLink)
					{
						await Context.Channel.SendMessageAsync(playerList[userId].name + "이미 접선 했잖아요! 멍!");
					}
					else
					{
						await Context.Channel.SendMessageAsync(playerList[userId].name + "님을 조사했습니다. 멍멍, 멍멍멍?");
						currentScene.wolfPick = new KeyValuePair<ulong, ulong>(Context.User.Id, userId);
					}
					break;
				case JobType.Spy:
					await Context.Channel.SendMessageAsync(playerList[userId].name + "님을 조사했습니다. 과연.. 그는 누구일까요?");
					currentScene.spyCheck = new KeyValuePair<ulong, ulong>(Context.User.Id, userId);
					break;
				default:
					await Context.Channel.SendMessageAsync("직업이 없는디요?");
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

			voteList[Context.User.Id] = livePlayer[int.Parse(input) - 1];
			await Context.Channel.SendMessageAsync(playerList[livePlayer[int.Parse(input) - 1]].name + "님을 투표하셨습니다!");
		}
	}
}
