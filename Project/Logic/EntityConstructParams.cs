using Core.FMath;

namespace Logic
{
	public sealed class EntityParam
	{
		public string rid;
		public byte skin = 0xff;
		public FVec3 position;
		public FVec3 direction;

		//champoin
		public string uid;
		public int team = -1;
	}
}