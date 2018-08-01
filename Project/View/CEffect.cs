using Logic;
using Logic.Misc;
using UnityEngine;
using View.Graphics;

namespace View
{
	public class CEffect : GPoolObject
	{
		private Vector3 _position;
		public Vector3 position
		{
			get => this._position;
			set
			{
				if ( this._position == value )
					return;
				this._position = value;
				if ( this.graphic != null )
					this.graphic.position = this._position;
			}
		}

		private Vector3 _direction = Vector3.forward;
		public Vector3 direction
		{
			get => this._direction;
			set
			{
				if ( this._direction == value )
					return;
				this._direction = value;
				if ( this.graphic != null )
					this.graphic.rotation = Quaternion.LookRotation( this._direction, Vector3.up );
			}
		}

		public CBattle battle { get; private set; }
		public EffectGraphic graphic { get; private set; }

		internal bool markToDestroy { get; private set; }

		private EffectData _data;
		private CEntity _caster;
		private CEntity _target;
		private float _destroyTime;

		public CEffect()
		{
			this.graphic = new EffectGraphic();
		}

		protected override void InternalDispose()
		{
			this.graphic.Dispose();
			this.graphic = null;
		}

		public void MarkToDestroy()
		{
			this.markToDestroy = true;
		}

		internal void OnCreate( CBattle battle, string rid, IEffectHolder holder, CEntity caster, CEntity target )
		{
			this.battle = battle;
			this.rid = rid;
			this._data = ModelFactory.GetEffectData( Utils.GetIDFromRID( this.rid ) );
			this._caster = caster;
			this._target = target;
			this.graphic.OnCreate( this, $"{this._data.model}" );
			this.graphic.name = this.rid;
			switch ( this._data.trackMode )
			{
				case EffectData.TrackMode.DockedAtCaster:
				case EffectData.TrackMode.FollowCaster:
					this.Track( this._caster );
					break;

				case EffectData.TrackMode.DockedAtTarget:
				case EffectData.TrackMode.FollowTarget:
					this.Track( this._target );
					break;
			}
			switch ( this._data.dissipatingMode )
			{
				case EffectData.DissipatingMode.Auto:
					this.graphic.particleScript.destroyNotifier = this.NotifiyDestroy;
					break;

				case EffectData.DissipatingMode.Timed:
					this._destroyTime = ( float )this.battle.time + this._data.duration;
					break;

				case EffectData.DissipatingMode.Programatic:
					holder.destroyNotifier = this.NotifiyDestroy;
					break;
			}
		}

		internal void OnDestroy()
		{
			this.graphic.OnDestroy();
			this.markToDestroy = false;
			this._caster = null;
			this._target = null;
			this.battle = null;
			this._data = null;
		}

		public void OnUpdate( UpdateContext context )
		{
			switch ( this._data.trackMode )
			{
				case EffectData.TrackMode.FollowCaster:
					this.Track( this._caster );
					break;

				case EffectData.TrackMode.FollowTarget:
					this.Track( this._target );
					break;
			}
			switch ( this._data.dissipatingMode )
			{
				case EffectData.DissipatingMode.Timed:
					if ( this._destroyTime >= ( float )context.time )
						this.markToDestroy = true;
					break;
			}
		}

		private void NotifiyDestroy( IEffectHolder holder )
		{
			this.markToDestroy = true;
		}

		private void Track( CEntity target )
		{
			this.position = target.position + new Vector3( 0, 0.1f, 0 );
			this.direction = target.direction;
		}
	}
}