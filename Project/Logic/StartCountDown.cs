using Core.FMath;
using Logic.Event;

namespace Logic
{
	public class StartCountDown
	{
		private static readonly Fix64 WAIT = Fix64.Two;

		private Battle _battle;
		private readonly Fix64 _beginTime;
		private readonly int _countDown;
		private int _num;
		private Fix64 _nextTime;
		private bool _valid;

		public StartCountDown( Battle battle, int countDown )
		{
			this._battle = battle;
			this._countDown = countDown;
			this._valid = true;
			this._beginTime = battle.time + WAIT;
			this._num = 0;
			this._nextTime = Fix64.Zero;
		}

		public void Dispose()
		{
			this._battle = null;
		}

		public void Update( UpdateContext context )
		{
			if ( !this._valid || context.time < this._beginTime )
				return;

			if ( context.time >= this._nextTime )
			{
				SyncEvent.CountDown( this._num, this._countDown );
				this._nextTime = context.time + Fix64.One;
				++this._num;
			}
			if ( this._num <= this._countDown )
				return;
			this._valid = false;
			this._battle.DestroyRails();
		}
	}
}