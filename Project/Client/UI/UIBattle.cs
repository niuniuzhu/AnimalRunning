using Client.Net;
using Core.FMath;
using Core.Math;
using Core.Net.Protocol;
using FairyUGUI.Core;
using FairyUGUI.Event;
using FairyUGUI.UI;
using Logic;
using Logic.Event;
using Logic.Misc;
using Protocol;
using Protocol.Gen;
using UnityEngine;
using UnityEngine.SceneManagement;
using View;
using View.Event;

namespace Client.UI
{
	public class UIBattle : IUIModule
	{
		private GComponent _root;
		private HUDManager _hudManager;
		private GestureStateOne _gestureStateOne;
		private bool _keyDown;
		private Vector2 _lastAxis;
		private GComponent _useItemBtn;
		private GComponent _winCom;

		public UIBattle()
		{
			UIPackage.AddPackage( "UI/battle" );

			UIObjectFactory.SetPackageItemExtension( UIPackage.GetItemURL( "battle", "Joystick" ), typeof( Joystick ) );
		}

		public void Dispose()
		{
		}

		public void Enter( object param )
		{
			BattleManager.Init( ( BattleParams )param );

			this._hudManager = new HUDManager();

			this._root = UIPackage.CreateObject( "battle", "Main" ).asCom;
			this._root.displayObject.name = "Battle";
			this._root.enableDrag = true;
			this._root.onDrag.Add( this.OnDrag );
			this._root.onTouchBegin.Add( this.OnTouchBegin );
			this._root.onTouchEnd.Add( this.OnTouchEnd );
			GRoot.inst.AddChild( this._root );
			this._root.size = GRoot.inst.size;
			this._root.AddRelation( GRoot.inst, RelationType.Size );

			this._gestureStateOne = new GestureStateOne( this );
			this._gestureStateOne.joystick = ( Joystick )this._root["joystick"];

			this._winCom = this._root["win_com"].asCom;
			this._winCom["n4"].onClick.Add( this.OnQuitBtnClick );

			this._useItemBtn = this._root["n11"].asCom;
			GComponent icon = this._useItemBtn["icon"].asCom;
			GLoader loader = icon["loader"].asLoader;
			loader.maskSprite = ( NSprite )UIPackage.GetItemAsset( "battle", "item_btn_mask" );

			EventCenter.AddListener( UIEventType.COUNT_DOWN, this.HandleCountDown );
			EventCenter.AddListener( UIEventType.WIN, this.HandleWin );
			EventCenter.AddListener( UIEventType.ENTITY_CREATED, this.HandleEntityCreated );
			EventCenter.AddListener( UIEventType.ENTITY_DESTROIED, this.HandleEntityDestroied );
			EventCenter.AddListener( UIEventType.ENTITY_ATTR_CHANGED, this.HandleEntityAttrChanged );
			EventCenter.AddListener( UIEventType.PICK_ITEM, this.HandlePickItem );
			EventCenter.AddListener( UIEventType.ITEM_USED, this.HandleItemUsed );

			if ( Env.useNetwork )
				NetModule.instance.AddQCMDListener( Module.BATTLE, Command.QCMD_LEAVE_BATTLE, this.HandleLeaveBattle );
		}

		public void Leave()
		{
			BattleManager.Dispose();

			SceneManager.LoadScene( 1, LoadSceneMode.Single );

			if ( Env.useNetwork )
				NetModule.instance.RemoveQCMDListener( Module.BATTLE, Command.QCMD_LEAVE_BATTLE, this.HandleLeaveBattle );

			EventCenter.RemoveListener( UIEventType.COUNT_DOWN, this.HandleCountDown );
			EventCenter.RemoveListener( UIEventType.WIN, this.HandleWin );
			EventCenter.RemoveListener( UIEventType.ENTITY_CREATED, this.HandleEntityCreated );
			EventCenter.RemoveListener( UIEventType.ENTITY_DESTROIED, this.HandleEntityDestroied );
			EventCenter.RemoveListener( UIEventType.ENTITY_ATTR_CHANGED, this.HandleEntityAttrChanged );
			EventCenter.RemoveListener( UIEventType.PICK_ITEM, this.HandlePickItem );
			EventCenter.RemoveListener( UIEventType.ITEM_USED, this.HandleItemUsed );

			this._useItemBtn.onClick.Remove( this.OnUseItemBtnClick );
			this._useItemBtn = null;

			this._winCom["n4"].onClick.Remove( this.OnQuitBtnClick );
			this._winCom = null;

			this._hudManager.Dispose();
			this._hudManager = null;

			this._gestureStateOne.Dispose();
			this._gestureStateOne = null;

			this._root.Dispose();
			this._root = null;
		}

		private void OnDrag( EventContext context )
		{
			PointerEventData e = ( PointerEventData )context.eventData;
			if ( e.pointerId == 0 )
				this._gestureStateOne.OnDrag( e.position );
		}

		private void OnTouchBegin( EventContext context )
		{
			PointerEventData e = ( PointerEventData )context.eventData;
			if ( e.pointerId == 0 )
				this._gestureStateOne.OnTouchBegin( e.position );
		}

		private void OnTouchEnd( EventContext context )
		{
			PointerEventData e = ( PointerEventData )context.eventData;
			if ( e.pointerId == 0 )
				this._gestureStateOne.OnTouchEnd( e.position );
		}

		private void OnUseItemBtnClick( EventContext context )
		{
			if ( CPlayer.instance == null ||
				 CPlayer.instance.mazeResult != Champion.MazeResult.Nan )
				return;
			this._useItemBtn.onClick.Remove( this.OnUseItemBtnClick );
			if ( Env.useNetwork )
				FrameActionManager.SetFrameAction( ProtocolManager.DTO_action_info( CPlayer.instance.rid, ( byte )FrameActionType.UseItem, string.Empty ) );
			else
				BattleManager.lBattle.HandleUseItem( CPlayer.instance.rid );
		}

		private void OnQuitBtnClick( EventContext context )
		{
			if ( Env.useNetwork )
			{
				this._root.ShowModalWait();
				NetModule.instance.Send( ProtocolManager.PACKET_BATTLE_QCMD_LEAVE_BATTLE() );
			}
			else
				Application.Quit();
		}

		public void Update()
		{
			this._hudManager.Update();
			this._gestureStateOne.Update();

			float hAxis = Input.GetAxisRaw( "Horizontal" );
			float vAxis = Input.GetAxisRaw( "Vertical" );
			if ( MathUtils.Abs( hAxis ) > 0f ||
				 MathUtils.Abs( vAxis ) > 0f )
			{
				this._keyDown = true;
				Vector2 axis = new Vector2( hAxis, vAxis );
				if ( axis != this._lastAxis )
				{
					this.HandleAxisInput( axis.normalized );
					this._lastAxis = axis;
				}
			}
			else if ( this._keyDown )
			{
				this._keyDown = false;
				this._lastAxis = Vector2.zero;
				this.HandleAxisInput( Vector2.zero );
			}
		}

		public void HandleAxisInput( Vector2 axis )
		{
			if ( CPlayer.instance == null ||
				 CPlayer.instance.mazeResult != Champion.MazeResult.Nan )
				return;
			axis.x = ( int )( axis.x * 100f ) * 0.01f;
			axis.y = ( int )( axis.y * 100f ) * 0.01f;
			if ( Env.useNetwork )
			{
				_DTO_action_info dto = ProtocolManager.DTO_action_info( CPlayer.instance.rid, ( byte )FrameActionType.Move, axis.x, 0f, axis.y );
				FrameActionManager.SetFrameAction( dto );
			}
			else
				BattleManager.lBattle.HandleBeginMove( CPlayer.instance.rid, new FVec3( axis.x, 0, axis.y ) );
		}

		private void HandleCountDown( BaseEvent baseEvent )
		{
			UIEvent e = ( UIEvent )baseEvent;
			this._root["count_down"].asTextField.text = string.Empty + ( e.i1 - e.i0 );
			if ( e.i0 == e.i1 )
			{
				this._root["count_down"].Dispose();
				GTextField goCom = this._root["go_text"].asTextField;
				goCom.visible = true;
				Transition transition = this._root.GetTransition( "t1" );
				transition.SetCompleteCallback( () => goCom.Dispose() );
				transition.Play();
			}
		}

		private void HandleWin( BaseEvent e )
		{
			this._winCom.visible = true;
		}

		private void HandleEntityCreated( BaseEvent e )
		{
			UIEvent uiEvent = ( UIEvent )e;
			this._hudManager.OnEntityCreated( uiEvent.target );
		}

		private void HandleEntityDestroied( BaseEvent e )
		{
			UIEvent uiEvent = ( UIEvent )e;
			this._hudManager.OnEntityDestroied( uiEvent.target );
		}

		private void HandleEntityAttrChanged( BaseEvent e )
		{
			UIEvent uiEvent = ( UIEvent )e;
			this._hudManager.OnEntityAttrChanged( uiEvent.target, uiEvent.attr, uiEvent.o0 );
		}

		private void HandlePickItem( BaseEvent e )
		{
			UIEvent uiEvent = ( UIEvent )e;
			if ( uiEvent.target != CPlayer.instance )
				return;
			GComponent icon = this._useItemBtn["icon"].asCom;
			GLoader loader = icon["loader"].asLoader;
			loader.url = UIPackage.GetItemURL( "battle", Utils.GetIDFromRID( uiEvent.itemId ) );
			this._useItemBtn.onClick.Add( this.OnUseItemBtnClick );
		}

		private void HandleItemUsed( BaseEvent e )
		{
			UIEvent uiEvent = ( UIEvent )e;
			if ( uiEvent.target != CPlayer.instance )
				return;
			bool result = uiEvent.b0;
			if ( result )
			{
				GComponent icon = this._useItemBtn["icon"].asCom;
				GLoader loader = icon["loader"].asLoader;
				loader.url = string.Empty;
			}
			else
				this._useItemBtn.onClick.Add( this.OnUseItemBtnClick );
		}

		private void HandleLeaveBattle( Packet packet )
		{
			this._root.CloseModalWait();

			_DTO_reply dto = ( ( _PACKET_GENERIC_ACMD_REPLY )packet ).dto;
			PResult result = ( PResult )dto.result;
			PResultUtils.ShowAlter( result );
			if ( result == PResult.SUCCESS )
				UIManager.EnterHall();
		}
	}
}