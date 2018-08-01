using System.Collections.Generic;
using Logic.Event;

namespace Logic
{
	public class BuffManager
	{
		private readonly GPool _gPool = new GPool();
		private readonly List<Buff> _buffs = new List<Buff>();
		private readonly Dictionary<string, Buff> _idToBuff = new Dictionary<string, Buff>();

		private Battle _battle;

		public BuffManager( Battle battle )
		{
			this._battle = battle;
		}

		public void Dispose()
		{
			int count = this._buffs.Count;
			for ( int i = 0; i < count; i++ )
				this._buffs[i].MarkToDestroy();
			this.DestroyBuffs();
			this._gPool.Dispose();
			this._battle = null;
		}

		public Buff CreateBuff( string id, Champion caster, Champion target )
		{
			Buff buff = this._gPool.Pop<Buff>();
			string rid = this._battle.random.IdHash( id );
			buff.OnCreate( this._battle, rid, caster, target );
			this._idToBuff[buff.rid] = buff;
			this._buffs.Add( buff );
			SyncEvent.CreateBuff( rid, caster.rid, target.rid );
			return buff;
		}

		public Buff GetBuff( string rid )
		{
			if ( string.IsNullOrEmpty( rid ) )
				return null;
			this._idToBuff.TryGetValue( rid, out Buff buff );
			return buff;
		}

		internal void Update( UpdateContext context )
		{
			int count = this._buffs.Count;
			for ( int i = 0; i < count; i++ )
			{
				Buff buff = this._buffs[i];
				buff.OnUpdate( context );
			}
			//清理Buff
			this.DestroyBuffs();
		}

		private void DestroyBuffs()
		{
			int count = this._buffs.Count;
			for ( int i = 0; i < count; i++ )
			{
				Buff buff = this._buffs[i];
				if ( !buff.markToDestroy )
					continue;

				buff.OnDiscard();

				SyncEvent.DestroyBuff( buff.rid );

				this._buffs.RemoveAt( i );
				this._idToBuff.Remove( buff.rid );
				this._gPool.Push( buff );
				--i;
				--count;
			}
		}

		public void DestroyBuff( string buffId )
		{
			Buff buff = this.GetBuff( buffId );
			buff.MarkToDestroy();
		}
	}
}