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
	public struct UserData
	{
		public ulong userId;
		public ulong targetId;
	}

	public abstract class Job
	{
		public List<UserData> userDatas;	// 유저 데이터 목록

		// 생성자
		protected Job()
		{
			userDatas = new List<UserData>();
		}

		// 리스트에 유저들로 하여금 능력을 사용
		public virtual void AbilityRoutine()
		{
			for (int i = 0; i < userDatas.Count; i++)
			{
				UseAbility(i);
			}
		}
		
		// 해당 인덱스의 유저id로 능력 사용
		protected abstract void UseAbility(int index);
	}

	public class Doctor : Job
	{
		// 살릴 사람 목록
		public List<ulong> reviveList;

		public Doctor() : base() { }

		// 살릴 사람들을 순서대로 살림
		public override void AbilityRoutine()
		{
			reviveList = new List<ulong>();
			base.AbilityRoutine();
		}
		
		// 해당 인덱스의 타겟 유저를 살림
		protected override void UseAbility(int index)
		{
			reviveList.Add(userDatas[index].targetId);
		}
	}

	public class Cop : Job
	{
		public Cop() : base() { }

		// 해당 인덱스 유저가 마피아팀인지 확인시켜줌
		protected override async void UseAbility(int index)
		{
			ulong targetId = userDatas[index].targetId;
			ulong userId = userDatas[index].userId;

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
		// 죽일 사람 목록
		public Dictionary<ulong, int> beKilledPlayers;

		public Killer() : base() { }

		// 죽일 사람들중 가장 많은 표를 받은사람을 죽임
		public override void AbilityRoutine()
		{
			beKilledPlayers = new Dictionary<ulong, int>();
			base.AbilityRoutine();
		}

		// 해당 인덱스의 유저를 죽임
		protected override void UseAbility(int index)
		{
			ulong targetId = userDatas[index].targetId;

			if (beKilledPlayers.ContainsKey(targetId))
			{
				beKilledPlayers[targetId]++;
			}
			else
			{
				beKilledPlayers[targetId] = 0;
			}
		}
	}

	public class Wolf : Job
	{
		// 늑대가 접선 했는지
		public Dictionary<ulong, bool> wolfLinked = new Dictionary<ulong, bool>();

		public Wolf() : base() { }

		// 해당 인덱스의 유저를 조사
		protected override void UseAbility(int index)
		{
			ulong targetId = userDatas[index].targetId;
			JobType targetJob = Mafia.playerList[targetId].job;

			//if (targetJob == JobType.Mafia || )
			//{

			//}
		}
	}

	public class Spy : Job
	{
		public Spy() : base() { }
		protected override async void UseAbility(int index)
		{
			ulong userId = userDatas[index].userId;
			ulong targetId = userDatas[index].targetId;
			JobType targetJob = Mafia.playerList[targetId].job;
			
			// 마피아이면
			if (targetJob == JobType.Mafia)
			{
				await Mafia.mainGuild.GetUser(userId).SendMessageAsync(Mafia.playerList[targetId].name + "님이 " + Mafia.ConvertJob(targetJob) + "인게 확인되어 접선을 성공했습니다! 이제부터 개인 DM으로 메세지를 주고받을 수 있습니다.");
			}
			else
			{
				await Mafia.mainGuild.GetUser(userId).SendMessageAsync(Mafia.playerList[userId].name + "님이 " + Mafia.ConvertJob(Mafia.playerList[userId].job) + "인게 확인되어 접선을 성공했습니다! 이제부터 개인 DM으로 메세지를 주고받을 수 있습니다.");
			}
		}
	}
}
