using Core.FMath;
using Core.Math;
using Logic.FSM;
using System.Collections.Concurrent;

namespace Logic.Event
{
	public static class SyncEventType
	{
		public const int GEN_MAZE = 10;
		public const int DESTROY_BATTLE = 11;
		public const int COUNT_DOWN = 12;
		public const int WIN = 13;
		public const int TERMINUS = 14;

		public const int ENTITY_CREATED = 20;
		public const int ENTITY_ADDED_TO_BATTLE = 21;
		public const int ENTITY_REMOVE_FROM_BATTLE = 22;
		public const int ENTITY_STATE_CHANGED = 23;
		public const int ENTITY_SYNC_PROPS = 24;

		public const int PICK_ITEM = 30;
		public const int USE_ITEM = 31;

		public const int BUFF_CREATED = 40;
		public const int BUFF_DESTROIED = 41;

		public const int SET_FRAME_ACTION = 99;

		public const int DEBUG_DRAW = 200;
	}

	public class SyncEvent : BaseEvent
	{
		#region pool support
		private static readonly ConcurrentStack<SyncEvent> POOL = new ConcurrentStack<SyncEvent>();

		private static SyncEvent Get()
		{
			if ( !POOL.IsEmpty )
			{
				POOL.TryPop( out SyncEvent e );
				return e;
			}
			return new SyncEvent();
		}

		private static void Release( SyncEvent element )
		{
			POOL.Push( element );
		}
		#endregion

		public override void Release()
		{
			Release( this );
		}

		public static void HandleFrameAction()
		{
			SyncEvent e = Get();
			e.type = SyncEventType.SET_FRAME_ACTION;
			e.BeginInvoke();
		}

		public static void GenMaze( int[] walkables, int startIndex, int endIndex )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.GEN_MAZE;
			e.walkables = walkables;
			e.i0 = startIndex;
			e.i1 = endIndex;
			e.BeginInvoke();
		}

		public static void DestroyBattle()
		{
			SyncEvent e = Get();
			e.type = SyncEventType.DESTROY_BATTLE;
			e.BeginInvoke();
		}

		public static void CountDown( int num, int countDown )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.COUNT_DOWN;
			e.i0 = num;
			e.i1 = countDown;
			e.BeginInvoke();
		}

		public static void Win( int team )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.WIN;
			e.i0 = team;
			e.BeginInvoke();
		}

		public static void Terminus( string entityId )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.TERMINUS;
			e.targetId = entityId;
			e.BeginInvoke();
		}

		public static void CreateEntity( string type, EntityParam param )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.ENTITY_CREATED;
			e.entityType = type;
			e.param = param;
			e.BeginInvoke();
		}

		public static void EntityRemoveFromBattle( string entityId )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.ENTITY_REMOVE_FROM_BATTLE;
			e.targetId = entityId;
			e.BeginInvoke();
		}

		public static void EntityAddedToBattle( string entityId )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.ENTITY_ADDED_TO_BATTLE;
			e.targetId = entityId;
			e.BeginInvoke();
		}

		public static SyncEvent BeginSyncProps( string targetId )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.ENTITY_SYNC_PROPS;
			e.attrCount = 0;
			e.targetId = targetId;
			return e;
		}

		public static void EndSyncProps( SyncEvent e )
		{
			e.BeginInvoke();
		}

		public static void AddSyncProp( SyncEvent e, Attr attr, object value )
		{
			e.attrs[e.attrCount] = attr;
			e.attrValues[e.attrCount] = value;
			++e.attrCount;
		}

		public static void ChangeState( string targetId, FSMStateType type, bool force = false, params object[] param )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.ENTITY_STATE_CHANGED;
			e.targetId = targetId;
			e.stateType = type;
			e.forceChange = force;
			e.stateParam = param;
			e.BeginInvoke();
		}

		public static void PickItem( string targetId, string itemId )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.PICK_ITEM;
			e.targetId = targetId;
			e.genericId = itemId;
			e.BeginInvoke();
		}

		public static void UseItem( string targetId, bool result )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.USE_ITEM;
			e.targetId = targetId;
			e.b0 = result;
			e.BeginInvoke();
		}

		public static void CreateBuff( string buffId, string casterId, string targetId )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.BUFF_CREATED;
			e.genericId = buffId;
			e.casterId = casterId;
			e.targetId = targetId;
			e.BeginInvoke();
		}

		public static void DestroyBuff( string rid )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.BUFF_DESTROIED;
			e.genericId = rid;
			e.BeginInvoke();
		}

		public static void DebugDraw( DebugDrawType type, FVec3 v0, FVec3 v1, Fix64 f, Color4 color )
		{
			SyncEvent e = Get();
			e.type = SyncEventType.DEBUG_DRAW;
			e.debugDrawType = type;
			e.dv1 = v0;
			e.dv2 = v1;
			e.df = f;
			e.dc = color;
			e.BeginInvoke();
		}

		#region map
		public int[] walkables;
		#endregion

		#region entity
		public EntityParam param;
		public string entityType;
		public string casterId;
		public string targetId;
		public string genericId;
		#endregion

		#region fsm state
		public FSMStateType stateType;
		public bool forceChange;
		public object[] stateParam;
		#endregion

		#region attr
		public readonly Attr[] attrs = new Attr[50];
		public readonly object[] attrValues = new object[50];
		public int attrCount;
		#endregion

		public int i0;
		public int i1;
		public Fix64 f0;
		public Fix64 f1;
		public bool b0;
		public bool b1;

		#region debug
		public enum DebugDrawType
		{
			Ray,
			Line,
			Cube,
			Sphere,
			WireCube,
			WireSphere
		}
		public DebugDrawType debugDrawType;
		public FVec3 dv1;
		public FVec3 dv2;
		public Color4 dc;
		public Fix64 df;
		#endregion
	}
}