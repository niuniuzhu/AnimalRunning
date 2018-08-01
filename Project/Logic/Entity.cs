using Core.FMath;
using Logic.Event;
using Logic.Misc;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Logic
{
	public abstract class Entity : GPoolObject
	{
		public string id => this._data.id;

		[Sync( Attr.Position )]
		public FVec3 position
		{
			get => this._position;
			internal set
			{
				if ( this._position == value )
					return;
				this._position = value;
				this.OnPositionChanged();
			}
		}

		[Sync( Attr.Direction )]
		public FVec3 direction
		{
			get => this._direction;
			internal set
			{
				if ( this._direction == value )
					return;
				this._direction = value;
				this.OnDirectionChanged();
			}
		}

		public Battle battle { get; private set; }

		internal bool markToDestroy { get; private set; }

		protected EntityData _data;

		private FVec3 _position;
		private FVec3 _direction = FVec3.forward;
		private PropertyInfo[] _syncProperties;
		private SyncAttribute[] _syncAttributes;

		protected Entity()
		{
			List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
			List<SyncAttribute> attributes = new List<SyncAttribute>();
			Type type = this.GetType();
			PropertyInfo[] properties = type.GetProperties();
			int count = properties.Length;
			for ( int i = 0; i < count; i++ )
			{
				PropertyInfo propertyInfo = properties[i];
				object[] syncAttributes = propertyInfo.GetCustomAttributes( typeof( SyncAttribute ), true );
				if ( syncAttributes.Length > 0 )
				{
					propertyInfos.Add( propertyInfo );
					attributes.Add( ( SyncAttribute )syncAttributes[0] );
				}
			}
			this._syncProperties = propertyInfos.ToArray();
			this._syncAttributes = attributes.ToArray();
		}

		protected override void InternalDispose()
		{
			this._syncProperties = null;
			this._syncAttributes = null;
		}

		public void MarkToDestroy()
		{
			this.markToDestroy = true;
		}

		internal void OnAddedToBattle( Battle battle, EntityParam param )
		{
			this.battle = battle;
			this.rid = param.rid;
			this._data = ModelFactory.GetEntityData( Utils.GetIDFromRID( this.rid ) );
			this.position = param.position;
			this.direction = param.direction;
			SyncEvent.EntityAddedToBattle( this.rid );
			this.InternalOnAddedToBattle( param );
			this.OnSyncState();
		}

		internal void OnRemoveFromBattle()
		{
			SyncEvent.EntityRemoveFromBattle( this.rid );
			this.InternalOnRemoveFromBattle();

			this.markToDestroy = false;
			this.battle = null;
			this._data = null;
		}

		protected virtual void InternalOnAddedToBattle( EntityParam param )
		{
		}

		protected virtual void InternalOnRemoveFromBattle()
		{
		}

		protected virtual void OnPositionChanged()
		{
		}

		protected virtual void OnDirectionChanged()
		{
		}

		public FVec3 PointToWorld( FVec3 point )
		{
			return this.position + FQuat.FromToRotation( FVec3.forward, this.direction ) * point;
		}

		public FVec3 PointToLocal( FVec3 point )
		{
			return FQuat.Inverse( FQuat.FromToRotation( FVec3.forward, this.direction ) ) * ( point - this.position );
		}

		public FVec3 VectorToWorld( FVec3 point )
		{
			return FQuat.FromToRotation( FVec3.forward, this.direction ) * point;
		}

		public FVec3 VectorToLocal( FVec3 point )
		{
			return FQuat.Inverse( FQuat.FromToRotation( FVec3.forward, this.direction ) ) * point;
		}

		public virtual void OnGenericUpdate( UpdateContext context )
		{
		}

		internal virtual void OnUpdateState( UpdateContext context )
		{
		}

		internal void OnSyncState()
		{
			SyncEvent e = SyncEvent.BeginSyncProps( this.rid );
			int count = this._syncProperties.Length;
			for ( int i = 0; i < count; i++ )
			{
				PropertyInfo propertyInfo = this._syncProperties[i];
				SyncAttribute attribute = this._syncAttributes[i];
				SyncEvent.AddSyncProp( e, attribute.attr, propertyInfo.GetValue( this, null ) );
			}
			SyncEvent.EndSyncProps( e );
		}
	}
}