using Logic;
using System.Collections.Generic;

namespace View
{
	public class CBuffManager
	{
		private readonly GPool _gPool = new GPool();
		private readonly List<CBuff> _buffs = new List<CBuff>();
		private readonly Dictionary<string, CBuff> _idToBuff = new Dictionary<string, CBuff>();

		private CBattle _battle;

		public CBuffManager( CBattle battle )
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

		public CBuff CreateBuff( string rid, CEntity caster, CEntity target )
		{
			CBuff buff = this._gPool.Pop<CBuff>();
			buff.OnCreate( this._battle, rid, caster, target );
			this._idToBuff[buff.rid] = buff;
			this._buffs.Add( buff );
			return buff;
		}

		public CBuff GetBuff( string rid )
		{
			if ( string.IsNullOrEmpty( rid ) )
				return null;
			this._idToBuff.TryGetValue( rid, out CBuff buff );
			return buff;
		}

		internal void Update( UpdateContext context )
		{
			int count = this._buffs.Count;
			for ( int i = 0; i < count; i++ )
			{
				CBuff buff = this._buffs[i];
				buff.OnUpdate( context );
			}
			//清理CBuff
			this.DestroyBuffs();
		}

		private void DestroyBuffs()
		{
			int count = this._buffs.Count;
			for ( int i = 0; i < count; i++ )
			{
				CBuff buff = this._buffs[i];
				if ( !buff.markToDestroy )
					continue;
				buff.OnDestroy();

				this._buffs.RemoveAt( i );
				this._idToBuff.Remove( buff.rid );
				this._gPool.Push( buff );
				--i;
				--count;
			}
		}
	}
}