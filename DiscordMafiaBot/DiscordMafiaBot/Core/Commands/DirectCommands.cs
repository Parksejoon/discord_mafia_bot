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
				await WrongCommandDM();
				return;
			}

			// 밤에만 사용 가능
			if (gameStatus != GameStatus.Night)
			{
				await WrongCommandTiming();
				return;
			}

			EmbedBuilder embed = new EmbedBuilder();

			ulong userId;

			if (input == null)
			{
				embed.WithColor(color);
				embed.Description = "이 명령어는 @player가 아닌 s.status에 나타난 `유저의 번호`로 대상을 지정합니다.\n" +
									"만약 `대상을 지정하지 않으면 자신이 능력의 대상`이되니 주의하세요!\n" +
									"(대상을 지정하지 않아도 되는 능력 제외)\n";
				userId = Context.User.Id;
				await Context.Channel.SendMessageAsync("", false, embed.Build());
			}
			else
			{
				userId = playerList.ToArray()[int.Parse(input) - 1].Key;
			}

			JobType jobType = playerList[Context.User.Id].job;

			await Context.Channel.SendMessageAsync(mainGuild.GetUser(userId).Nickname + "");

			switch (jobType)
			{
				case JobType.Citizen:
					await Context.Channel.SendMessageAsync("어.. 시민이 무슨 능력이 있었나요?");
					break;
				case JobType.Cop:
					await Context.Channel.SendMessageAsync(mainChannel.GetUserAsync(userId) + "");
					break;
				case JobType.Doctor:
					await Context.Channel.SendMessageAsync(":ambulance: `의사`");
					break;
				case JobType.MOCongress:
					await Context.Channel.SendMessageAsync("국회의원은 가만히 있어도 되요..");
					break;
				case JobType.Mafia:
					await Context.Channel.SendMessageAsync(":gun: `마피아`");
					break;
				case JobType.Wolf:
					await Context.Channel.SendMessageAsync(":wolf: `늑대인간`");
					break;
				case JobType.Spy:
					await Context.Channel.SendMessageAsync(":spy: `스파이`");
					break;
				default:
					await Context.Channel.SendMessageAsync("직업이 없는디요?");
					break;
			}
		}
	}
}
