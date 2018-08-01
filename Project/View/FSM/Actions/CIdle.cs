using Logic;
using Random = UnityEngine.Random;

namespace View.FSM.Actions
{
	public class CIdle : CChampionAction
	{
		private float _time;
		private float _nextTime;

		protected override void OnEnter( object[] param )
		{
			this.owner.graphic.animator.SetBool( "idle", true );
			this._time = 0;
			float idle2Length = this.owner.graphic.animator.GetClipLength( "idle2" );
			this._nextTime = Random.Range( idle2Length, idle2Length + 8f );
		}

		protected override void OnExit()
		{
			this.owner.graphic.animator.SetBool( "idle", false );
		}

		protected override void OnUpdate( UpdateContext context )
		{
			this._time += ( float )context.deltaTime;
			if ( this._time >= this._nextTime )
			{
				this._time = 0;
				float idle2Length = this.owner.graphic.animator.GetClipLength( "idle2" );
				this._nextTime = Random.Range( idle2Length, idle2Length + 8f );
				this.owner.graphic.animator.CrossFade( "idle2", 1.0f );
			}
		}
	}
}