using Core.Misc;
using Core.Net;
using Core.Net.Protocol;
using Protocol.Gen;
using System;
using System.Reflection;

namespace SocketTest
{
	public class TCPTest
	{
		private const string NETWORK_NAME = "client";

		public TCPTest()
		{
			AssemblyName[] assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
			foreach ( AssemblyName assembly in assemblies )
				Assembly.Load( assembly );

			Logger.logAction = Console.WriteLine;
			Logger.infoAction = Console.WriteLine;
			Logger.netAction = Console.WriteLine;
			Logger.warnAction = Console.WriteLine;
			Logger.errorAction = Console.WriteLine;
			Logger.factalAction = Console.WriteLine;

			NetworkManager.SetupKCP();
			NetworkManager.CreateClient( NETWORK_NAME, NetworkManager.PType.Tcp );
			NetworkManager.AddClientEventHandler( NETWORK_NAME, this.ProcessClientEvent );
			NetworkManager.Connect( NETWORK_NAME, "127.0.0.1", 2551 );
		}

		private void ProcessClientEvent( SocketEvent e )
		{
			switch ( e.type )
			{
				case SocketEvent.Type.Close:
					this.HandleClientClosed( e.msg );
					break;

				case SocketEvent.Type.Receive:
					this.ProcessDataReceived( e.packet );
					break;

				case SocketEvent.Type.Connect:
					break;
			}
		}

		public void Update( long dt )
		{
			NetworkManager.Update( dt );
			if ( Program.currentKey == ConsoleKey.A )
			{
				NetworkManager.Send( NETWORK_NAME, ProtocolManager.PACKET_USER_QCMD_LOGIN( "user", "password" ) );
			}
		}

		public void Dispose()
		{
			NetworkManager.Dispose();
		}

		public void Send( Packet packet )
		{
			Logger.Net( $"<color=\"#CCFF33\">Send [packet:{packet}], time:{TimeUtils.utcTime}</color>" );

			NetworkManager.Send( NETWORK_NAME, packet );
		}

		private void HandleClientClosed( string message )
		{
			Logger.Net( $"Socket closed:{message}" );
		}

		private void ProcessDataReceived( Packet packet )
		{
			Logger.Net( $"<color=\"#00FFFF\">Received [packet:{packet}], time:{TimeUtils.GetLocalTime( TimeUtils.utcTime )}</color>" );
		}
	}
}