using Core.FMath;
using Core.Math;
using Logic.Event;
using Logic.Map;

namespace Logic
{
	public class Collider : Entity, ITileObject
	{
		public int tileIndex { get; set; } = -1;
		public FBounds bounds => this._data.bounds;

		[Sync( Attr.Bounds )]
		public FBounds worldBounds { get; private set; }

		public bool enableCollision { get; set; } = true;

		protected override void OnPositionChanged()
		{
			base.OnPositionChanged();
			this.worldBounds = new FBounds( this.position + this.bounds.center, this.bounds.size );
			this.battle.maze.UpdateObjectPosition( this, false );
		}

		public override void OnGenericUpdate( UpdateContext context )
		{
			base.OnGenericUpdate( context );
			//debug draw
			SyncEvent.DebugDraw( SyncEvent.DebugDrawType.WireCube, this.worldBounds.center, this.worldBounds.size, Fix64.Zero, Color4.blue );
		}

		protected override void InternalOnRemoveFromBattle()
		{
			this.battle.maze.UpdateObjectPosition( this, true );
			base.InternalOnRemoveFromBattle();
		}
	}
}