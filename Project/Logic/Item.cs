using Core.FMath;
using Logic.Event;

namespace Logic
{
	public class Item : Collider, ITrigger
	{
		public Fix64 triggerRadius => this._data.triggerRadius;

		public void OnTrigger( Entity entity )
		{
			if ( ( ( Champion )entity ).PickItem( this.rid ) )
			{
				SyncEvent.PickItem( entity.rid, this.rid );
				this.MarkToDestroy();
			}
		}
	}
}