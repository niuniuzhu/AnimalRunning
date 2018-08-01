using Core.FMath;
using Core.Math;
using Core.Misc;
using System.Collections;

namespace Logic
{
	public sealed class MapData
	{
		public string id { get; }
		public int startCountDown { get; }
		public FVec3 scale { get; }
		public FVec3 offset { get; }
		public int row { get; }
		public int col { get; }
		public int startIndex { get; }
		public int endIndex { get; }
		public FVec2 startPointPlace { get; }
		public FVec3 camOffset { get; }
		public FVec3 camLookAtOffset { get; }
		public Fix64 camSmooth { get; }
		public Fix64 FOWFogFrequency { get; }
		public Fix64 FOWFogAmplitude { get; }
		public Fix64 FOWDistanceToPlayer { get; }
		public string[] items { get; }
		public int maxItemCount { get; }
		public Fix64[] itemUpdateInterval { get; }
		public string bgSnd { get; }
		public string countDownSnd { get; }
		public string winSnd { get; }
		public string loseSnd { get; }
		public string surfaceMat { get; }

		public MapData( string id )
		{
			this.id = id;
			Hashtable def = Defs.GetMap( this.id );
			this.startCountDown = def.GetInt( "start_count_down" );
			this.scale = def.GetFVec3( "scale" );
			this.offset = def.GetFVec3( "offset" );
			this.row = MathUtils.Max( 3, def.GetInt( "row" ) );
			this.col = MathUtils.Max( 3, def.GetInt( "col" ) );
			this.startPointPlace = def.GetFVec2( "start_point_place" );
			this.startIndex = def.GetInt( "start_index" );
			this.endIndex = def.GetInt( "end_index" );
			this.camOffset = def.GetFVec3( "cam_offset" );
			this.camLookAtOffset = def.GetFVec3( "cam_lookat_offset" );
			this.camSmooth = def.GetFix64( "cam_smooth" );
			this.FOWFogFrequency = def.GetFix64( "FOW_fog_frequency" );
			this.FOWFogAmplitude = def.GetFix64( "FOW_fog_amplitude" );
			this.FOWDistanceToPlayer = def.GetFix64( "FOW_distance_to_player" );
			this.items = def.GetStringArray( "items" );
			this.maxItemCount = def.GetInt( "max_item_count" );
			this.itemUpdateInterval = def.GetFix64Array( "item_update_interval" );
			this.bgSnd = def.GetString( "bg_snd" );
			this.countDownSnd = def.GetString( "count_down_snd" );
			this.winSnd = def.GetString( "win_snd" );
			this.loseSnd = def.GetString( "lose_snd" );
			this.surfaceMat = def.GetString( "surface_mat" );
		}
	}
}