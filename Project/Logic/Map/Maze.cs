using Core.Algorithm.Generic;
using Core.Algorithm.Graph;
using Core.Algorithm.Triangulation;
using Core.FMath;
using Core.Math;
using Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logic.Map
{
	public class Maze
	{
		public FVec3 scale { get; }
		public FVec3 offset { get; }
		public int row { get; } = 20;
		public int col { get; } = 20;
		public FVec2 startPointPlace { get; }
		public int startIndex { get; private set; }
		public int endIndex { get; private set; }
		public FVec3 centerOfStartRange { get; private set; }
		public int[] walkables { get; private set; }

		private FMat4 _localToWorld;
		private FMat4 _worldToLocal;
		private Tile[] _tiles;
		private List<FBounds> _tmpBounds = new List<FBounds>();
		private int[] _tmpIndices = new int[6];
		private int[] _tmpIndices2 = new int[9];

		public Maze( FPseudoRandom random,
					  FVec3 scale, FVec3 offset, int row, int col,
					  int startIndex, int endIndex, FVec2 startPointPlace )
		{
			this.startPointPlace = startPointPlace;
			if ( this.row < ( int )this.startPointPlace.y + 3 ||
				 this.col < ( int )this.startPointPlace.x + 3 )
				throw new Exception( "The row or col invaild" );

			this.scale = scale;
			this.offset = offset;
			this.row = row;
			this.col = col;

			this._localToWorld = FMat4.FromTRS( this.offset, FQuat.identity, this.scale );
			this._worldToLocal = FMat4.NonhomogeneousInverse( this._localToWorld );

			//创建二维图
			Graph2D graph2 = this.CreateFullDigraph( random );
			//创建格子
			this.CreateTiles( graph2 );

			//是否随机起点和终点
			this.startIndex = startIndex < 0 ? this.RandomIndexWithinBorder( random ) : startIndex;
			if ( endIndex < 0 )
			{
				int[] candidate = new int[5];
				for ( int i = 0; i < 5; i++ )
					candidate[i] = this.RandomIndexWithinBorder( random );
				endIndex = this.GetFarthestToStartIndex( this.startIndex, candidate );
			}
			this.endIndex = endIndex;

			//初始化起点范围
			HashSet<int> walkablesHs = new HashSet<int>();
			this.SetStart( graph2, this.startIndex, walkablesHs );

			//创建起点和终点
			int[] coord = graph2.IndexToCoord( this.startIndex );
			Delaunay.DVertex startVertex = new Delaunay.DVertex( coord[0], coord[1] );
			coord = graph2.IndexToCoord( this.endIndex );
			Delaunay.DVertex endVertex = new Delaunay.DVertex( coord[0], coord[1] );
			//创建外圆周附近的顶点
			List<Delaunay.DVertex> vertices1 = this.GenVertices( 24, 1f, 0.8f, 24, random );
			//创建内圆周附近的顶点
			List<Delaunay.DVertex> vertices2 = this.GenVertices( 12, 0.2f, 0.6f, 12, random );
			//合并所有顶点
			List<Delaunay.DVertex> vertices = new List<Delaunay.DVertex> { startVertex, endVertex };
			vertices.AddRange( vertices1 );
			vertices.AddRange( vertices2 );

			//点集合的三角化
			List<Delaunay.DTriangle> triangles = Delaunay.Triangulate( vertices );

			//从三角形集合创建图
			GraphBase graph = Tools.TrianglesToGraph( triangles, random.NextFloat );
			//Prim算法创建最小生成树
			List<GraphEdge> edges = GraphSearcher.PrimSearch( graph, triangles[0].v0 );

			//每条边用A*算法生成路径
			int count = edges.Count;
			for ( int i = 0; i < count; i++ )
			{
				GraphEdge edge = edges[i];
				Delaunay.DVertex v0 = vertices[edge.from];
				Delaunay.DVertex v1 = vertices[edge.to];
				int[] path = GraphSearcher.AStarSearch( graph2, graph2.CoordToIndex( ( int )v0.x, ( int )v0.y ),
														graph2.CoordToIndex( ( int )v1.x, ( int )v1.y ) );
				this.SetPathWalkable( path, walkablesHs );

				//把顶点扩展成房间
				if ( i == 0 )
					this.SetIndexToWalkable( graph2, ( int )v0.x, ( int )v0.y, random.Next( 1, 3 ), random.Next( 1, 3 ), walkablesHs );
				this.SetIndexToWalkable( graph2, ( int )v1.x, ( int )v1.y, random.Next( 1, 3 ), random.Next( 1, 3 ), walkablesHs );

			}

			this.walkables = walkablesHs.ToArray();
		}

		public void Dispose()
		{
			foreach ( Tile tile in this._tiles )
				tile.Dispose();
			Array.Clear( this._tiles, 0, this._tiles.Length );
			this._tiles = null;
			this.walkables = null;
			this._tmpBounds = null;
			this._tmpIndices = null;
			this._tmpIndices2 = null;
		}

		private Graph2D CreateFullDigraph( FPseudoRandom random )
		{
			Graph2D graph = new Graph2D( this.row, this.col );
			for ( int i = 1; i < this.row - 1; i++ )
			{
				for ( int j = 1; j < this.col - 1; j++ )
				{
					int cur = i * this.col + j;
					GraphNode node = graph[cur];
					if ( j < this.col - 2 )
						node.AddEdge( cur, cur + 1, random.NextFloat() );
					if ( j > 1 )
						node.AddEdge( cur, cur - 1, random.NextFloat() );
				}
			}
			for ( int i = 1; i < this.col - 1; i++ )
			{
				for ( int j = 1; j < this.row - 1; j++ )
				{
					int cur = j * this.col + i;
					GraphNode node = graph[cur];
					if ( j < this.row - 2 )
						node.AddEdge( cur, cur + this.col, random.NextFloat() );
					if ( j > 1 )
						node.AddEdge( cur, cur - this.col, random.NextFloat() );
				}
			}
			return graph;
		}

		private int RandomIndexWithinBorder( FPseudoRandom random )
		{
			int x = random.Next( 1, this.col - 1 );
			int y = random.Next( 1, this.row - 1 );
			return y * this.col + x;
		}

		private int GetFarthestToStartIndex( int start, int[] candidate )
		{
			int m = 0, d0 = 0;
			int sx = start % this.col;
			int sz = -start / this.col;
			for ( int i = 0; i < candidate.Length; i++ )
			{
				int index = candidate[i];
				int x = index % this.col;
				int z = -index / this.col;
				int dx = x - sx;
				int dz = z - sz;
				int d = dx * dx + dz * dz;
				if ( d >= d0 )
				{
					d0 = d;
					m = i;
				}
			}
			return candidate[m];
		}

		private List<Delaunay.DVertex> GenVertices( int vertexCount, float innerRadius, float outterRadius, int slpiceSegment, FPseudoRandom rnd )
		{
			float cx = .5f * this.col;
			float cy = .5f * this.row;
			float hw = .5f * ( this.col - 1 );
			float hh = .5f * ( this.row - 1 );
			List<Delaunay.DVertex> vertices = new List<Delaunay.DVertex>();
			for ( int i = 0; i < vertexCount; i++ )
			{
				//横轴半径
				int r1 = ( int )( hw * ( rnd.NextFloat() * ( outterRadius - innerRadius ) + innerRadius ) );
				//纵轴半径
				int r2 = ( int )( hh * ( rnd.NextFloat() * ( outterRadius - innerRadius ) + innerRadius ) );
				//随机角度
				float sita = rnd.NextFloat() * MathUtils.PI * 2f;
				//椭圆极坐标参数方程
				int x = ( int )( cx + r1 * MathUtils.Cos( sita ) );
				int y = ( int )( cy + r2 * MathUtils.Sin( sita ) );

				vertices.Add( new Delaunay.DVertex( x, y ) );
			}
			//取半径的平均单位比例
			float avgRadius = ( outterRadius + innerRadius ) * 0.5f;
			//椭圆周长近似公式
			float p = MathUtils.Sqrt( 0.5f * avgRadius * avgRadius * ( hw * hw + hh * hh ) );
			//按给定的分段数计算阈值
			float threshold = 2f * MathUtils.PI * p / slpiceSegment;
			//去掉距离较近的顶点
			TripVertex( vertices, threshold );
			return vertices;
		}

		private static void TripVertex( List<Delaunay.DVertex> vertices, float threshold )
		{
			threshold *= threshold;
			for ( int i = 0; i < vertices.Count; i++ )
			{
				Delaunay.DVertex v0 = vertices[i];
				for ( int j = vertices.Count - 1; j > i; j-- )
				{
					Delaunay.DVertex v1 = vertices[j];
					float dx = v0.x - v1.x;
					float dy = v0.y - v1.y;
					if ( dx * dx + dy * dy <= threshold )
						vertices.RemoveAt( j );
				}
			}
		}

		private void CreateTiles( Graph2D graph )
		{
			this._tiles = new Tile[graph.size];
			for ( int i = 0; i < graph.row; i++ )
			{
				for ( int j = 0; j < graph.col; j++ )
				{
					int index = graph.GetNode( i, j ).index;
					Tile tile = new Tile( index );
					tile.flag = Tile.Flag.Obstacle;
					if ( i == 0 || i == graph.row - 1 )
						tile.flag = Tile.Flag.Border;
					if ( j == 0 || j == graph.col - 1 )
						tile.flag = Tile.Flag.Border;
					tile.aabb = new FBounds
					{
						min = new FVec3( j, 0, -( i + 1 ) ) * this.scale,
						max = new FVec3( j + 1, 1f, -i ) * this.scale.z
					};
					this._tiles[index] = tile;
				}
			}
		}

		private void SetIndexToWalkable( Graph2D graph, int cx, int cy, int ex, int ey, HashSet<int> walkables )
		{
			if ( cx + ex > this.col - 1 )
				cx = this.col - 1 - ex;
			if ( cy + ey > this.row - 1 )
				cy = this.row - 1 - ey;
			for ( int i = 0; i < ex; i++ )
			{
				int x = cx + i;
				for ( int j = 0; j < ey; j++ )
				{
					int y = cy + j;
					this.SetTileWalkable( graph.CoordToIndex( x, y ), walkables );
				}
			}
		}

		private void SetStart( Graph2D graph, int start, HashSet<int> walkables )
		{
			int startPointPlaceX = ( int )this.startPointPlace.x;
			int startPointPlaceY = ( int )this.startPointPlace.y;
			int[] coord = graph.IndexToCoord( start );
			if ( coord[0] + startPointPlaceX > this.col - 1 )
				coord[0] = this.col - 1 - startPointPlaceX;
			if ( coord[1] + startPointPlaceY > this.row - 1 )
				coord[1] = this.row - 1 - startPointPlaceY;
			for ( int i = 0; i < startPointPlaceX; i++ )
			{
				int x = coord[0] + i;
				for ( int j = 0; j < startPointPlaceY; j++ )
				{
					int y = coord[1] + j;
					this.SetTileWalkable( graph.CoordToIndex( x, y ), walkables );
				}
			}

			//计算起始范围的中点坐标(世界坐标)
			float i0 = ( coord[0] + startPointPlaceX - coord[0] ) * 0.5f + coord[0];
			float i1 = -( coord[1] + startPointPlaceY - coord[1] ) * 0.5f - coord[1];
			this.centerOfStartRange = this._localToWorld.TransformPoint( new FVec3( i0, 0, i1 ) );
		}

		private void SetPathWalkable( int[] path, HashSet<int> walkables )
		{
			int count = path.Length;
			for ( int i = 0; i < count; i++ )
				this.SetTileWalkable( path[i], walkables );
		}

		private void SetTileWalkable( int index, HashSet<int> walkables )
		{
			Tile tile = this._tiles[index];
			tile.flag = Tile.Flag.Walkable;
			walkables.Add( index );
		}

		private FBounds GetTileBounds( int index )
		{
			FVec3 p = this.LocalIndexToGlobalPoint( index );
			p.z -= this.scale.z;
			FBounds bounds = new FBounds();
			bounds.min = p;
			bounds.max = bounds.min + this.scale;
			return bounds;
		}

		public int CoordToIndex( FVec3 p )
		{
			return -( int )p.z * this.col + ( int )p.x; ;
		}

		public FVec3 IndexToCoord( int index )
		{
			return new FVec3( index % this.col, 0, -index / this.col );
		}

		public FVec3 LocalIndexToGlobalPoint( int index )
		{
			return this._localToWorld.TransformPoint( this.IndexToCoord( index ) );
		}

		public FVec3 LocalPointToGlobalPoint( FVec3 point )
		{
			return this._localToWorld.TransformPoint( point );
		}

		public int GlobalPointToLocalIndex( FVec3 point )
		{
			return this.CoordToIndex( this._worldToLocal.TransformPoint( point ) );
		}

		public FVec3 GlobalPointToLocalPoint( FVec3 point )
		{
			return this._worldToLocal.TransformPoint( point );
		}

		public int GetRandomIndexInWalkables( FPseudoRandom random )
		{
			return this.walkables[random.Next( 0, this.walkables.Length )];
		}

		public FVec3 GetRandomPointInWalkables( FPseudoRandom random )
		{
			int index = this.GetRandomIndexInWalkables( random );
			FVec3 coord = this.IndexToCoord( index );
			coord.x *= this.scale.x;
			coord.z *= this.scale.z;
			return new FVec3( random.NextFix64( coord.x, coord.x + this.scale.x ), coord.y,
							  random.NextFix64( coord.z - this.scale.z, coord.z ) );
		}

		private void GetRightNeighborBounds( ITileObject self, int tileIndex, ref List<FBounds> boundsList )
		{
			this._tmpIndices[0] = tileIndex;
			this._tmpIndices[1] = tileIndex - this.col;
			this._tmpIndices[2] = tileIndex + this.col;
			this._tmpIndices[3] = tileIndex + 1;
			this._tmpIndices[4] = tileIndex - this.col + 1;
			this._tmpIndices[5] = tileIndex + this.col + 1;
			this.GetColliderBoundsWithinIndices( self, this._tmpIndices, ref boundsList );
		}

		private void GetLeftNeighborBounds( ITileObject self, int tileIndex, ref List<FBounds> boundsList )
		{
			this._tmpIndices[0] = tileIndex;
			this._tmpIndices[1] = tileIndex - this.col;
			this._tmpIndices[2] = tileIndex + this.col;
			this._tmpIndices[3] = tileIndex - 1;
			this._tmpIndices[4] = tileIndex - this.col - 1;
			this._tmpIndices[5] = tileIndex + this.col - 1;
			this.GetColliderBoundsWithinIndices( self, this._tmpIndices, ref boundsList );
		}

		private void GetUpNeighborBounds( ITileObject self, int tileIndex, ref List<FBounds> boundsList )
		{
			this._tmpIndices[0] = tileIndex;
			this._tmpIndices[1] = tileIndex - 1;
			this._tmpIndices[2] = tileIndex + 1;
			this._tmpIndices[3] = tileIndex - this.col;
			this._tmpIndices[4] = tileIndex - this.col - 1;
			this._tmpIndices[5] = tileIndex - this.col + 1;
			this.GetColliderBoundsWithinIndices( self, this._tmpIndices, ref boundsList );
		}

		private void GetDownNeighborBounds( ITileObject self, int tileIndex, ref List<FBounds> boundsList )
		{
			this._tmpIndices[0] = tileIndex;
			this._tmpIndices[1] = tileIndex - 1;
			this._tmpIndices[2] = tileIndex + 1;
			this._tmpIndices[3] = tileIndex + this.col;
			this._tmpIndices[4] = tileIndex + this.col - 1;
			this._tmpIndices[5] = tileIndex + this.col + 1;
			this.GetColliderBoundsWithinIndices( self, this._tmpIndices, ref boundsList );
		}

		private void GetColliderBoundsWithinIndices( ITileObject self, int[] indices, ref List<FBounds> boundsList )
		{
			for ( int i = 0; i < 6; i++ )
			{
				int index = indices[i];
				Tile tile = this._tiles[index];
				if ( tile.flag != Tile.Flag.Walkable )
					boundsList.Add( tile.aabb );
				this.GetCollidersInTile( self, index, ref boundsList );
			}
		}

		private void GetCollidersInTile( ITileObject self, int tileIndex, ref List<FBounds> boundsList )
		{
			List<ITileObject> objects = this._tiles[tileIndex].objects;
			int count = objects.Count;
			for ( int i = 0; i < count; i++ )
			{
				ITileObject tileObject = objects[i];
				if ( tileObject != self && tileObject.enableCollision )
					boundsList.Add( tileObject.worldBounds );
			}
		}

		private Fix64 GetMinIntersectTime( FBounds movingBounds, List<FBounds> staticBounds, Fix64 d, FBounds.Axis axis )
		{
			Fix64 minT = Fix64.MaxValue;
			foreach ( FBounds b in staticBounds )
			{
				if ( !b.IntersectMovingBoundsByAxis( movingBounds, d, axis, out Fix64 t ) )
					continue;

				if ( t >= minT )
					continue;

				minT = t;
			}
			return minT > Fix64.One ? Fix64.One : minT;
		}

		public Fix64 MoveDetection( ITileObject self, Fix64 distance, int direction )
		{
			if ( distance == Fix64.Zero )
				return Fix64.Zero;

			FBounds bounds = self.worldBounds;
			FVec3 position = bounds.center;
			position.y = Fix64.Zero;
			int girdIndex = this.GlobalPointToLocalIndex( position );

			Fix64 minT;
			if ( direction == 0 )//x轴
			{
				if ( distance > Fix64.Zero )//右
					this.GetRightNeighborBounds( self, girdIndex, ref this._tmpBounds );
				else if ( distance < Fix64.Zero )//左
					this.GetLeftNeighborBounds( self, girdIndex, ref this._tmpBounds );

				minT = this.GetMinIntersectTime( bounds, this._tmpBounds, distance, FBounds.Axis.X );
				this._tmpBounds.Clear();
			}
			else//z轴
			{
				if ( distance > Fix64.Zero )
					this.GetUpNeighborBounds( self, girdIndex, ref this._tmpBounds );
				else if ( distance < Fix64.Zero )
					this.GetDownNeighborBounds( self, girdIndex, ref this._tmpBounds );

				minT = this.GetMinIntersectTime( bounds, this._tmpBounds, distance, FBounds.Axis.Z );
				this._tmpBounds.Clear();
			}
			distance = minT * distance;

			return distance;
		}

		public FVec3 RestrictInBounds( ITileObject tileObject, bool ignoreObstacle )
		{
			int index = this.GlobalPointToLocalIndex( tileObject.position );
			FBounds bounds = this.GetTileBounds( index );
			FVec3 min = bounds.min;
			FVec3 max = bounds.max;
			FVec3 halfSize = tileObject.worldBounds.size * Fix64.Half;
			FVec3 p = tileObject.position;
			if ( ( ignoreObstacle || this._tiles[index + 1].flag != Tile.Flag.Walkable ) &&
				 p.x > max.x - halfSize.x )
				p.x = max.x - halfSize.x;
			else if ( ( ignoreObstacle || this._tiles[index - 1].flag != Tile.Flag.Walkable ) &&
					  p.x < min.x + halfSize.x )
				p.x = min.x + halfSize.x;
			if ( ( ignoreObstacle || this._tiles[index - this.col].flag != Tile.Flag.Walkable ) &&
				 p.z > max.z - halfSize.z )
				p.z = max.z - halfSize.z;
			else if ( ( ignoreObstacle || this._tiles[index + this.col].flag != Tile.Flag.Walkable ) &&
					  p.z < min.z + halfSize.z )
				p.z = min.z + halfSize.z;
			return p;
		}

		public void UpdateObjectPosition( ITileObject tileObject, bool removeOnly )
		{
			if ( tileObject.tileIndex >= 0 )
				this._tiles[tileObject.tileIndex].RemoveObject( tileObject );
			if ( removeOnly )
				return;
			int index = this.GlobalPointToLocalIndex( tileObject.position );
			this._tiles[index].AddObject( tileObject );
			tileObject.tileIndex = index;
		}

		public void GetTileObjectsAround( int tileIndex, ref List<ITileObject> objects )
		{
			this._tmpIndices2[0] = tileIndex;
			this._tmpIndices2[1] = tileIndex - 1;
			this._tmpIndices2[2] = tileIndex + 1;
			this._tmpIndices2[3] = tileIndex - this.col;
			this._tmpIndices2[4] = tileIndex - this.col - 1;
			this._tmpIndices2[5] = tileIndex - this.col + 1;
			this._tmpIndices2[6] = tileIndex + this.col;
			this._tmpIndices2[7] = tileIndex + this.col - 1;
			this._tmpIndices2[8] = tileIndex + this.col + 1;
			for ( int i = 0; i < 9; i++ )
				objects.AddRange( this._tiles[this._tmpIndices2[i]].objects );
		}
	}
}