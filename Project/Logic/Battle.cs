using Core.FMath;
using Logic.Event;
using Logic.Map;
using Logic.Misc;
using System.Collections.Generic;

namespace Logic
{
	public struct BattleParams
	{
		public int frameRate;
		public int framesPerKeyFrame;
		public string uid;
		public string id;
		public int rndSeed;
		public Player[] players;

		public struct Player
		{
			public string id;
			public string name;
			public string cid;
			public byte skin;
			public int team;
		}
	}

	public class Battle
	{
		public int frame { get; private set; }
		public Fix64 deltaTime { get; private set; }
		public Fix64 time { get; private set; }
		public Fix64 realTime { get; private set; }
		public string[] items => this._data.items;
		public int maxItemCount => this._data.maxItemCount;
		public Fix64[] itemUpdateInterval => this._data.itemUpdateInterval;
		public Maze maze => this._maze;
		public FPseudoRandom random => this._random;

		private readonly StartCountDown _startCountDown;
		private readonly Maze _maze;
		private readonly FPseudoRandom _random;
		private readonly BuffManager _buffManager;
		private readonly EntityManager _entityManager;
		private readonly UpdateContext _context;
		private readonly MapData _data;

		public Battle( BattleParams param )
		{
			this._data = ModelFactory.GetMapData( Utils.GetIDFromRID( param.id ) );
			this._random = new FPseudoRandom( param.rndSeed );
			this._buffManager = new BuffManager( this );
			this._entityManager = new EntityManager( this );
			this._context = new UpdateContext();
			this._startCountDown = new StartCountDown( this, this._data.startCountDown );
			this._maze = new Maze( this._random, this._data.scale, this._data.offset, this._data.row, this._data.col, this._data.startIndex,
								   this._data.endIndex, this._data.startPointPlace );
			SyncEvent.GenMaze( this._maze.walkables, this._maze.startIndex, this._maze.endIndex );
			this.CreateRails();
			this.CreateTerminus();
			this.CreatePlayers( param.players );
			this._entityManager.SupplyItems();
		}

		public void Dispose()
		{
			this._startCountDown.Dispose();
			this._buffManager.Dispose();
			this._entityManager.Dispose();
			this._maze.Dispose();
			SyncEvent.DestroyBattle();
		}

		public void Update( Fix64 realDeltaTime, Fix64 deltaTime )
		{
			++this.frame;

			this.deltaTime = deltaTime;
			this.realTime += realDeltaTime;
			this.time += this.deltaTime;

			this._context.deltaTime = this.deltaTime;
			this._context.time = this.time;
			this._context.frame = this.frame;

			this._startCountDown.Update( this._context );
			this._buffManager.Update( this._context );
			this._entityManager.Update( this._context );
		}

		private void CreateRails()
		{
			FVec3 worldOffset =
				this._maze.LocalPointToGlobalPoint( new FVec3( this._maze.startPointPlace[0] * Fix64.Half, Fix64.Zero,
															  this._maze.startPointPlace[1] * Fix64.Half ) );
			FVec3 center = this._maze.centerOfStartRange;
			FVec3[] positions = new FVec3[4];
			positions[0] = new FVec3( center.x, center.y, center.z + worldOffset.z );
			positions[1] = new FVec3( center.x + worldOffset.x, center.y, center.z );
			positions[2] = new FVec3( center.x, center.y, center.z - worldOffset.z );
			positions[3] = new FVec3( center.x - worldOffset.x, center.y, center.z );

			FVec3[] offset = new FVec3[4];
			offset[0] = new FVec3( 0, 0, 0.5f );
			offset[1] = new FVec3( 0.5f, 0, 0 );
			offset[2] = new FVec3( 0, 0, -0.5f );
			offset[3] = new FVec3( -0.5f, 0, 0 );

			FVec3[] directions = new FVec3[4];
			directions[0] = FVec3.backward;
			directions[1] = FVec3.left;
			directions[2] = FVec3.forward;
			directions[3] = FVec3.right;

			for ( int i = 0; i < 4; i++ )
			{
				EntityParam param = new EntityParam();
				param.rid = ( i % 2 == 0 ? "_c0@" : "_c0_1@" ) + i;
				param.position = positions[i];
				param.direction = directions[i];
				Rail rail = this._entityManager.Create<Rail>( param );
				rail.position += offset[i] * rail.bounds.size;
			}
		}

		private void CreateTerminus()
		{
			FVec3 endPos = this._maze.IndexToCoord( this._maze.endIndex );
			endPos.x += Fix64.Half;
			endPos.z -= Fix64.Half;
			endPos = this._maze.LocalPointToGlobalPoint( endPos );
			EntityParam param = new EntityParam();
			param.rid = this._random.IdHash( "_c0_2" );
			param.position = endPos;
			param.direction = FVec3.forward;
			Terminus terminus = this._entityManager.Create<Terminus>( param );
		}

		internal void DestroyRails()
		{
			int count = this._entityManager.entityCount;
			for ( int i = 0; i < count; i++ )
			{
				Entity entity = this._entityManager.GetEntityAt( i );
				if ( entity.id == "_c0" ||
					 entity.id == "_c0_1" )
					entity.MarkToDestroy();
			}
		}

		private void CreatePlayers( BattleParams.Player[] players )
		{
			FVec3 worldOffset =
				this._maze.LocalPointToGlobalPoint( new FVec3( this._maze.startPointPlace[0] * Fix64.Half, Fix64.Zero,
															  this._maze.startPointPlace[1] * Fix64.Half ) );
			FVec3 center = this._maze.centerOfStartRange;
			int count = players.Length;
			for ( int i = 0; i < count; i++ )
			{
				BattleParams.Player player = players[i];
				FVec2 rndCircle = this._random.onUnitCircle;
				EntityParam entityParam = new EntityParam
				{
					rid = player.cid + "@" + player.id,
					uid = player.id,
					skin = player.skin,
					team = player.team,
					position = new FVec3( this._random.NextFix64( center.x - worldOffset.x, center.x + worldOffset.x ),
										 center.y,
										 this._random.NextFix64( center.z - worldOffset.z, center.z + worldOffset.z ) ),
					direction = new FVec3( rndCircle.x, Fix64.Zero, rndCircle.y )
				};
				Champion champion = this._entityManager.Create<Champion>( entityParam );
				champion.position = this.maze.RestrictInBounds( champion, true );
			}
		}

		public Item CreateItem( string rid, FVec3 position, FVec3 direction )
		{
			EntityParam entityParam = new EntityParam
			{
				rid = rid,
				uid = string.Empty,
				team = -1,
				position = position,
				direction = direction
			};
			Item item = this._entityManager.Create<Item>( entityParam );
			item.position = this._maze.RestrictInBounds( item, false );
			item.enableCollision = false;
			return item;
		}

		public Buff CreateBuff( string id, Champion caster, Champion target )
		{
			return this._buffManager.CreateBuff( id, caster, target );
		}

		public void DestroyBuff( string buffId )
		{
			this._buffManager.DestroyBuff( buffId );
		}

		public void GetChampionsNearby( Champion target, TargetType targetType, Fix64 radius, int maxNumber, ref List<Champion> champions )
		{
			this._entityManager.GetChampionsNearby( target, targetType, radius, maxNumber, ref champions );
		}

		public void ReachTerminus( Champion champion )
		{
			if ( champion.mazeResult != Champion.MazeResult.Nan )
				return;
			champion.BeginMove( FVec3.zero );
			SyncEvent.Terminus( champion.rid );
			champion.mazeResult = Champion.MazeResult.Win;
			int count = this._entityManager.championCount;
			for ( int i = 0; i < count; i++ )
			{
				Champion other = this._entityManager.GetChampionAt( i );
				if ( other.team == champion.team &&
					 other.mazeResult != Champion.MazeResult.Win )
					return;
			}
			for ( int i = 0; i < count; i++ )
			{
				Champion other = this._entityManager.GetChampionAt( i );
				if ( other.team != champion.team )
					other.mazeResult = Champion.MazeResult.Lose;
			}
			this.Win( champion.team );
		}

		private void Win( int team )
		{
			SyncEvent.Win( team );
		}

		public void HandleBeginMove( string rid, FVec3 direction )
		{
			Champion entity = this._entityManager.GetChampion( rid );
			entity.BeginMove( direction );
		}

		public void HandleUseItem( string rid )
		{
			Champion entity = this._entityManager.GetChampion( rid );
			bool result = entity.UseItem();
			SyncEvent.UseItem( rid, result );
		}
	}
}
