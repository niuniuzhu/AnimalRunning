using Client.Net;
using FairyUGUI.UI;
using Game.Loader;
using Game.Task;
using Logic;
using Protocol;
using Protocol.Gen;
using System.Collections;
using Core.Net.Protocol;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client.UI
{
	public class UILoadLevel : IUIModule
	{
		private LoadBatch _lb;
		private GComponent _root;

		public UILoadLevel()
		{
			UIPackage.AddPackage( "UI/cutscene" );
		}

		public void Dispose()
		{
		}

		public void Enter( object param )
		{
			NetModule.instance.AddACMDListener( Module.BATTLE, Command.ACMD_ENTER_BATTLE, this.OnEnterBattle );
			NetModule.instance.AddQCMDListener( Module.ROOM, Command.QCMD_MAP_READY, this.OnMapReadyResult );

			this._root = UIPackage.CreateObject( "cutscene", "Main" ).asCom;
			GRoot.inst.AddChild( this._root );
			this._root.size = GRoot.inst.size;

			GProgressBar bar = this._root["bar"].asProgress;
			bar.value = 0;
			bar.minValue = 0;
			bar.maxValue = 1;

			//preload here
			_DTO_begin_fight dto = ( _DTO_begin_fight )param;
			SyncTask.Create( this.StartLoad( dto ) );
		}

		public void Leave()
		{
			NetModule.instance.RemoveACMDListener( Module.BATTLE, Command.ACMD_ENTER_BATTLE, this.OnEnterBattle );
			NetModule.instance.RemoveQCMDListener( Module.ROOM, Command.QCMD_MAP_READY, this.OnMapReadyResult );

			if ( this._lb != null )
			{
				this._lb.Cancel();
				this._lb = null;
			}

			if ( this._root != null )
			{
				this._root.Dispose();
				this._root = null;
			}
		}

		private IEnumerator StartLoad( _DTO_begin_fight dto )
		{
			GProgressBar bar = this._root["bar"].asProgress;
			AsyncOperation ao = SceneManager.LoadSceneAsync( "maze", LoadSceneMode.Single );
			while ( !ao.isDone )
			{
				bar.value = ao.progress;
				yield return 0;
			}
			bar.value = 1f;
			NetModule.instance.Send( ProtocolManager.PACKET_ROOM_QCMD_MAP_READY() );
		}

		public void Update()
		{
		}

		private void OnEnterBattle( Packet packet )
		{
			_DTO_enter_battle dto = ( ( _PACKET_BATTLE_ACMD_ENTER_BATTLE )packet ).dto;
			BattleParams param;
			param.frameRate = dto.frameRate;
			param.framesPerKeyFrame = dto.framesPerKeyFrame;
			param.uid = dto.uid;
			param.id = dto.mapId;
			param.rndSeed = dto.rndSeed;
			int count = dto.players.Length;
			param.players = new BattleParams.Player[count];
			for ( int i = 0; i < count; i++ )
			{
				_DTO_player_info playerInfoDTO = dto.players[i];
				BattleParams.Player p;
				p.id = playerInfoDTO.uid;
				p.cid = playerInfoDTO.cid;
				p.name = playerInfoDTO.name;
				p.skin = playerInfoDTO.skin;
				p.team = playerInfoDTO.team;
				param.players[i] = p;
			}

			UIManager.EnterBattle( param );
		}

		private void OnMapReadyResult( Packet packet )
		{
			_DTO_reply dto = ( ( _PACKET_GENERIC_ACMD_REPLY )packet ).dto;
			PResult result = ( PResult )dto.result;
			PResultUtils.ShowAlter( result );
		}
	}
}