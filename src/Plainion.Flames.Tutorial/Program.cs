using System;
using System.Threading;

namespace Plainion.Flames.Tutorial
{
    class Program
    {
        public static int myDummyGlobalCounter = 0;

        static void Main( string[] args )
        {
            var t1 = new Thread( obj => Thread.Sleep( 5000 ) );
            var t2 = new Thread( obj =>
            {
                var start = DateTime.Now;
                while( ( DateTime.Now - start ).TotalSeconds < 3 )
                {
                    // do some work
                    for( int i = 0; i < 10; i++ )
                    {
                        myDummyGlobalCounter += i;
                    }
                }
            } );
            var t3 = new Thread( obj =>
            {
                Thread.Sleep( 1000 );
                try
                {
                    throw new NullReferenceException();
                }
                catch { }
            } );

            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();
        }
    }
}
