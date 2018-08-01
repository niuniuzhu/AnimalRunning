using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Core.Net;

namespace SocketTest
{
	class Program
	{
		public static ConsoleKey currentKey;

		static int Main( string[] args )
		{
			Task.Run( () =>
			{
				while ( true )
				{
					while ( !Console.KeyAvailable )
					{
						Thread.Sleep( 100 );
					}
					currentKey = Console.ReadKey( true ).Key;
				}
			} );

			TCPTest tcpTest = new TCPTest();
			Stopwatch sw = new Stopwatch();
			sw.Start();
			while ( true )
			{
				if ( sw.ElapsedMilliseconds >= 10 )
				{
					tcpTest.Update( 10 );
					sw.Restart();
					currentKey = ConsoleKey.NoName;
				}
				if ( currentKey == ConsoleKey.Enter )
					break;
				Thread.Sleep( 1 );
			}
			tcpTest.Dispose();
			return 0;
		}
	}
}
