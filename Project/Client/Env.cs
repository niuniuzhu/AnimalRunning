using System.Diagnostics;

namespace Client
{
	public static class Env
	{
		public static readonly string VERSION = "1.0";

		public static bool isEditor;
		public static int platform;
		public static bool useNetwork;
		public static bool isRunning;

		public static long lag;
		public static long serverTime;
		public static long timeDiff;

		private static readonly Stopwatch STOPWATCH = new Stopwatch();

		public static long elapsed => STOPWATCH.ElapsedMilliseconds;

		internal static void StartTime()
		{
			STOPWATCH.Start();
		}
	}
}