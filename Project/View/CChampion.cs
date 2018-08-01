using Core.FMath;
using Logic;
using Logic.FSM;
using UnityEngine;
using View.FSM.Actions;
using View.Misc;

namespace View
{
	public class CChampion : CCollider
	{
		public Fix64 naturalSpeed => this._data.naturalSpeed;

		public int team { get; private set; }
		public Fix64 fov { get; private set; }
		public Fix64 moveSpeedFactor { get; protected set; }
		public Vector3 velocity { get; protected set; }
		public Fix64 speed { get; protected set; }
		public Champion.MazeResult mazeResult { get; private set; }

		private Fix64 _destFov;
		private FiniteStateMachine _fsm;

		public CChampion()
		{
			this._fsm = new FiniteStateMachine( this );
			FSMState state = this._fsm.CreateState( FSMStateType.Idle );
			state.CreateAction<CIdle>();
			state = this._fsm.CreateState( FSMStateType.Move );
			state.CreateAction<CMove>();
		}

		protected override void InternalDispose()
		{
			this._fsm = null;

			base.InternalDispose();
		}

		internal override void OnAddedToBattle()
		{
			base.OnAddedToBattle();

			this._fsm.Start();
		}

		internal override void OnRemoveFromBattle()
		{
			this._fsm.Stop();

			base.OnRemoveFromBattle();
		}

		public override void OnUpdateState( UpdateContext context )
		{
			base.OnUpdateState( context );

			this.fov = Fix64.Lerp( this.fov, this._destFov, context.deltaTime * ( Fix64 )7 );
			this._fsm.Update( context );
		}

		public void HandleEntityStateChanged( FSMStateType type, bool force, object[] param )
		{
			this._fsm.ChangeState( type, force, param );
		}

		protected override void OnAttrChanged( Attr attr, object value )
		{
			switch ( attr )
			{
				case Attr.Team:
					this.team = ( int )value;
					break;

				case Attr.Fov:
					this._destFov = ( Fix64 )value;
					break;

				case Attr.MoveSpeedFactor:
					this.moveSpeedFactor = ( Fix64 )value;
					this.UpdateGraphicSpeed( ( float )this.moveSpeedFactor );
					break;

				case Attr.Velocity:
					this.velocity = ( ( FVec3 )value ).ToVector3();
					break;

				case Attr.Speed:
					this.speed = ( Fix64 )value;
					break;

				case Attr.MazeResult:
					this.mazeResult = ( Champion.MazeResult )value;
					break;
			}
			base.OnAttrChanged( attr, value );
		}
	}
}