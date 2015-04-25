using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Plainion.Flames.Infrastructure;
using Plainion.Progress;

namespace Plainion.Flames.Modules.BFlames
{
    [Export( typeof( ITraceReader ) )]
    [Export( typeof( BFlamesReader ) )]
    class BFlamesReader : ITraceReader
    {
        public BFlamesReader()
        {
            FileFilters = new[] { new FileFilter( ".bflames", "Binary flames (*.bflames)" ) };
        }

        public IEnumerable<FileFilter> FileFilters { get; private set; }

        [ImportMany]
        public IEnumerable<Lazy<IAssociatedEventsSerializer>> AssociatedEventsSerializers { get; set; }
        
        public Task ReadAsync( string filename, TraceModelBuilder builder, IProgress<IProgressInfo> progress )
        {
            return Task.Run( () =>
                {
                    using( var reader = new BinaryReader( new FileStream( filename, FileMode.Open, FileAccess.Read ) ) )
                    {
                        var parser = new Parser( reader, builder );
                        parser.AssociatedEventsSerializers = AssociatedEventsSerializers;

                        parser.Read();
                    }
                } );
        }
    }
}
