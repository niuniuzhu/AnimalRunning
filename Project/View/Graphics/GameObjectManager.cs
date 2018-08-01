using System.Collections.Generic;
using UnityEngine;

namespace View.Graphics
{
	public class GameObjectManager
	{
		private const string RESOURCE_PATH = "Prefabs/";

		private readonly Dictionary<string, GameObjectPool> _pool = new Dictionary<string, GameObjectPool>();

		public GameObject Pop( string id )
		{
			if ( !this._pool.TryGetValue( id, out GameObjectPool pool ) )
			{
				pool = new GameObjectPool( id );
				this._pool[id] = pool;
			}
			return pool.Pop();
		}

		public void Push( GameObject go )
		{
			GameObjectPool pool = this._pool[go.name];
			pool.Push( go );
		}

		public void Dispose()
		{
			foreach ( KeyValuePair<string, GameObjectPool> kv in this._pool )
				kv.Value.Dispose();
			this._pool.Clear();
		}

		class GameObjectPool : Queue<GameObject>
		{
			public string id { get; }

			public GameObjectPool( string id )
			{
				this.id = id;
			}

			public GameObject Pop()
			{
				GameObject go;
				if ( this.Count > 0 )
				{
					go = this.Dequeue();
					go.SetActive( true );
					return go;
				}
				GameObject resources = Resources.Load<GameObject>( RESOURCE_PATH + this.id );
				if ( resources == null )
					resources = Resources.Load<GameObject>( RESOURCE_PATH + "failed" );
				go = Object.Instantiate( resources );
				go.name = this.id;
				return go;
			}

			public void Push( GameObject go )
			{
				go.SetActive( false );
				this.Enqueue( go );
			}

			public void Dispose()
			{
				while ( this.Count > 0 )
				{
					GameObject go = this.Dequeue();
					Object.Destroy( go );
				}
			}
		}
	}
}