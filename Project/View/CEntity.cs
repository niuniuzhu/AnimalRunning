using Core.FMath;
using Logic;
using UnityEngine;
using View.Event;
using View.Graphics;
using View.Misc;
using Utils = Logic.Misc.Utils;

namespace View
{
	public class CEntity : GPoolObject
	{
		public string id => this._data.id;
		public string name => this._data.name;

		private Vector3 _position;
		public Vector3 position
		{
			get => this._position;
			set
			{
				if ( this._position == value )
					return;
				this._position = value;
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
				this.graphic.rotation = Quaternion.LookRotation( this._direction, Vector3.up );
			}
		}

		public CBattle battle { get; private set; }
		public EntityGraphic graphic { get; private set; }

		internal bool markToDestroy { get; private set; }

		protected EntityData _data;

		private Vector3 _logicPos;
		private Vector3 _logicDir;

		protected CEntity()
		{
			this.graphic = new EntityGraphic();
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

		internal virtual void OnCreated( CBattle battle, EntityParam param )
		{
			this.battle = battle;
			this.rid = param.rid;
			this._data = ModelFactory.GetEntityData( Utils.GetIDFromRID( this.rid ) );

			this._logicPos = this._position = param.position.ToVector3();
			this._logicDir = this._direction = param.direction.ToVector3();

			this.graphic.OnCreate( this, this._data.model, param.skin );
			this.graphic.name = this.rid;
			this.graphic.position = this.position;
			this.graphic.rotation = Quaternion.LookRotation( this.direction, Vector3.up );
		}

		internal virtual void OnDestroy()
		{
			this.markToDestroy = false;
			this.graphic.OnDestroy();
			this.battle = null;
			this._data = null;
		}

		internal virtual void OnAddedToBattle()
		{
		}

		internal virtual void OnRemoveFromBattle()
		{
			this.markToDestroy = true;
		}

		protected virtual void OnAttrChanged( Attr attr, object value )
		{
			switch ( attr )
			{
				case Attr.Position:
					this._logicPos = ( ( FVec3 )value ).ToVector3();
					break;

				case Attr.Direction:
					this._logicDir = ( ( FVec3 )value ).ToVector3();
					break;
			}

			UIEvent.EntityAttrChanged( this, attr, value );
		}

		public virtual void OnUpdateState( UpdateContext context )
		{
			float dt = ( float )context.deltaTime;
			this.position = Vector3.Lerp( this.position, this._logicPos, dt * 10f );
			this.direction = Vector3.Slerp( this.direction, this._logicDir, dt * 8f );
		}

		protected void UpdateGraphicSpeed( float speed )
		{
			this.graphic.animator.speed = speed;
		}

		internal void HandleSyncProps( Attr[] attrs, object[] values, int count )
		{
			for ( int i = 0; i < count; i++ )
				this.OnAttrChanged( attrs[i], values[i] );
		}
	}
}