using Client.Net;
using Protocol.Gen;
using System.Collections.Generic;

namespace Client
{
	public static class FrameActionManager
	{
		private static readonly List<_DTO_action_info> ACTIONS = new List<_DTO_action_info>();

		public static void SetFrameAction( _DTO_action_info action )
		{
			ACTIONS.Add( action );
		}

		public static void SendActions( int frameId )
		{
			NetModule.instance.Send( ProtocolManager.PACKET_BATTLE_QCMD_ACTION( ACTIONS.ToArray(), frameId ) );
			ACTIONS.Clear();
		}
	}
}