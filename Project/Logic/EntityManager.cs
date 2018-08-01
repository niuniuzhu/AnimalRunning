using Core.FMath;
using Logic.Event;
using System.Collections.Generic;
using Logic.Map;

namespace Logic
{
	public class EntityManager
	{
		public int entityCount => this._entities.Count;
		public int championCount => this._champions.Count;
		public int itemCount => this._items.Count;

		private readonly GPool _gPool = new GPool();

		private readonly List<Entity> _entities = new List<Entity>();
		private readonly Dictionary<string, Entity> _idToEntity = new Dictionary<string, Entity>();

		private readonly List<Item> _items = new List<Item>();
		private readonly Dictionary<string, Item> _idToItem = new Dictionary<string, Item>();

		private readonly List<Champion> _champions = new List<Champion>();
		private readonly Dictionary<string, Champion> _idToChampion = new Dictionary<string, Champion>();

		private readonly List<Collider> _colliders = new List<Collider>();
		private readonly List<ITrigger> _triggers = new List<ITrigger>();

		private Battle _battle;
		private Fix64 _itemRefreshTime;
		private List<ITileObject> _championsAround = new List<ITileObject>();

		public EntityManager( Battle battle )
		{
			this._battle = battle;
		}

		public void Dispose()
		{
			int count = this._entities.Count;
			for ( int i = 0; i < count; i++ )
				this._entities[i].MarkToDestroy();
			this.DestroyEnties();
			this._gPool.Dispose();
			this._championsAround = null;
			this._battle = null;
		}

		public T Create<T>( EntityParam param ) where T : Entity, new()
		{
			T entity = this._gPool.Pop<T>();
			this._idToEntity[param.rid] = entity;
			this._entities.Add( entity );

			System.Type t = typeof( T );
			if ( typeof( Collider ).IsAssignableFrom( t ) )
			{
				Collider collider = entity as Collider;
				this._colliders.Add( collider );
			}
			if ( typeof( ITrigger ).IsAssignableFrom( t ) )
			{
				ITrigger trigger = entity as ITrigger;
				this._triggers.Add( trigger );
			}
			if ( typeof( Item ).IsAssignableFrom( t ) )
			{
				Item item = entity as Item;
				this._idToItem[param.rid] = item;
				this._items.Add( item );
			}
			if ( typeof( Champion ).IsAssignableFrom( t ) )
			{
				Champion champion = entity as Champion;
				this._idToChampion[param.rid] = champion;
				this._champions.Add( champion );
			}
			SyncEvent.CreateEntity( entity.GetType().Name, param );
			entity.OnAddedToBattle( this._battle, param );
			return entity;
		}

		public Entity GetEntity( string rid )
		{
			if ( string.IsNullOrEmpty( rid ) )
				return null;
			this._idToEntity.TryGetValue( rid, out Entity entity );
			return entity;
		}

		public Entity GetEntityAt( int index )
		{
			if ( index < 0 ||
				 index > this._entities.Count - 1 )
				return null;
			return this._entities[index];
		}

		public Champion GetChampion( string id )
		{
			if ( string.IsNullOrEmpty( id ) )
				return null;
			this._idToChampion.TryGetValue( id, out Champion champion );
			return champion;
		}

		public Champion GetChampionAt( int index )
		{
			if ( index < 0 ||
				 index > this._champions.Count - 1 )
				return null;
			return this._champions[index];
		}

		public void GetChampionsNearby( Champion target, TargetType targetType, Fix64 radius, int maxNumber, ref List<Champion> champions )
		{
			if ( targetType == TargetType.Self )
			{
				champions.Add( target );
				return;
			}

			radius *= radius;
			int count = this._champions.Count;
			for ( int i = 0; i < count; i++ )
			{
				Champion champion = this._champions[i];

				if ( targetType == TargetType.Hostile &&
					 champion.team == target.team )
					continue;

				if ( targetType == TargetType.Teamate &&
					 champion.team != target.team )
					continue;

				if ( champion == target )
				{
					champions.Add( target );
					continue;
				}

				FVec3 d = target.position - champion.position;
				if ( d.SqrMagnitude() <= radius )
					champions.Add( champion );

				if ( maxNumber >= 0 && this._champions.Count == maxNumber )
					break;
			}
		}

		public Item GetItem( string id )
		{
			if ( string.IsNullOrEmpty( id ) )
				return null;
			this._idToItem.TryGetValue( id, out Item item );
			return item;
		}

		public Item GetItemAt( int index )
		{
			if ( index < 0 ||
				 index > this._items.Count - 1 )
				return null;
			return this._items[index];
		}

		internal void Update( UpdateContext context )
		{
			//重新分配物品
			if ( context.time >= this._itemRefreshTime )
				this.SupplyItems();

			this.GenericUpdate( context );
			//更新状态
			this.UpdateState( context );
			//更新触发器
			this.UpdateTrigger( context );
			//发送实体状态
			this.SyncState();
			//清理实体
			this.DestroyEnties();
		}

		private void UpdateTrigger( UpdateContext context )
		{
			this.TriggerDetection();
		}

		private void GenericUpdate( UpdateContext context )
		{
			int count = this._entities.Count;
			for ( int i = 0; i < count; i++ )
			{
				Entity entity = this._entities[i];
				entity.OnGenericUpdate( context );
			}
		}

		private void UpdateState( UpdateContext context )
		{
			//champoin
			int count = this._champions.Count;
			for ( int i = 0; i < count; i++ )
			{
				Entity entity = this._champions[i];
				entity.OnUpdateState( context );
			}
			//item
			count = this._items.Count;
			for ( int i = 0; i < count; i++ )
			{
				Entity entity = this._items[i];
				entity.OnUpdateState( context );
			}
		}

		private void DestroyEnties()
		{
			int count = this._entities.Count;
			for ( int i = 0; i < count; i++ )
			{
				Entity entity = this._entities[i];
				if ( !entity.markToDestroy )
					continue;

				entity.OnRemoveFromBattle();

				if ( entity is Collider collider )
				{
					this._colliders.Remove( collider );
				}
				if ( entity is ITrigger trigger )
				{
					this._triggers.Remove( trigger );
				}
				if ( entity is Champion champion )
				{
					this._champions.Remove( champion );
					this._idToChampion.Remove( entity.rid );
				}
				if ( entity is Item item )
				{
					this._items.Remove( item );
					this._idToItem.Remove( entity.rid );
				}

				this._entities.RemoveAt( i );
				this._idToEntity.Remove( entity.rid );
				this._gPool.Push( entity );
				--i;
				--count;
			}
		}

		private void SyncState()
		{
			int count = this._entities.Count;
			for ( int i = 0; i < count; i++ )
			{
				Entity entity = this._entities[i];
				entity.OnSyncState();
			}
		}

		public void SupplyItems()
		{
			int count = this._battle.maxItemCount - this._items.Count;
			for ( int i = 0; i < count; i++ )
			{
				string id = this._battle.items[this._battle.random.Next( 0, this._battle.items.Length )];
				FVec3 position = this._battle.maze.GetRandomPointInWalkables( this._battle.random );
				FVec2 rndCircle = this._battle.random.onUnitCircle;
				Item item = this._battle.CreateItem( this._battle.random.IdHash( id ), position,
													 new FVec3( rndCircle.x, Fix64.Zero, rndCircle.y ) );
				this._items.Add( item );
			}
			this._itemRefreshTime = this._battle.time +
								   this._battle.random.NextFix64( this._battle.itemUpdateInterval[0],
																  this._battle.itemUpdateInterval[1] );
		}

		private void TriggerDetection()
		{
			int count = this._triggers.Count;
			for ( int i = 0; i < count; i++ )
			{
				ITrigger trigger = this._triggers[i];
				Fix64 r = trigger.triggerRadius * trigger.triggerRadius;
				this._battle.maze.GetTileObjectsAround( trigger.tileIndex, ref this._championsAround );
				int c2 = this._championsAround.Count;
				Champion champion = null;
				Fix64 min = Fix64.MaxValue;
				for ( int j = 0; j < c2; j++ )
				{
					ITileObject tileObject = this._championsAround[j];
					if ( !( tileObject is Champion ) )
						continue;

					Fix64 d = tileObject.position.DistanceSquared( trigger.position );
					if ( d <= r && d < min )
					{
						champion = ( Champion )tileObject;
						min = d;
					}
				}
				if ( champion != null )
				{
					trigger.OnTrigger( champion );
					--i;
					--count;
				}
				this._championsAround.Clear();
			}
		}
	}
}