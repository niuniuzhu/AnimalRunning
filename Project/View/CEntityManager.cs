using System.Collections.Generic;
using Logic;
using UnityEngine;
using View.Event;

namespace View
{
	public class CEntityManager
	{
		public int entityCount => this._entities.Count;
		public int championCount => this._champions.Count;
		public int itemCount => this._items.Count;

		private readonly GPool _gPool = new GPool();

		private readonly List<CEntity> _entities = new List<CEntity>();
		private readonly Dictionary<string, CEntity> _idToEntity = new Dictionary<string, CEntity>();

		private readonly List<CItem> _items = new List<CItem>();
		private readonly Dictionary<string, CItem> _idToItem = new Dictionary<string, CItem>();

		private readonly List<CChampion> _champions = new List<CChampion>();
		private readonly Dictionary<string, CChampion> _idToChampion = new Dictionary<string, CChampion>();

		private CBattle _battle;

		public CEntityManager( CBattle battle )
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
			this._battle = null;
		}

		public T Create<T>( EntityParam param ) where T : CEntity, new()
		{
			T entity = this._gPool.Pop<T>();
			this._idToEntity[param.rid] = entity;
			this._entities.Add( entity );

			System.Type t = typeof( T );
			if ( typeof( CItem ).IsAssignableFrom( t ) )
			{
				CItem item = entity as CItem;
				this._idToItem[param.rid] = item;
				this._items.Add( item );
			}
			if ( typeof( CChampion ).IsAssignableFrom( t ) )
			{
				CChampion champion = entity as CChampion;
				this._idToChampion[param.rid] = champion;
				this._champions.Add( champion );
			}
			entity.OnCreated( this._battle, param );

			return entity;
		}

		public CEntity GetEntity( string id )
		{
			this._idToEntity.TryGetValue( id, out CEntity entity );
			return entity;
		}

		public CEntity GetEntityAt( int index )
		{
			if ( index < 0 ||
				 index > this._entities.Count - 1 )
				return null;
			return this._entities[index];
		}

		public CChampion GetChampion( string id )
		{
			if ( string.IsNullOrEmpty( id ) )
				return null;
			this._idToChampion.TryGetValue( id, out CChampion champion );
			return champion;
		}

		public CChampion GetChampionAt( int index )
		{
			if ( index < 0 ||
				 index > this._champions.Count - 1 )
				return null;
			return this._champions[index];
		}

		public void GetChampionsNearby( CChampion target, TargetType targetType, float radius, int maxNumber, ref List<CChampion> champions )
		{
			if ( maxNumber <= 0 )
				return;

			if ( targetType == TargetType.Self )
			{
				champions.Add( target );
				return;
			}

			radius *= radius;
			int count = this._champions.Count;
			for ( int i = 0; i < count; i++ )
			{
				CChampion champion = this._champions[i];

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

				Vector3 d = target.position - champion.position;
				if ( d.sqrMagnitude <= radius )
					champions.Add( champion );

				if ( this._champions.Count == maxNumber )
					break;
			}
		}

		public CItem GetItem( string id )
		{
			if ( string.IsNullOrEmpty( id ) )
				return null;
			this._idToItem.TryGetValue( id, out CItem item );
			return item;
		}

		public CItem GetItemAt( int index )
		{
			if ( index < 0 ||
				 index > this._items.Count - 1 )
				return null;
			return this._items[index];
		}

		private void DestroyEnties()
		{
			int count = this._entities.Count;
			for ( int i = 0; i < count; i++ )
			{
				CEntity entity = this._entities[i];
				if ( !entity.markToDestroy )
					continue;
				UIEvent.EntityDestroied( entity );
				entity.OnDestroy();

				if ( entity is CChampion )
				{
					this._champions.Remove( ( CChampion )entity );
					this._idToChampion.Remove( entity.rid );
				}
				if ( entity is CItem item )
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

		public void Update( UpdateContext context )
		{
			//更新状态
			this.UpdateState( context );
			//处理战斗信息
			//清理实体
			this.DestroyEnties();
		}

		private void UpdateState( UpdateContext context )
		{
			//champoin
			int count = this._champions.Count;
			for ( int i = 0; i < count; i++ )
			{
				CEntity entity = this._champions[i];
				entity.OnUpdateState( context );
			}
			//item
			count = this._items.Count;
			for ( int i = 0; i < count; i++ )
			{
				CEntity entity = this._items[i];
				entity.OnUpdateState( context );
			}
		}
	}
}