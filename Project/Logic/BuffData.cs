using System.Collections;
using System.Linq;
using Core.FMath;
using Core.Misc;

namespace Logic
{
	public sealed class BuffData
	{
		public string id { get; }
		public string[] conflicts { get; }
		public Attr[] attrs { get; }
		public Fix64[] values { get; }
		public Fix64 duration { get; }
		public string fx { get; }
		public string snd { get; }

		public BuffData( string id )
		{
			this.id = id;
			Hashtable def = Defs.GetBuff( this.id );
			this.conflicts = def.GetStringArray( "conflicts" );
			this.attrs = def.GetIntArray( "attrs" ).Cast<Attr>().ToArray();
			this.values = def.GetFix64Array( "values" );
			this.duration = def.GetFix64( "duration" );
			this.fx = def.GetString( "fx" );
			this.snd = def.GetString( "snd" );
		}
	}
}