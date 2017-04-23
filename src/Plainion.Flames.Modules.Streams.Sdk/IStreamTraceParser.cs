using System;
using System.ComponentModel.Composition;
using System.IO;

namespace Plainion.Flames.Modules.Streams
{
    [InheritedExport( typeof( IStreamTraceParser ) )]
    public interface IStreamTraceParser
    {
        void Process( Stream stream, IParserContext context );
    }
}
