using System;
using System.Text;
using System.Collections.Generic;

namespace DiscordMafiaBot.Core.Commands
{
	public struct JobProcess
	{
		public Doctor doctor;
		public Cop cop;
		public Killer killer;
		public Wolf wolf;
		public Spy spy;
	}

	public class GameData
	{
		public JobProcess jobProcess;

		public int playerCount;			// 남아있는 사람
		public int currentDay = 0;		// 현재 날짜
		public List<ulong> livePlayer;  // 남은 플레이어들
		public int mafiaCount;          // 마피아 갯수


		public GameData(int _playerCount)
		{
			playerCount = _playerCount;
			livePlayer = new List<ulong>();

			jobProcess = new JobProcess();

			jobProcess.doctor = new Doctor();
			jobProcess.cop = new Cop();
			jobProcess.killer = new Killer();
			jobProcess.wolf = new Wolf();
			jobProcess.spy = new Spy();

			ResetJobProcess();
		}

		public void ResetJobProcess()
		{
			jobProcess.doctor.Reset();
			jobProcess.cop.Reset();
			jobProcess.killer.Reset();
			jobProcess.wolf.Reset();
			jobProcess.spy.Reset();
		}
	}
}
