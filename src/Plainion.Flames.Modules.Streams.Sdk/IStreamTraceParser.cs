using System;
using System.ComponentModel.Composition;
using System.IO;

namespace Plainion.Flames.Modules.Streams
{
    [InheritedExport( typeof( IStreamTraceParser ) )]
    public interface IStreamTraceParser
    {
        Action<TraceLineBase> TraceLine { get; set; }
        Action<TraceInfo> TraceInfo{ get; set; }

        void Process( Stream stream, ITraceLineFactory factory );
    }
}
