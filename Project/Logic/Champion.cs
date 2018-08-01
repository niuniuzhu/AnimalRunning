using Core.FMath;
using Logic.Event;
using Logic.FSM;
using Logic.FSM.Actions;
using Logic.Misc;
using System.Collections.Generic;

namespace Logic
{
	public class Champion : Collider
	{
		public enum MazeResult
		{
			Nan,
			Win,
			Lose
		}

		public Fix64 naturalSpeed => this._data.naturalSpeed;

		[Sync( Attr.Uid )]
		public string uid { get; private set; }

		[Sync( Attr.Team )]
		public int team { get; private set; }

		[Sync( Attr.Fov )]
		public Fix64 fov { get; set; }

		[Sync( Attr.MoveSpeedFactor )]
		public Fix64 moveSpeedFactor { get; set; }

		[Sync( Attr.Velocity )]
		public FVec3 velocity { get; protected set; }

		[Sync( Attr.Speed )]
		public Fix64 speed { get; protected set; }

		[Sync( Attr.MazeResult )]
		public MazeResult mazeResult { get; set; }

		public string itemId { get; private set; }

		private FiniteStateMachine _fsm;
		private FVec3 _movingDirection;
		private List<Champion> _tmpChampions = new List<Champion>();
		private readonly List<string> _buffs = new List<string>();
		private readonly Dictionary<string, List<string>> _buffPucker = new Dictionary<string, List<string>>();

		public Champion()
		{
			this._fsm = new FiniteStateMachine( this );
			FSMState state = this._fsm.CreateState( FSMStateType.Idle );
			state.CreateAction<LIdle>();
			state = this._fsm.CreateState( FSMStateType.Move );
			state.CreateAction<LMove>();
		}

		protected override void InternalDispose()
		{
			this._fsm.Dispose();
			this._fsm = null;
			this._tmpChampions = null;

			base.InternalDispose();
		}

		protected override void InternalOnAddedToBattle( EntityParam param )
		{
			base.InternalOnAddedToBattle( param );

			this.uid = param.uid;
			this.team = param.team;
			this.fov = this._data.fov;
			this.speed = Fix64.Zero;
			this.velocity = FVec3.zero;
			this.moveSpeedFactor = Fix64.One;
			this.mazeResult = MazeResult.Nan;
			this._movingDirection = FVec3.zero;

			this._fsm.Start();
			this.ChangeState( FSMStateType.Idle );
		}

		protected override void InternalOnRemoveFromBattle()
		{
			this._fsm.Stop();

			while ( this._buffs.Count > 0 )
				this.battle.DestroyBuff( this._buffs[0] );
		}

		private void ChangeState( FSMStateType type, bool force = false, params object[] param )
		{
			SyncEvent.ChangeState( this.rid, type, force, param );
			this._fsm.ChangeState( type, force, param );
		}

		internal override void OnUpdateState( UpdateContext context )
		{
			this._fsm.Update( context );
			this.MoveStep( this._movingDirection );

			base.OnUpdateState( context );
		}

		public void BeginMove( FVec3 direction )
		{
			if ( direction.SqrMagnitude() < Fix64.Epsilon )
				this.ChangeState( FSMStateType.Idle );
			else
				this.ChangeState( FSMStateType.Move, false, direction );

			this._movingDirection = direction;
		}

		private void MoveStep( FVec3 direction )
		{
			if ( direction.SqrMagnitude() < Fix64.Epsilon )
			{
				this.speed = Fix64.Zero;
				this.velocity = FVec3.zero;
				return;
			}

			Fix64 dt = this.battle.deltaTime;
			FVec3 desiredDistance = direction * this.naturalSpeed * this.moveSpeedFactor * dt;

			FVec3 oldPos = this.position, pos = this.position;

			Fix64 dx = this.battle.maze.MoveDetection( this, desiredDistance.x, 0 );
			pos.x += dx;
			this.position = pos;

			Fix64 dz = this.battle.maze.MoveDetection( this, desiredDistance.z, 1 );
			pos.z += dz;
			this.position = pos;

			Fix64 moveDistance = ( this.position - oldPos ).Magnitude();
			this.speed = moveDistance / dt;
			this.velocity = desiredDistance / dt;
			this.direction = direction;
		}

		public bool PickItem( string itemId )
		{
			if ( !string.IsNullOrEmpty( this.itemId ) )
				return false;
			this.itemId = itemId;
			return true;
		}

		public bool UseItem()
		{
			if ( string.IsNullOrEmpty( this.itemId ) )
				return false;

			EntityData itemData = ModelFactory.GetEntityData( Utils.GetIDFromRID( this.itemId ) );
			this.battle.GetChampionsNearby( this, itemData.trigger.targetType, itemData.trigger.radius, itemData.trigger.triggerCount, ref this._tmpChampions );
			int count = this._tmpChampions.Count;
			if ( count > 0 )
			{
				for ( int i = 0; i < count; i++ )
				{
					Champion target = this._tmpChampions[i];
					this.battle.CreateBuff( itemData.trigger.buff, this, target );
				}
			}
			else
				this.battle.CreateBuff( itemData.trigger.buff, this, this );
			this._tmpChampions.Clear();
			this.itemId = string.Empty;
			return true;
		}

		public void AddBuff( string buffRid )
		{
			this._buffs.Add( buffRid );
			string bid = Utils.GetIDFromRID( buffRid );
			if ( this._buffPucker.TryGetValue( bid, out List<string> buffRids ) )
				buffRids.Add( buffRid );
			else
				this._buffPucker[bid] = new List<string> { buffRid };
		}

		public void RemoveBuff( string buffRid )
		{
			this._buffs.Remove( buffRid );
			string bid = Utils.GetIDFromRID( buffRid );
			this._buffPucker[bid].Remove( buffRid );
			if ( this._buffPucker[bid].Count == 0 )
				this._buffPucker.Remove( bid );
		}

		public List<string> GetBuffIdsInPucker( string buffId )
		{
			this._buffPucker.TryGetValue( buffId, out List<string> buffRids );
			return buffRids;
		}
	}
}