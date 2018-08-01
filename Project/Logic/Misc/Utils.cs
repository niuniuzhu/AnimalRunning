using Core.Misc;
using System;

namespace Logic.Misc
{
	public static class Utils
	{
		public static string MakeRidFromID( string id )
		{
			return id + "@" + GuidHash.GetString();
		}

		public static string GetIDFromRID( string rid )
		{
			int pos = rid.IndexOf( "@", StringComparison.Ordinal );
			string id = pos != -1 ? rid.Substring( 0, pos ) : rid;
			return id;
		}
	}
}