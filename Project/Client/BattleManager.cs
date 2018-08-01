using Client.Net;
using Core.FMath;
using Core.Net.Protocol;
using Core.Structure;
using Logic;
using Logic.Event;
using Protocol.Gen;
using System.Diagnostics;
using System.Threading;
using View;

namespace Client
{
	public static class BattleManager
	{
		public static CBattle cBattle { get; private set; }

		public static Battle lBattle { get; private set; }

		private static bool _init;
		private static bool _shouldSendActions;
		private static int _sendActionFrame;
		private static Thread _logicThread;
		private static Fix64 _elapsedSinceLastLogicUpdate;
		private static Fix64 _elapsed;
		private static int _frameRate;
		private static int _framesPerKeyFrame;
		private static int _nextKeyFrame;
		private static readonly SwitchQueue<_DTO_frame_info> SERVER_KEYFRAMES = new SwitchQueue<_DTO_frame_info>();

		public static void Init( BattleParams param )
		{
			_init = true;
			_shouldSendActions = false;

			_elapsedSinceLastLogicUpdate = Fix64.Zero;
			_framesPerKeyFrame = param.framesPerKeyFrame;
			_frameRate = param.frameRate;
			_nextKeyFrame = _framesPerKeyFrame;

			cBattle = new CBattle( param );
			lBattle = new Battle( param );

			_logicThread = new Thread( LogicWorker );
			_logicThread.IsBackground = true;
			_logicThread.Start();

			if ( Env.useNetwork )
			{
				NetModule.instance.AddACMDListener( Module.BATTLE, Command.ACMD_BATTLE_START, HandleBattleStart );
				NetModule.instance.AddACMDListener( Module.BATTLE, Command.ACMD_FRAME, HandleFrame );
				NetModule.instance.AddACMDListener( Module.BATTLE, Command.ACMD_BATTLE_END, HandleBattleEnd );
				NetModule.instance.Send( ProtocolManager.PACKET_BATTLE_QCMD_BATTLE_CREATED() );
			}
		}

		public static void Dispose()
		{
			if ( !_init )
				return;

			_init = false;

			_elapsed = Fix64.Zero;
			_elapsedSinceLastLogicUpdate = Fix64.Zero;
			_shouldSendActions = false;
			_sendActionFrame = 0;
			SERVER_KEYFRAMES.Clear();

			lBattle.Dispose();
			EventCenter.Sync();
			cBattle.Dispose();

			lBattle = null;
			cBattle = null;

			_logicThread.Join();
			_logicThread = null;

			if ( Env.useNetwork )
			{
				NetModule.instance.RemoveACMDListener( Module.BATTLE, Command.ACMD_BATTLE_START, HandleBattleStart );
				NetModule.instance.RemoveACMDListener( Module.BATTLE, Command.ACMD_FRAME, HandleFrame );
				NetModule.instance.RemoveACMDListener( Module.BATTLE, Command.ACMD_BATTLE_END, HandleBattleEnd );
			}
		}

		private static void HandleBattleStart( Packet packet )
		{
		}

		private static void HandleBattleEnd( Packet packet )
		{
		}

		private static void HandleFrame( Packet packet )
		{
			SERVER_KEYFRAMES.Push( ( ( _PACKET_BATTLE_ACMD_FRAME )packet ).dto );
		}

		public static void Update( float deltaTime )
		{
			if ( !_init )
				return;
			if ( _shouldSendActions )
			{
				FrameActionManager.SendActions( _sendActionFrame );
				_shouldSendActions = false;
			}
			EventCenter.Sync();
			cBattle.Update( deltaTime );
		}

		public static void LateUpdate()
		{
			if ( !_init )
				return;
			cBattle.LateUpdate();
		}

		public static void OnDrawGizmos()
		{
			if ( !_init )
				return;
			cBattle.OnDrawGizmos();
		}

		private static void LogicWorker()
		{
			long millisecondsPreFrame = 1000 / _frameRate;
			long realCost = 0;
			long lastElapsedMilliseconds = 0;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			while ( lBattle != null )
			{
				if ( Env.useNetwork )
					UpdateLogic( ( Fix64 )( realCost * 0.001f ), ( Fix64 )( millisecondsPreFrame * 0.001f ) );
				else
					UpdateLogic2( ( Fix64 )( realCost * 0.001f ), ( Fix64 )( millisecondsPreFrame * 0.001f ) );
				Thread.Sleep( 10 );
				long elapsedMilliseconds = sw.ElapsedMilliseconds;
				realCost = elapsedMilliseconds - lastElapsedMilliseconds;
				lastElapsedMilliseconds = elapsedMilliseconds;
			}
			sw.Stop();
		}

		private static void UpdateLogic( Fix64 rdt, Fix64 fdt )
		{
			_elapsed += rdt;

			//如果本地frame比服务端慢，则需要快速步进追赶服务端帧数
			SERVER_KEYFRAMES.Switch();
			while ( !SERVER_KEYFRAMES.isEmpty )
			{
				_DTO_frame_info dto = SERVER_KEYFRAMES.Pop();
				int length = dto.frameId - lBattle.frame;
				while ( length >= 0 )
				{
					if ( length == 0 )
						HandleAction( dto );
					else
					{
						lBattle.Update( _elapsed, fdt );
						_elapsed = Fix64.Zero;
					}
					--length;
				}
				_nextKeyFrame = dto.frameId + _framesPerKeyFrame;
			}

			if ( lBattle.frame < _nextKeyFrame )
			{
				_elapsedSinceLastLogicUpdate += rdt;

				while ( _elapsedSinceLastLogicUpdate >= fdt )
				{
					if ( lBattle.frame >= _nextKeyFrame )
						break;

					lBattle.Update( _elapsed, fdt );

					if ( lBattle.frame == _nextKeyFrame )
					{
						_sendActionFrame = lBattle.frame;
						_shouldSendActions = true;
					}

					_elapsed = Fix64.Zero;
					_elapsedSinceLastLogicUpdate -= fdt;
				}
			}
		}

		private static void UpdateLogic2( Fix64 rdt, Fix64 fdt )
		{
			_elapsed += rdt;
			while ( _elapsed >= fdt )
			{
				lBattle.Update( _elapsed, fdt );
				_elapsed -= fdt;
			}
		}

		private static void HandleAction( _DTO_frame_info dto )
		{
			int count = dto.actions.Length;
			for ( int i = 0; i < count; i++ )
			{
				_DTO_action_info action = dto.actions[i];

				if ( action.sender == CPlayer.instance.rid )
					SyncEvent.HandleFrameAction();

				FrameActionType type = ( FrameActionType )action.type;
				switch ( type )
				{
					case FrameActionType.Move:
						{
							lBattle.HandleBeginMove( action.sender, new FVec3( action.x, action.y, action.z ) );
						}
						break;

					case FrameActionType.UseItem:
						{
							lBattle.HandleUseItem( action.sender );
						}
						break;
				}
			}
		}
	}
}