namespace View
{
	public sealed class CPlayer : CChampion
	{
		public static CPlayer instance { get; internal set; }

		public CPlayer()
		{
			instance = this;
		}
	}
}