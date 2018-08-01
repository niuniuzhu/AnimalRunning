using Core.FMath;

namespace Logic.Map
{
	public interface ITileObject
	{
		int tileIndex { get; set; }
		FVec3 position { get; }
		FBounds worldBounds { get; }
		bool enableCollision { get; set; }
	}
}