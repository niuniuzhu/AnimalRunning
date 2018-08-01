using Core.FMath;

namespace Logic
{
	public class Terminus : Collider, ITrigger
	{
		public Fix64 triggerRadius => this._data.triggerRadius;

		public void OnTrigger( Entity entity )
		{
			Champion champion = ( Champion ) entity;
			this.battle.ReachTerminus( champion );
		}
	}
}