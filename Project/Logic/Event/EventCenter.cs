using Core.Structure;
using System.Collections.Generic;

namespace Logic.Event
{
	public static class EventCenter
	{
		public delegate void EventHandler( BaseEvent e );

		private static readonly Dictionary<int, List<EventHandler>> HANDLERS = new Dictionary<int, List<EventHandler>>();
		private static readonly SwitchQueue<BaseEvent> PENDING_LIST = new SwitchQueue<BaseEvent>();

		public static void AddListener( int type, EventHandler handler )
		{
			if ( !HANDLERS.TryGetValue( type, out List<EventHandler> list ) )
			{
				list = new List<EventHandler>();
				HANDLERS.Add( type, list );
			}
			list.Add( handler );
		}

		public static void RemoveListener( int type, EventHandler handler )
		{
			if ( !HANDLERS.TryGetValue( type, out List<EventHandler> list ) )
				return;
			bool result = list.Remove( handler );
			if ( !result )
				return;
			if ( list.Count == 0 )
				HANDLERS.Remove( type );
		}

		public static void BeginInvoke( BaseEvent e )
		{
			PENDING_LIST.Push( e );
		}

		public static void Invoke( BaseEvent e )
		{
			if ( HANDLERS.TryGetValue( e.type, out List<EventHandler> notifyHandlers ) )
			{
				int count = notifyHandlers.Count;
				for ( int i = 0; i < count; i++ )
					notifyHandlers[i].Invoke( e );
			}
			e.Release();
		}

		public static void Sync()
		{
			PENDING_LIST.Switch();
			while ( !PENDING_LIST.isEmpty )
			{
				BaseEvent e = PENDING_LIST.Pop();
				Invoke( e );
			}
		}
	}
}