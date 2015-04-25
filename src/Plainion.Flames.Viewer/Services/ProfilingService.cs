using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace Plainion.Flames.Viewer.Services
{
    [Export]
    class ProfilingService
    {
        private TraceEventSession mySession;
        private Dictionary<int, double> myStartTimes;

        // TODO: go for observables/RX?
        public void Start()
        {
            Task.Factory.StartNew( () =>
            {
                myStartTimes = new Dictionary<int, double>();

                using( mySession = new TraceEventSession( "Plainion.Flames.Viewer.Monitor" ) )
                {
                    mySession.Source.Dynamic.All += delegate( TraceEvent e )
                    {
                        if( myStartTimes == null ) return;

                        if( e.Opcode == TraceEventOpcode.Start )
                        {
                            myStartTimes[ (int)e.PayloadByName("ObjectId") ] = e.TimeStampRelativeMSec;
                        }
                        else if( e.Opcode == TraceEventOpcode.Stop )
                        {
                            var objectId = ( int )e.PayloadByName( "ObjectId" );
                            Debug.WriteLine( string.Format( "{0}|{1}|{2:0.00} ms",
                                objectId,
                                e.TaskName,
                                ( e.TimeStampRelativeMSec - myStartTimes[ objectId ] ) ) );
                        }
                    };

                    mySession.EnableProvider( "Plainion-Flames" );

                    mySession.Source.Process();
                }
            }, TaskCreationOptions.LongRunning );
        }

        public void Stop()
        {
            mySession.Stop();
            myStartTimes = null;
        }
    }
}
