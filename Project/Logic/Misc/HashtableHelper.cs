using Core.Math;
using System;
using System.Collections;

namespace Logic.Misc
{
	public static class HashtableHelperEx
	{
		public static Vec2 GetVector2( this Hashtable ht, string key )
		{
			if ( !ht.ContainsKey( key ) )
				return Vec2.zero;
			ArrayList v = ( ArrayList )ht[key];
			return new Vec2( Convert.ToSingle( v[0] ), Convert.ToSingle( v[1] ) );
		}

		public static void SetVector2( this Hashtable ht, string key, Vec2 v )
		{
			float[] al = new float[2];
			al[0] = v.x;
			al[1] = v.y;
			ht[key] = al;
		}

		public static Vec3 GetVector3( this Hashtable ht, string key )
		{
			if ( !ht.ContainsKey( key ) )
				return Vec3.zero;
			ArrayList v = ( ArrayList )ht[key];
			return new Vec3( Convert.ToSingle( v[0] ), Convert.ToSingle( v[1] ), Convert.ToSingle( v[2] ) );
		}

		public static void SetVector3( this Hashtable ht, string key, Vec3 v )
		{
			float[] al = new float[3];
			al[0] = v.x;
			al[1] = v.y;
			al[2] = v.z;
			ht[key] = al;
		}

		public static Vec4 GetVector4( this Hashtable ht, string key )
		{
			if ( !ht.ContainsKey( key ) )
				return Vec4.zero;
			ArrayList v = ( ArrayList )ht[key];
			return new Vec4( Convert.ToSingle( v[0] ), Convert.ToSingle( v[1] ), Convert.ToSingle( v[2] ), Convert.ToSingle( v[3] ) );
		}

		public static void SetVector4( this Hashtable ht, string key, Vec4 v )
		{
			float[] al = new float[4];
			al[0] = v.x;
			al[1] = v.y;
			al[2] = v.z;
			al[3] = v.w;
			ht[key] = al;
		}

		public static Vec2 GetVector2FromString( this Hashtable ht, string key )
		{
			if ( !ht.ContainsKey( key ) )
				return Vec2.zero;
			string v = ( string )ht[key];
			string[] arr = v.Split( ',' );
			return new Vec2( Convert.ToSingle( arr[0] ), Convert.ToSingle( arr[1] ) );
		}

		public static Vec3 GetVector3FromString( this Hashtable ht, string key )
		{
			if ( !ht.ContainsKey( key ) )
				return Vec3.zero;
			string v = ( string )ht[key];
			string[] arr = v.Split( ',' );
			return new Vec3( Convert.ToSingle( arr[0] ), Convert.ToSingle( arr[1] ), Convert.ToSingle( arr[2] ) );
		}

		public static Vec4 GetVector4FromString( this Hashtable ht, string key )
		{
			if ( !ht.ContainsKey( key ) )
				return Vec4.zero;
			string v = ( string )ht[key];
			string[] arr = v.Split( ',' );
			return new Vec4( Convert.ToSingle( arr[0] ), Convert.ToSingle( arr[1] ), Convert.ToSingle( arr[2] ), Convert.ToSingle( arr[3] ) );
		}
	}
}