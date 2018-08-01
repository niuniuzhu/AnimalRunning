using Core.FMath;
using Logic;
using Logic.Event;
using System.Collections.Generic;

namespace View.Event
{
	public static class UIEventType
	{
		public const int COUNT_DOWN = 10012;
		public const int WIN = 10013;

		public const int ENTITY_CREATED = 10020;
		public const int ENTITY_DESTROIED = 10021;
		public const int ENTITY_ATTR_CHANGED = 10023;

		public const int PICK_ITEM = 10030;
		public const int ITEM_USED = 10031;
	}

	public class UIEvent : BaseEvent
	{
		#region pool support
		private static readonly Stack<UIEvent> POOL = new Stack<UIEvent>();

		private static UIEvent Get()
		{
			return POOL.Count > 0 ? POOL.Pop() : new UIEvent();
		}

		private static void Release( UIEvent element )
		{
			POOL.Push( element );
		}

		public override void Release()
		{
			this.target = null;
			Release( this );
		}

		public static void CountDown( int num, int countDown )
		{
			UIEvent e = Get();
			e.type = UIEventType.COUNT_DOWN;
			e.i0 = num;
			e.i1 = countDown;
			e.BeginInvoke();
		}

		public static void Win( int team )
		{
			UIEvent e = Get();
			e.type = UIEventType.WIN;
			e.i0 = team;
			e.BeginInvoke();
		}

		public static void EntityCreated( CEntity target )
		{
			UIEvent e = Get();
			e.type = UIEventType.ENTITY_CREATED;
			e.target = target;
			e.Invoke();
		}

		public static void EntityDestroied( CEntity target )
		{
			UIEvent e = Get();
			e.type = UIEventType.ENTITY_DESTROIED;
			e.target = target;
			e.Invoke();
		}

		public static void EntityAttrChanged( CEntity target, Attr attr, object value )
		{
			UIEvent e = Get();
			e.type = UIEventType.ENTITY_ATTR_CHANGED;
			e.target = target;
			e.attr = attr;
			e.o0 = value;
			e.Invoke();
		}

		public static void PickItem( CChampion target, string itemId )
		{
			UIEvent e = Get();
			e.type = UIEventType.PICK_ITEM;
			e.target = target;
			e.itemId = itemId;
			e.Invoke();
		}

		public static void ItemUsed( CChampion target, bool result )
		{
			UIEvent e = Get();
			e.type = UIEventType.ITEM_USED;
			e.target = target;
			e.b0 = result;
			e.Invoke();
		}

		#endregion

		#region all values
		public int i0;
		public int i1;
		public Fix64 f0;
		public Fix64 f1;
		public bool b0;
		public bool b1;
		public object o0;
		#endregion

		#region entity created/destroied event
		public CEntity target;
		public string itemId;
		#endregion

		public Attr attr;
	}
}