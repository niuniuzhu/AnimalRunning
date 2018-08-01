using System.Collections.Generic;
using Core.FMath;
using Logic.Misc;

namespace Logic
{
	public sealed class Buff : GPoolObject
	{
		public string id => this._data.id;
		public Battle battle { get; private set; }

		internal bool markToDestroy { get; private set; }

		private BuffData _data;
		private Champion _caster;
		private Champion _target;
		private Fix64 _discardTime;

		protected override void InternalDispose()
		{
		}

		public void MarkToDestroy()
		{
			this.markToDestroy = true;
			this.Invalid();
		}

		public void OnCreate( Battle battle, string rid, Champion caster, Champion target )
		{
			this.battle = battle;
			this.rid = rid;
			this._data = ModelFactory.GetBuffData( Utils.GetIDFromRID( this.rid ) );
			this._discardTime = this.battle.time + this._data.duration;
			this._caster = caster;
			this._target = target;

			int count = this._data.conflicts.Length;
			for ( int i = 0; i < count; i++ )
			{
				string conflictId = this._data.conflicts[i];
				List<string> buffIds = target.GetBuffIdsInPucker( conflictId );
				if ( buffIds != null )
				{
					for ( int j = buffIds.Count - 1; j >= 0; j-- )
						this.battle.DestroyBuff( buffIds[j] );
				}
			}
			this.Appply();
		}

		public void OnDiscard()
		{
			this.markToDestroy = false;
			this.battle = null;
			this._caster = null;
			this._target = null;
			this._data = null;
		}

		public void OnUpdate( UpdateContext context )
		{
			if ( context.time >= this._discardTime )
				this.MarkToDestroy();
		}

		private void Appply()
		{
			this.Op( 0 );
			this._target.AddBuff( this.rid );
		}

		private void Invalid()
		{
			this.Op( 1 );
			this._target.RemoveBuff( this.rid );
		}

		private void Op( int op )
		{
			int count = this._data.attrs.Length;
			for ( int i = 0; i < count; i++ )
			{
				Attr attr = this._data.attrs[i];
				Fix64 value = this._data.values[i];
				switch ( attr )
				{
					case Attr.MoveSpeedFactor:
						{
							Champion champion = ( Champion )this._target;
							if ( op == 0 )
								champion.moveSpeedFactor *= value;
							else
								champion.moveSpeedFactor /= value;
						}
						break;

					case Attr.Fov:
						{
							Champion champion = ( Champion )this._target;
							if ( op == 0 )
								champion.fov *= value;
							else
								champion.fov /= value;
						}
						break;
				}
			}
		}
	}
}