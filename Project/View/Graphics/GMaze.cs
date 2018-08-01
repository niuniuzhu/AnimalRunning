using System.Collections.Generic;
using Logic;
using UnityEngine;
using UnityEngine.Rendering;

namespace View.Graphics
{
	public class GMaze
	{
		private const int MAX_PLAYER_IN_FOW = 8;
		private const float TILE_HEIGHT = -1;

		public int row { get; }
		public int col { get; }
		public Vector3 scale { get; }
		public Vector3 offset { get; }

		private CBattle _battle;
		private Matrix4x4 _localToWorld;
		private Matrix4x4 _worldToLocal;
		private GameObject _root;
		//private Transform _ground;
		//private Material _groundMat;
		private Transform _FOWPlane;
		private Material _FOWPlaneMat;
		private Vector4[] _tmpPos = new Vector4[MAX_PLAYER_IN_FOW];
		private float[] _tmpFov = new float[MAX_PLAYER_IN_FOW];
		private List<CChampion> _tmpEntities = new List<CChampion>();
		private bool _bright;

		public GMaze( CBattle battle, Vector3 scale, Vector3 offset, int row, int col )
		{
			this._battle = battle;
			this.scale = scale;
			this.offset = offset;
			this.row = row;
			this.col = col;

			this._localToWorld = Matrix4x4.TRS( this.offset, Quaternion.identity, this.scale );
			this._worldToLocal = this._localToWorld.inverse;

			this._root = new GameObject( "Maze_root" );
			this._root.isStatic = true;
			Object.DontDestroyOnLoad( this._root );

			//this._ground = Object.Instantiate( Resources.Load<GameObject>( "Prefabs/Water0" ) ).transform;
			//this._ground.localScale = new Vector3( this.col * this.scale.x * 2, this.row * this.scale.z * 2, 1 );//because rotation.x = 90
			//this._ground.position = new Vector3( this.col * this.scale.x * 0.5f, -0.4f, -this.row * this.scale.z * 0.5f );
			//this._ground.SetParent( this._root.transform );
			//this._groundMat = this._ground.GetComponent<MeshRenderer>().material;
			//this._groundMat.mainTextureScale = new Vector2( this.col * this.scale.x * 2, this.row * this.scale.z * 2 );

			this._FOWPlane = Object.Instantiate( Resources.Load<GameObject>( "Prefabs/FOWPlane" ) ).transform;
			this._FOWPlane.localScale = new Vector3( this.col * this.scale.x * 2, this.row * this.scale.z * 2, 1 );//because rotation.x = 90
			this._FOWPlane.position = new Vector3( this.col * this.scale.x * 0.5f, 2f, -this.row * this.scale.z * 0.5f );
			this._FOWPlane.SetParent( this._root.transform );
			this._FOWPlaneMat = this._FOWPlane.GetComponent<MeshRenderer>().material;
			this._FOWPlaneMat.SetFloat( "_Frequency", ( float )this._battle.FOWFogFrequency );
			this._FOWPlaneMat.SetFloat( "_Amplitude", ( float )this._battle.FOWFogAmplitude );
		}

		public void Dispose()
		{
			if ( this._root != null )
			{
				//Object.Destroy( this._ground.gameObject );
				//this._ground = null;
				//Object.Destroy( this._groundMat );
				//this._groundMat = null;
				Object.Destroy( this._FOWPlane.gameObject );
				this._FOWPlane = null;
				Object.Destroy( this._FOWPlaneMat );
				this._FOWPlaneMat = null;
				MeshRenderer mr = this._root.GetComponent<MeshRenderer>();
				Object.Destroy( mr.material );
				Object.Destroy( this._root );
				this._root = null;
			}
			this._battle = null;
			this._tmpPos = null;
			this._tmpFov = null;
			this._tmpEntities = null;
			this._bright = false;
		}

		public void Set( int[] walkables )
		{
			MeshFilter mf = this._root.AddComponent<MeshFilter>();
			mf.mesh = this.GetMesh( walkables );
			MeshRenderer mr = this._root.AddComponent<MeshRenderer>();
			mr.shadowCastingMode = ShadowCastingMode.Off;
			mr.receiveShadows = false;
			Material resources = Resources.Load<Material>( "Materials/Tile/" + this._battle.surfaceMat );
			if ( resources == null )
				resources = Resources.Load<Material>( "Materials/Tile/failed" );
			mr.material = Object.Instantiate( resources );
		}

		private Mesh GetMesh( int[] walkables )
		{
			int c1 = walkables.Length;
			int c2 = c1 * 16;
			Vector3[] vertices = new Vector3[c2];
			Vector3[] normals = new Vector3[c2];
			Vector2[] uvs = new Vector2[c2];
			int[] indices = new int[c1 * 24];
			//4 tiles per line
			const float widthPerTile = 0.25f;// 1/4
			const float ey = 1f;
			const float sy1 = 9f / 128;//8 pixel used by bottom, 8/128
			const float sy0 = 0f;
			for ( int i = 0; i < c1; i++ )
			{
				int index = walkables[i];
				Vector3 position = new Vector3( ( index % this.col ) * this.scale.x + this.offset.x, 0,
												-( index / this.row ) * this.scale.z + this.offset.z );
				int m = i * 16;
				float px = position.x + this.scale.x;
				float pz = position.z - this.scale.z;
				//top
				vertices[m + 0] = position;
				vertices[m + 1] = new Vector3( px, position.y, position.z );
				vertices[m + 2] = new Vector3( position.x, position.y, pz );
				vertices[m + 3] = new Vector3( px, position.y, pz );
				//front
				vertices[m + 4] = vertices[m + 2];
				vertices[m + 5] = vertices[m + 3];
				vertices[m + 6] = vertices[m + 4] + new Vector3( 0, TILE_HEIGHT, 0 );
				vertices[m + 7] = vertices[m + 5] + new Vector3( 0, TILE_HEIGHT, 0 );
				//right
				vertices[m + 8] = vertices[m + 1];
				vertices[m + 9] = vertices[m + 3];
				vertices[m + 10] = vertices[m + 8] + new Vector3( 0, TILE_HEIGHT, 0 );
				vertices[m + 11] = vertices[m + 9] + new Vector3( 0, TILE_HEIGHT, 0 );
				//left
				vertices[m + 12] = vertices[m + 0];
				vertices[m + 13] = vertices[m + 2];
				vertices[m + 14] = vertices[m + 12] + new Vector3( 0, TILE_HEIGHT, 0 );
				vertices[m + 15] = vertices[m + 13] + new Vector3( 0, TILE_HEIGHT, 0 );
				//normals
				normals[m + 0] = Vector3.up;
				normals[m + 1] = Vector3.up;
				normals[m + 2] = Vector3.up;
				normals[m + 3] = Vector3.up;
				normals[m + 4] = Vector3.back;
				normals[m + 5] = Vector3.back;
				normals[m + 6] = Vector3.back;
				normals[m + 7] = Vector3.back;
				normals[m + 8] = Vector3.right;
				normals[m + 9] = Vector3.right;
				normals[m + 10] = Vector3.right;
				normals[m + 11] = Vector3.right;
				normals[m + 12] = Vector3.left;
				normals[m + 13] = Vector3.left;
				normals[m + 14] = Vector3.left;
				normals[m + 15] = Vector3.left;

				int rndX = Random.Range( 0, 4 );
				float sx = rndX * widthPerTile;
				float ex = sx + widthPerTile;
				//top
				uvs[m + 0] = new Vector2( sx, ey );
				uvs[m + 1] = new Vector2( ex, ey );
				uvs[m + 2] = new Vector2( sx, sy1 );
				uvs[m + 3] = new Vector2( ex, sy1 );
				//front
				uvs[m + 4] = uvs[m + 2];
				uvs[m + 5] = uvs[m + 3];
				uvs[m + 6] = new Vector2( sx, sy0 );
				uvs[m + 7] = new Vector2( ex, sy0 );
				//right
				uvs[m + 8] = uvs[m + 4];
				uvs[m + 9] = uvs[m + 5];
				uvs[m + 10] = uvs[m + 6];
				uvs[m + 11] = uvs[m + 7];
				//left
				uvs[m + 12] = uvs[m + 4];
				uvs[m + 13] = uvs[m + 5];
				uvs[m + 14] = uvs[m + 6];
				uvs[m + 15] = uvs[m + 7];

				int n = i * 24;
				//top
				indices[n + 0] = m + 0;
				indices[n + 1] = m + 1;
				indices[n + 2] = m + 2;
				indices[n + 3] = m + 3;
				indices[n + 4] = m + 2;
				indices[n + 5] = m + 1;
				//front
				indices[n + 6] = m + 4;
				indices[n + 7] = m + 5;
				indices[n + 8] = m + 6;
				indices[n + 9] = m + 7;
				indices[n + 10] = m + 6;
				indices[n + 11] = m + 5;
				//right
				indices[n + 12] = m + 10;
				indices[n + 13] = m + 9;
				indices[n + 14] = m + 8;
				indices[n + 15] = m + 9;
				indices[n + 16] = m + 10;
				indices[n + 17] = m + 11;
				//left
				indices[n + 18] = m + 12;
				indices[n + 19] = m + 13;
				indices[n + 20] = m + 14;
				indices[n + 21] = m + 15;
				indices[n + 22] = m + 14;
				indices[n + 23] = m + 13;
			}
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.uv = uvs;
			mesh.triangles = indices;
			mesh.normals = normals;
			return mesh;
		}

		public int CoordToIndex( Vector3 p )
		{
			return -( int )p.z * this.col + ( int )p.x; ;
		}

		public Vector3 IndexToCoord( int index )
		{
			return new Vector3( index % this.col, 0, -index / this.col );
		}

		public Vector3 LocalIndexToGlobalPoint( int index )
		{
			return this._localToWorld.MultiplyPoint( this.IndexToCoord( index ) );
		}

		public Vector3 LocalPointToGlobalPoint( Vector2 point )
		{
			return this._localToWorld.MultiplyPoint( new Vector3( point.x, 0, -point.y ) );
		}

		public int GlobalPointToLocalIndex( Vector3 point )
		{
			return this.CoordToIndex( this._worldToLocal.MultiplyPoint( point ) );
		}

		public Vector3 GlobalPointToLocalPoint( Vector3 point )
		{
			return this._worldToLocal.MultiplyPoint( point );
		}

		public void Bright()
		{
			this._bright = true;
		}

		public void Update( UpdateContext context )
		{
			if ( CPlayer.instance != null )
			{
				this._battle.GetChampionsNearby( CPlayer.instance, TargetType.Teamate,
												 ( float )this._battle.FOWDistanceToPlayer, MAX_PLAYER_IN_FOW,
												 ref this._tmpEntities );
				int count = this._tmpEntities.Count;
				for ( int i = 0; i < count; i++ )
				{
					CChampion champion = this._tmpEntities[i];
					Vector3 screenPos = this._battle.camera.WorldToScreenPoint( champion.position );
					Ray ray = this._battle.camera.ScreenPointToRay( screenPos );
					Physics.Raycast( ray, out RaycastHit hit, 999, 1 << 8 );
					this._tmpPos[i] = hit.point;
					this._tmpFov[i] = ( float )champion.fov;
				}
				this._FOWPlaneMat.SetInt( "_NumPlayers", count );
				this._FOWPlaneMat.SetVectorArray( "_PlayerPositions", this._tmpPos );
				this._FOWPlaneMat.SetFloatArray( "_PlayerFovs", this._tmpFov );
				this._FOWPlaneMat.SetFloat( "_FogRadius", ( float )CPlayer.instance.fov );
				this._tmpEntities.Clear();
			}
			if ( this._bright )
			{
				Color color = this._FOWPlaneMat.GetColor( "_Color" );
				color = Color.Lerp( color, new Color( 0, 0, 0, 0 ), ( float )context.deltaTime * 0.4f );
				this._FOWPlaneMat.SetColor( "_Color", color );
			}
		}
	}
}