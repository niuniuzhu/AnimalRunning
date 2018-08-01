using Logic.Event;
using View.Event;
using View.Misc;

namespace View
{
	public class CBattleEventHandler
	{
		private CBattle _battle;

		public CBattleEventHandler( CBattle battle )
		{
			this._battle = battle;

			EventCenter.AddListener( SyncEventType.GEN_MAZE, this.HandleGenMaze );
			EventCenter.AddListener( SyncEventType.DESTROY_BATTLE, this.HandleDestroyBattle );
			EventCenter.AddListener( SyncEventType.COUNT_DOWN, this.HandleCountDown );
			EventCenter.AddListener( SyncEventType.WIN, this.HandleWin );
			EventCenter.AddListener( SyncEventType.TERMINUS, this.HandleTerminus );
			EventCenter.AddListener( SyncEventType.ENTITY_CREATED, this.HandleEntityCreate );
			EventCenter.AddListener( SyncEventType.ENTITY_ADDED_TO_BATTLE, this.HandleEntityAddedToBattle );
			EventCenter.AddListener( SyncEventType.ENTITY_REMOVE_FROM_BATTLE, this.HandleEntityRemoveFromBattle );
			EventCenter.AddListener( SyncEventType.ENTITY_STATE_CHANGED, this.HandleEntityStateChanged );
			EventCenter.AddListener( SyncEventType.ENTITY_SYNC_PROPS, this.HandleEntitySyncProps );
			EventCenter.AddListener( SyncEventType.PICK_ITEM, this.HandlePickItem );
			EventCenter.AddListener( SyncEventType.USE_ITEM, this.HandleUseItem );
			EventCenter.AddListener( SyncEventType.BUFF_CREATED, this.HandleBuffCreated );
			EventCenter.AddListener( SyncEventType.BUFF_DESTROIED, this.HandleBuffDestroied );
			EventCenter.AddListener( SyncEventType.DEBUG_DRAW, this.HandleDebugDraw );
		}

		public void Dispose()
		{
			EventCenter.RemoveListener( SyncEventType.GEN_MAZE, this.HandleGenMaze );
			EventCenter.RemoveListener( SyncEventType.DESTROY_BATTLE, this.HandleDestroyBattle );
			EventCenter.RemoveListener( SyncEventType.COUNT_DOWN, this.HandleCountDown );
			EventCenter.RemoveListener( SyncEventType.WIN, this.HandleWin );
			EventCenter.RemoveListener( SyncEventType.TERMINUS, this.HandleTerminus );
			EventCenter.RemoveListener( SyncEventType.ENTITY_CREATED, this.HandleEntityCreate );
			EventCenter.RemoveListener( SyncEventType.ENTITY_ADDED_TO_BATTLE, this.HandleEntityAddedToBattle );
			EventCenter.RemoveListener( SyncEventType.ENTITY_REMOVE_FROM_BATTLE, this.HandleEntityRemoveFromBattle );
			EventCenter.RemoveListener( SyncEventType.ENTITY_STATE_CHANGED, this.HandleEntityStateChanged );
			EventCenter.RemoveListener( SyncEventType.ENTITY_SYNC_PROPS, this.HandleEntitySyncProps );
			EventCenter.RemoveListener( SyncEventType.PICK_ITEM, this.HandlePickItem );
			EventCenter.RemoveListener( SyncEventType.USE_ITEM, this.HandleUseItem );
			EventCenter.RemoveListener( SyncEventType.BUFF_CREATED, this.HandleBuffCreated );
			EventCenter.RemoveListener( SyncEventType.BUFF_DESTROIED, this.HandleBuffDestroied );
			EventCenter.RemoveListener( SyncEventType.DEBUG_DRAW, this.HandleDebugDraw );
		}

		private void HandleGenMaze( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandleGenMaze( e.walkables, e.i0, e.i1 );
		}

		private void HandleDestroyBattle( BaseEvent baseEvent )
		{
			this._battle.HandleDestroyBattle();
		}

		private void HandleCountDown( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			UIEvent.CountDown( e.i0, e.i1 );
			this._battle.HandleCountDown( e.i0, e.i1 );
		}

		private void HandleWin( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			UIEvent.Win( e.i0 );
			this._battle.HandleWin( e.i0 );
		}

		private void HandleTerminus( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandleTerminus( e.targetId );
		}

		private void HandleEntityCreate( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandleEntityCreate( e.entityType, e.param );
		}

		private void HandleEntityAddedToBattle( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandleEntityAddedToBattle( e.targetId );
		}

		private void HandleEntityRemoveFromBattle( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandleEntityRemoveFromBattle( e.targetId );
		}

		private void HandleEntityStateChanged( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandleEntityStateChanged( e.targetId, e.stateType, e.forceChange, e.stateParam );
		}

		private void HandleEntitySyncProps( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandleEntitySyncProps( e.targetId, e.attrs, e.attrValues, e.attrCount );
		}

		private void HandlePickItem( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandlePickItem( e.targetId, e.genericId );
		}

		private void HandleUseItem( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandleUseItem( e.targetId, e.b0 );
		}

		private void HandleBuffCreated( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandleBuffCreated( e.genericId, e.casterId, e.targetId );
		}

		private void HandleBuffDestroied( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandleBuffDestroied( e.genericId );
		}

		private void HandleDebugDraw( BaseEvent baseEvent )
		{
			SyncEvent e = ( SyncEvent )baseEvent;
			this._battle.HandleDebugDraw( e.debugDrawType, e.dv1.ToVector3(), e.dv2.ToVector3(), ( float )e.df, e.dc.ToColor() );
		}
	}
}