using System;
using System.Text;
using System.Collections.Generic;

namespace DiscordMafiaBot.Core.Commands
{
	public class GameData
	{
		public int playerCount;			// 남아있는 사람
		public int currentDay = 0;		// 현재 날짜
		public bool wolfLink = false;	// 늑대가 접선 했는지
		public List<ulong> livePlayer;	// 남은 플레이어들

		public GameData(int _playerCount)
		{
			playerCount = _playerCount;
			livePlayer = new List<ulong>();
		}
	}
}
