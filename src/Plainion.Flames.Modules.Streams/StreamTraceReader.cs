using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Plainion.Flames.Infrastructure;
using Plainion.Logging;
using Plainion.Progress;

namespace Plainion.Flames.Modules.Streams
{
    [Export( typeof( ITraceReader ) )]
    class StreamTraceReader : ITraceReader
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger( typeof( StreamTraceReader ) );

        public StreamTraceReader()
        {
            FileFilters = new[] { 
                new FileFilter( ".log", "Stream trace file (*.log)" ),
                new FileFilter( ".txt", "Stream trace file (*.txt)" ),
                new FileFilter( ".trace", "Stream trace file (*.trace)" ) };
        }

        public IEnumerable<FileFilter> FileFilters { get; private set; }

        public Task ReadAsync( string filename, TraceModelBuilder builder, IProgress<IProgressInfo> progress )
        {
            return Task.Run( () =>
                {
                    var moduleLocation = Path.GetDirectoryName( GetType().Assembly.Location );
                    var catalog = new DirectoryCatalog( moduleLocation, "Plainion.Flames.Modules.Streams.*.dll" );
                    var container = new CompositionContainer( catalog );

                    container.Compose( new CompositionBatch() );

                    var parsers = container.GetExportedValues<IStreamTraceParser>();

                    if( parsers != null && parsers.Any() )
                    {
                        myLogger.Warning( "No parser implementation found for {0}. Using default implementation: {1}", typeof( IStreamTraceParser ).Name, typeof( SampleTraceParser ).FullName );
                        parsers = new[] { new SampleTraceParser() };
                    }
                    else
                    {
                        Contract.Requires( parsers.Count() == 1, "Multiple parser implementations found for" + typeof( IStreamTraceParser ).Name );
                    }

                    var fileInfo = new FileInfo( filename );
                    if( fileInfo.Length == 0 )
                    {
                        return;
                    }

                    Build( builder, fileInfo, parsers.Single() );
                } );
        }

        private void Build( TraceModelBuilder builder, FileInfo file, IStreamTraceParser parser )
        {
            var context = new ParserContext( builder, ( int )( file.Length / 120 ) );

            using( var stream = new FileStream( file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            {
                parser.Process( stream, context );
            }

            foreach( var group in context.Lines.GroupBy( e => e.ProcessId ) )
            {
                BuildProcess( builder, group.Key, group.ToList() );
            }

            GC.Collect();
        }

        private void BuildProcess( TraceModelBuilder builder, int processId, List<TraceLineBase> lines )
        {
            var process = builder.CreateProcess( processId );

            var builders = lines
                .GroupBy( e => e.ThreadId )
                .Select( x => new TraceThreadBuilder( builder, process, x.Key, x.AsEnumerable() ) );

            foreach( var b in builders )
            {
                b.Build();
            }
        }
    }
}
