using System;
using System.Collections.Generic;
using System.IO;

namespace Plainion.Flames.Modules.Streams
{
    class SampleTraceParser : IStreamTraceParser
    {
        public Action<TraceLineBase> TraceLine { get; set; }
        public Action<TraceInfo> TraceInfo { get; set; }

        public void Process( Stream stream, ITraceLineFactory factory )
        {
            using( var reader = new StreamReader( stream ) )
            {
                DateTime? creationTime = null;
                // relative time from trace start in micro seconds
                long time = 0;

                while( !reader.EndOfStream )
                {
                    var line = reader.ReadLine();

                    //time = 0;
                    int pid = 0;
                    int tid = 0;
                    string module = null;
                    string nameSpace = null;
                    string className = null;
                    string methodName = null;

                    if( creationTime == null )
                    {
                        creationTime = DateTime.Now;
                    }

                    //if( is entering trace )
                    {
                        if( TraceLine != null )
                        {
                            TraceLine( factory.CreateEnteringLine( time, pid, tid, module, nameSpace, className, methodName ) );
                        }
                    }
                    //else if( leaving trace )
                    {
                        if( TraceLine != null )
                        {
                            TraceLine( factory.CreateLeavingLine( time, pid, tid, module, nameSpace, className, methodName ));
                        }
                    }
                }

                if( TraceInfo != null )
                {
                    TraceInfo( new TraceInfo
                    {
                        CreationTimestamp = creationTime.Value,
                        TraceDuration = time
                    } );
                }
            }
        }
    }
}
