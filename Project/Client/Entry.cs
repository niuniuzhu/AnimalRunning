using Client.Net;
using Client.UI;
using Client.UI.Wins;
using Core.Net;
using Game.Misc;
using Logic;
using System;
using System.Net.Sockets;
using UnityEngine;
using Logger = Core.Misc.Logger;

namespace Client
{
	public class Entry : MonoBehaviour
	{
		[HideInInspector]
		public string cid = "c0";
		[HideInInspector]
		public int image = 0;
		[HideInInspector]
		public string map = "m1";

		[HideInInspector]
		public bool useNetwork;
		[HideInInspector]
		public bool useKCP;
		[HideInInspector]
		public string ip = "127.0.0.1";
		[HideInInspector]
		public int port = 2553;

		[HideInInspector]
		public string logServerIp = "127.0.0.1";
		[HideInInspector]
		public int logServerPort = 23000;
		[HideInInspector]
		public LoggerProxy.LogLevel logLevel = LoggerProxy.LogLevel.All;

		void Start()
		{
			AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;

			Application.runInBackground = true;
			Application.targetFrameRate = 60;
			DontDestroyOnLoad( this.gameObject );

			Env.isEditor = Application.isEditor;
			Env.isRunning = true;
			Env.platform = ( int )Application.platform;
			Env.useNetwork = this.useNetwork;
			Env.StartTime();

			LoggerProxy.Init( Application.dataPath.Replace( "\\", "/" ) + "/../Log/", this.logServerIp, this.logServerPort );
			LoggerProxy.logLevel = this.logLevel;

			Defs.Init( Resources.Load<TextAsset>( "Defs/b_defs" ).text );

			UIManager.Init();

			if ( Env.useNetwork )
			{
				NetworkManager.SetupKCP();
				Windows.CONNECTING_WIN.Open( NetworkConfig.CONNECTION_TIMEOUT / 1000f );
				NetModule.Initialize( this.useKCP ? NetworkManager.PType.Kcp : NetworkManager.PType.Tcp );
				NetModule.instance.OnSocketEvent += this.OnSocketEvent;
				NetModule.instance.Connect( this.ip, this.port );
			}
			else
			{
				Standalone.Init( this.cid, ( byte )this.image, this.map );
				Standalone.Load();
			}
		}

		private void OnSocketEvent( SocketEvent e )
		{
			switch ( e.type )
			{
				case SocketEvent.Type.Connect:
					if ( e.errorCode == SocketError.Success )
					{
						Logger.Log( $"Connected to server {this.ip}:{this.port}" );
						Windows.CONNECTING_WIN.Hide();
						UIManager.EnterLogin();
					}
					else
					{
						string msg = $"Socket error, type:{e.type}, code:{e.errorCode}, msg:{e.msg}";
						Logger.Warn( msg );
						Windows.ALERT_WIN.Open( msg, this.OnConfirmDisconnected );
					}
					break;

				case SocketEvent.Type.Close:
					{
						string msg = $"Socket error, type:{e.type}, code:{e.errorCode}, msg:{e.msg}";
						Logger.Warn( msg );
						Windows.ALERT_WIN.Open( msg, this.OnConfirmDisconnected );
					}
					break;
			}
		}

		private void OnConfirmDisconnected()
		{
			if ( Env.isRunning )
			{
				Windows.CloseAll();
				UIManager.LeaveModule();
				Windows.CONNECTING_WIN.Open( NetworkConfig.CONNECTION_TIMEOUT / 1000f );
				NetModule.instance.Connect( this.ip, this.port );
			}
		}

		void Update()
		{
			if ( Env.useNetwork )
				NetModule.instance?.Update( ( long )( Time.deltaTime * 1000 ) );
			UIManager.Update();
			BattleManager.Update( Time.deltaTime );
		}

		void LateUpdate()
		{
			UIManager.LateUpdate();
			BattleManager.LateUpdate();
		}

		void OnDrawGizmos()
		{
			BattleManager.OnDrawGizmos();
		}

		void OnApplicationQuit()
		{
			Env.isRunning = false;
			if ( Env.useNetwork )
			{
				NetModule.instance.OnSocketEvent -= this.OnSocketEvent;
				NetModule.instance.Dispose();
			}
			else
				Standalone.Dispose();
			UIManager.Dispose();
			LoggerProxy.Dispose();
			AppDomain.CurrentDomain.UnhandledException -= this.OnUnhandledException;
		}

		private void OnUnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			Logger.Error( e.ExceptionObject.ToString() );
		}
	}
}