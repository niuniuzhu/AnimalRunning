using System.Collections;
using Core.Misc;

namespace Logic
{
	public class EffectData
	{
		public enum TrackMode
		{
			DockedAtCaster,
			DockedAtTarget,
			FollowCaster,
			FollowTarget
		}

		public enum DissipatingMode
		{
			Auto,
			Timed,
			Programatic
		}

		public string id { get; }
		public string model { get; }
		public TrackMode trackMode { get; }
		public DissipatingMode dissipatingMode { get; }
		public float duration { get; }

		public EffectData( string id )
		{
			this.id = id;
			Hashtable def = Defs.GetEffect( this.id );
			this.model = def.GetString( "model" );
			this.trackMode = ( TrackMode ) def.GetInt( "track_mode" );
			this.dissipatingMode = ( DissipatingMode ) def.GetInt( "dissipating_mode" );
			this.duration = def.GetFloat( "duration" );
		}
	}
}