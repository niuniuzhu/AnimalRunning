using System.Collections.Generic;
using Game.Misc;
using Logic;

namespace View
{
	public class FxManager
	{
		public int effectCount => this._effects.Count;

		private readonly GPool _gPool = new GPool();

		private readonly List<CEffect> _effects = new List<CEffect>();
		private readonly Dictionary<string, CEffect> _idToEffect = new Dictionary<string, CEffect>();

		private CBattle _battle;

		public FxManager( CBattle battle )
		{
			this._battle = battle;
		}

		public void Dispose()
		{
			int count = this._effects.Count;
			for ( int i = 0; i < count; i++ )
				this._effects[i].MarkToDestroy();
			this.DestroyEffects();
			this._gPool.Dispose();
			this._battle = null;
		}

		public CEffect CreateEffect( string id, IEffectHolder holder, CEntity caster, CEntity target )
		{
			CEffect effect = this._gPool.Pop<CEffect>();
			string rid = Utils.MakeRidFromID( id );
			effect.OnCreate( this._battle, rid, holder, caster, target );
			this._idToEffect[effect.rid] = effect;
			this._effects.Add( effect );
			return effect;
		}

		public CEffect GetEffect( string id )
		{
			this._idToEffect.TryGetValue( id, out CEffect effect );
			return effect;
		}

		public CEffect GetEffectAt( int index )
		{
			if ( index < 0 ||
				 index > this._effects.Count - 1 )
				return null;
			return this._effects[index];
		}

		private void DestroyEffects()
		{
			int count = this._effects.Count;
			for ( int i = 0; i < count; i++ )
			{
				CEffect effect = this._effects[i];
				if ( !effect.markToDestroy )
					continue;
				effect.OnDestroy();

				this._effects.RemoveAt( i );
				this._idToEffect.Remove( effect.rid );
				this._gPool.Push( effect );
				--i;
				--count;
			}
		}

		public void Update( UpdateContext context )
		{
			int count = this._effects.Count;
			for ( int i = 0; i < count; i++ )
			{
				CEffect effect = this._effects[i];
				effect.OnUpdate( context );
			}
			this.DestroyEffects();
		}
	}
}