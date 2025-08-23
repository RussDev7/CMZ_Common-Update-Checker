using System;
using System.Collections.Generic;
using System.Threading;
using DNA.Net.MatchMaking;

namespace DNA.Net.GamerServices.LidgrenProvider
{
	public class LidgrenNetworkSessionStaticProvider : NetworkSessionStaticProvider
	{
		protected override NetworkSession CreateSession()
		{
			return LidgrenNetworkSessionProvider.CreateNetworkSession(this);
		}

		protected override void FinishBeginCreate(NetworkSessionStaticProvider.BeginCreateSessionState state)
		{
			base.TaskScheduler.QueueUserWorkItem(new ParameterizedThreadStart(this.CreateSessionsThread), state);
		}

		protected virtual void CreateSessionsThread(object state)
		{
			NetworkSessionStaticProvider.BeginCreateSessionState sqs = (NetworkSessionStaticProvider.BeginCreateSessionState)state;
			sqs.Session.StartHost(sqs);
			sqs.Event.Set();
			if (sqs.Callback != null)
			{
				sqs.Callback(sqs);
			}
		}

		protected override void FinishBeginJoin(NetworkSessionStaticProvider.BeginJoinSessionState state)
		{
			base.TaskScheduler.QueueUserWorkItem(new ParameterizedThreadStart(this.BeginJoinThread), state);
		}

		protected virtual void BeginJoinThread(object state)
		{
			NetworkSessionStaticProvider.BeginJoinSessionState sqs = (NetworkSessionStaticProvider.BeginJoinSessionState)state;
			sqs.Session.StartClient(sqs);
			while ((sqs.Session.HostConnectionResult != NetworkSession.ResultCode.Succeeded || sqs.Session.LocalGamers.Count <= 0) && sqs.Session.HostConnectionResult <= NetworkSession.ResultCode.Succeeded)
			{
				Thread.Sleep(100);
				sqs.Session.Update();
			}
			sqs.HostConnectionResult = sqs.Session.HostConnectionResult;
			sqs.HostConnectionResultString = sqs.Session.HostConnectionResultString;
			sqs.Event.Set();
			if (sqs.Callback != null)
			{
				sqs.Callback(sqs);
			}
		}

		public override IAsyncResult BeginJoinInvited(ulong lobbyId, int version, string gameName, IEnumerable<SignedInGamer> localGamers, AsyncCallback callback, object asyncState, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			throw new NotImplementedException("Begin join not yet implemented");
		}

		protected override void FinishBeginFind(NetworkSessionStaticProvider.SessionQueryState state)
		{
			base.TaskScheduler.QueueUserWorkItem(new ParameterizedThreadStart(this.FindAvailableSessionsThread), state);
		}

		protected virtual void FindAvailableSessionsThread(object state)
		{
			NetworkSessionStaticProvider.SessionQueryState sqs = (NetworkSessionStaticProvider.SessionQueryState)state;
			List<AvailableNetworkSession> sessions = new List<AvailableNetworkSession>();
			try
			{
				ClientSessionInfo[] clientSessions = this.NetworkSessionServices.QueryClientInfo(sqs.SearchProperties);
				foreach (ClientSessionInfo info in clientSessions)
				{
					sessions.Add(new AvailableNetworkSession(info));
				}
			}
			catch
			{
			}
			sqs.Sessions = new AvailableNetworkSessionCollection(sessions);
			sqs.Event.Set();
			if (sqs.Callback != null)
			{
				sqs.Callback(sqs);
			}
		}

		public override HostDiscovery GetHostDiscoveryObject(string gamename, int version, PlayerID playerID)
		{
			return new LidgrenHostDiscovery(gamename, version, playerID);
		}
	}
}
