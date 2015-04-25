using System;
using System.ComponentModel.Composition;
using System.IO;

namespace Plainion.Flames.Modules.StringTracing
{
    [InheritedExport( typeof( IStringTraceParser ) )]
    public interface IStringTraceParser
    {
        Action<TraceLineBase> TraceLine { get; set; }
        Action<TraceInfo> TraceInfo{ get; set; }

        void Process( Stream stream, ITraceLineFactory factory );
    }
}
