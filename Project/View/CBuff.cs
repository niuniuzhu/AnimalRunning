using Logic;
using Logic.Misc;

namespace View
{

	public sealed class CBuff : GPoolObject, IEffectHolder
	{
		public CBattle battle { get; private set; }
		public EffectHolderDestroied destroyNotifier { private get; set; }

		internal bool markToDestroy { get; private set; }

		private BuffData _data;

		protected override void InternalDispose()
		{
		}

		public void MarkToDestroy()
		{
			this.markToDestroy = true;
		}

		public void OnCreate( CBattle battle, string rid, CEntity castr, CEntity target )
		{
			this.battle = battle;
			this.rid = rid;
			this._data = ModelFactory.GetBuffData( Utils.GetIDFromRID( this.rid ) );
			if ( !string.IsNullOrEmpty( this._data.fx ) )
				this.battle.CreateEffect( this._data.fx, this, castr, target );
			if ( !string.IsNullOrEmpty( this._data.snd ) )
				target.graphic.audioSource.PlayOneShot( this._data.snd, 1f );

		}

		internal void OnDestroy()
		{
			this.destroyNotifier?.Invoke( this );
			this.destroyNotifier = null;
			this.markToDestroy = false;
			this.battle = null;
			this._data = null;
		}

		public void OnUpdate( UpdateContext context )
		{
		}
	}
}