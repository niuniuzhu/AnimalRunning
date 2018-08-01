using Core.FMath;
using Core.Misc;
using System.Collections;

namespace Logic
{
	public class EntityData
	{
		//entity
		public string id;
		public string name;
		public string model;

		//collider
		public FBounds bounds;

		//champion
		public Fix64 fov;
		public Fix64 naturalSpeed;

		//item
		public Fix64 triggerRadius;
		public TriggerData trigger;

		public EntityData( string id )
		{
			this.id = id;
			Hashtable def = Defs.GetEntity( this.id );
			this.name = def.GetString( "name" );
			this.model = def.GetString( "model" );
			this.naturalSpeed = def.GetFix64( "natural_speed" );
			FVec3 size = def.GetFVec3( "size" );
			this.bounds = new FBounds( new FVec3( Fix64.Zero, size.y * Fix64.Half, Fix64.Zero ), size );
			this.fov = def.GetFix64( "fov" );
			this.triggerRadius = def.GetFix64( "trigger_raduis" );
			Hashtable triggerDef = def.GetMap( "trigger" );
			if ( triggerDef != null )
				this.trigger = new TriggerData( triggerDef );
		}
	}

	public class TriggerData
	{
		public string buff;
		public Fix64 radius;
		public TargetType targetType;
		public int triggerCount;

		public TriggerData( Hashtable data )
		{
			this.buff = data.GetString( "buff" );
			this.radius = data.GetFix64( "radius" );
			this.targetType = ( TargetType )data.GetInt( "target_type" );
			this.triggerCount = data.GetInt( "trigger_count" );
		}
	}
}