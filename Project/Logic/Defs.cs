using System.Collections;
using Core.Misc;

namespace Logic
{
	public static class Defs
	{
		private static Hashtable _defs;

		public static void Init( string defs )
		{
			_defs = ( Hashtable )MiniJSON.JsonDecode( defs );
		}

		public static Hashtable Get( string key )
		{
			return ( Hashtable )_defs[key];
		}

		public static Hashtable GetMap( string id )
		{
			Hashtable ht = _defs.GetMap( "maps" );
			Hashtable defaultHt = ht.GetMap( "default" );
			Hashtable result = ht.GetMap( id ) ?? new Hashtable();
			result.Concat( defaultHt );
			return result;
		}

		public static Hashtable GetEntity( string id )
		{
			Hashtable ht = _defs.GetMap( "entities" );
			Hashtable defaultHt = ht.GetMap( "default" );
			Hashtable result = ht.GetMap( id ) ?? new Hashtable();
			result.Concat( defaultHt );
			return result;
		}

		public static Hashtable GetBuff( string id )
		{
			Hashtable ht = _defs.GetMap( "buffs" );
			Hashtable defaultHt = ht.GetMap( "default" );
			Hashtable result = ht.GetMap( id ) ?? new Hashtable();
			result.Concat( defaultHt );
			return result;
		}

		public static Hashtable GetEffect( string id )
		{
			Hashtable ht = _defs.GetMap( "effects" );
			Hashtable defaultHt = ht.GetMap( "default" );
			Hashtable result = ht.GetMap( id ) ?? new Hashtable();
			result.Concat( defaultHt );
			return result;
		}
	}
}