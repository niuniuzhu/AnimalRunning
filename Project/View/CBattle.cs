using Core.FMath;
using Game.Misc;
using Logic;
using Logic.Event;
using Logic.FSM;
using System.Collections.Generic;
using UnityEngine;
using View.Event;
using View.Graphics;
using View.Misc;

namespace View
{
	public class CBattle
	{
		public Fix64 FOWFogFrequency => this._data.FOWFogFrequency;
		public Fix64 FOWFogAmplitude => this._data.FOWFogAmplitude;
		public Fix64 FOWDistanceToPlayer => this._data.FOWDistanceToPlayer;
		public string surfaceMat => this._data.surfaceMat;

		public int frame { get; private set; }
		public Fix64 deltaTime { get; private set; }
		public Fix64 time { get; private set; }
		public CCamera camera { get; private set; }

		private string _uid;
		private MapData _data;
		private readonly GMaze _maze;
		private readonly UpdateContext _context;
		private readonly CBattleEventHandler _eventHandler;
		private readonly GameObjectManager _gameObjectManager;
		private readonly FxManager _fxManager;
		private readonly CBuffManager _buffManager;
		private readonly CEntityManager _entityManager;
		private readonly DebugDrawer _debugDrawer;
		private CTerminus _terminus;

		public CBattle( BattleParams param )
		{
			this._uid = param.uid;
			this._data = ModelFactory.GetMapData( Utils.GetIDFromRID( param.id ) );

			this._maze = new GMaze( this, this._data.scale.ToVector3(), this._data.offset.ToVector3(), this._data.row,
									this._data.col );
			this._context = new UpdateContext();
			this._eventHandler = new CBattleEventHandler( this );
			this._gameObjectManager = new GameObjectManager();
			this._fxManager = new FxManager( this );
			this._buffManager = new CBuffManager( this );
			this._entityManager = new CEntityManager( this );
			this._debugDrawer = new DebugDrawer();

			this.camera = new CCamera( this._data );
			this.camera.enable = false;
			//this.camera.SetConstraints(
			//	new Vector3( 0, 0, -this._maze.row * this._maze.scale.z ),
			//	new Vector3( this._maze.col * this._maze.scale.x, 0, 0 ) );
		}

		public void Dispose()
		{
			this.camera.Dispose();
			this.camera = null;
			this._maze.Dispose();
			this._fxManager.Dispose();
			this._buffManager.Dispose();
			this._entityManager.Dispose();
			this._gameObjectManager.Dispose();
			this._eventHandler.Dispose();
			this._data = null;
			CPlayer.instance = null;
			this._terminus = null;
		}

		public CEffect CreateEffect( string id, IEffectHolder holder, CEntity caster, CEntity target )
		{
			return this._fxManager.CreateEffect( id, holder, caster, target );
		}

		public GameObject GetModel( string id )
		{
			return this._gameObjectManager.Pop( id );
		}

		public void PushModel( GameObject go )
		{
			this._gameObjectManager.Push( go );
		}

		public void GetChampionsNearby( CChampion target, TargetType targetType, float radius, int maxNumber, ref List<CChampion> champions )
		{
			this._entityManager.GetChampionsNearby( target, targetType, radius, maxNumber, ref champions );
		}

		public void HandleGenMaze( int[] walkables, int startIndex, int endIndex )
		{
			this._maze.Set( walkables );
		}

		public void HandleDestroyBattle()
		{
		}

		public void HandleCountDown( int num, int countDown )
		{
			this.camera.PlayOneShot( this._data.countDownSnd, 1 );
		}

		public void HandleWin( int team )
		{
			this.camera.PlayOneShot( team == CPlayer.instance.team ? this._data.winSnd : this._data.loseSnd, 1 );
			this.camera.smoothTime = 0.8f;
			this.camera.target = this._terminus.graphic.root;
			this._maze.Bright();
		}

		public void HandleTerminus( string targetId )
		{
			this._terminus.Bright();
		}

		public void HandleEntityCreate( string type, EntityParam param )
		{
			switch ( type )
			{
				case "Champion":
					CEntity entity = this._uid == param.uid
										 ? this._entityManager.Create<CPlayer>( param )
										 : this._entityManager.Create<CChampion>( param );
					break;

				case "Rail":
					this._entityManager.Create<CRail>( param );
					break;

				case "Item":
					this._entityManager.Create<CItem>( param );
					break;

				case "Terminus":
					this._terminus = this._entityManager.Create<CTerminus>( param );
					break;
			}
		}

		public void HandleEntityAddedToBattle( string rid )
		{
			CEntity entity = this._entityManager.GetEntity( rid );
			entity.OnAddedToBattle();
			if ( entity is CPlayer )
			{
				this.camera.enable = true;
				this.camera.target = CPlayer.instance.graphic.root;
				this.camera.UpdateVisualImmediately();
				this.camera.PlayDelay( this._data.bgSnd, 3f );
			}

			UIEvent.EntityCreated( entity );
		}

		public void HandleEntityRemoveFromBattle( string rid )
		{
			CEntity entity = this._entityManager.GetEntity( rid );
			entity.OnRemoveFromBattle();
		}

		public void HandleEntityStateChanged( string rid, FSMStateType type, bool force, object[] param )
		{
			CChampion entity = this._entityManager.GetChampion( rid );
			entity.HandleEntityStateChanged( type, force, param );
		}

		public void HandleEntitySyncProps( string rid, Attr[] attrs, object[] values, int count )
		{
			CEntity entity = this._entityManager.GetEntity( rid );
			entity.HandleSyncProps( attrs, values, count );
		}

		public void HandlePickItem( string targetId, string itemId )
		{
			CChampion entity = this._entityManager.GetChampion( targetId );
			UIEvent.PickItem( entity, itemId );
		}

		public void HandleUseItem( string targetId, bool result )
		{
			CChampion entity = this._entityManager.GetChampion( targetId );
			UIEvent.ItemUsed( entity, result );
		}

		public void HandleBuffCreated( string buffId, string casterId, string targetId )
		{
			CEntity caster = this._entityManager.GetEntity( casterId );
			CEntity target = this._entityManager.GetEntity( targetId );
			this._buffManager.CreateBuff( buffId, caster, target );
		}

		public void HandleBuffDestroied( string buffId )
		{
			CBuff buff = this._buffManager.GetBuff( buffId );
			buff.MarkToDestroy();
		}

		public void HandleDebugDraw( SyncEvent.DebugDrawType type, Vector3 v0, Vector3 v1, float f, Color color )
		{
			this._debugDrawer.HandleDebugDraw( type, v0, v1, f, color );
		}

		public void Update( float deltaTime )
		{
			++this.frame;
			this.deltaTime = ( Fix64 )deltaTime;
			this.time += this.deltaTime;

			this._context.deltaTime = this.deltaTime;
			this._context.time = this.time;
			this._context.frame = this.frame;

			this._fxManager.Update( this._context );
			this._buffManager.Update( this._context );
			this._entityManager.Update( this._context );
			this._maze.Update( this._context );
		}

		public void LateUpdate()
		{
			this.camera?.Update( this._context );
		}

		public void OnDrawGizmos()
		{
			this._debugDrawer.Update();
		}
	}
}