using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Discord;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordMafiaBot.Core.Commands
{
	public abstract class Job
	{
		public ulong userId;
		public ulong targetId;

		public Job(ulong _userId, ulong _targetId) { targetId = _targetId; userId = _userId; }
		public abstract void UseAbility();
	}

	public class Doctor : Job
	{
		public Doctor(ulong _userId, ulong _targetId) : base(_userId, _targetId) { }
		public override void UseAbility()
		{

		}
	}

	public class Cop : Job
	{
		public Cop(ulong _userId, ulong _targetId) : base(_userId, _targetId) { }
		public override async void UseAbility()
		{
			// 마피아 팀이면
			if (Mafia.playerList[targetId].job >= JobType.Mafia)
			{
				await Mafia.mainGuild.GetUser(userId).SendMessageAsync(Mafia.playerList[targetId].name + "님은 마피아팀이었습니다!");
			}
			else
			{
				await Mafia.mainGuild.GetUser(userId).SendMessageAsync(Mafia.playerList[targetId].name + "님은 마피아팀이 아니었습니다..");
			}
		}
	}

	public class Killer : Job
	{
		public Killer(ulong _userId, ulong _targetId) : base(_userId, _targetId) { }
		public override void UseAbility()
		{
			
		}
	}

	public class Wolf : Job
	{
		public Wolf(ulong _userId, ulong _targetId) : base(_userId, _targetId) { }
		public override void UseAbility()
		{

		}
	}

	public class Spy : Job
	{
		public Spy(ulong _userId, ulong _targetId) : base(_userId, _targetId) { }
		public override void UseAbility()
		{

		}
	}
}
