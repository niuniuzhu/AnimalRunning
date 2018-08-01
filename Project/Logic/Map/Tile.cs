using Core.FMath;
using System.Collections.Generic;

namespace Logic.Map
{
	internal sealed class Tile
	{
		public enum Flag
		{
			Obstacle,
			Border,
			Walkable,
		}

		internal int index { get; }
		internal Flag flag { get; set; }

		private readonly List<ITileObject> _objects = new List<ITileObject>();

		internal List<ITileObject> objects => this._objects;

		internal FBounds aabb;

		internal Tile( int index )
		{
			this.index = index;
		}

		internal void AddObject( ITileObject tileObject )
		{
			this._objects.Add( tileObject );
		}

		internal void RemoveObject( ITileObject tileObject )
		{
			this._objects.Remove( tileObject );
		}

		public void Dispose()
		{
			this._objects.Clear();
		}
	}
}