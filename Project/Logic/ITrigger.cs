using Core.FMath;
using Logic.Map;

namespace Logic
{
	public interface ITrigger : ITileObject
	{
		Fix64 triggerRadius { get; }

		void OnTrigger( Entity entity );
	}
}