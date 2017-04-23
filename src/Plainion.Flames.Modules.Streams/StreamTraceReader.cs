using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Plainion.Flames.Infrastructure;
using Plainion.Progress;

namespace Plainion.Flames.Modules.Streams
{
    [Export( typeof( ITraceReader ) )]
    class StreamTraceReader : ITraceReader
    {
        public StreamTraceReader()
        {
            FileFilters = new[] { new FileFilter( ".log", "String trace file (*.log)" ) };
        }

        public IEnumerable<FileFilter> FileFilters { get; private set; }

        public Task ReadAsync( string filename, TraceModelBuilder builder, IProgress<IProgressInfo> progress )
        {
            return Task.Run( ()=>
                {
                    var moduleLocation = Path.GetDirectoryName( GetType().Assembly.Location );
                    var catalog = new DirectoryCatalog( moduleLocation, "Plainion.Flames.Modules.Streams.*.dll" );
                    var container = new CompositionContainer( catalog );

                    container.Compose( new CompositionBatch() );

                    var parsers = container.GetExportedValues<IStreamTraceParser>();

                    Contract.Requires( parsers != null && parsers.Any(), "No parser implementation found for " + typeof( IStreamTraceParser ).Name );
                    Contract.Requires( parsers.Count() == 1, "Multiple parser implementations found for" + typeof( IStreamTraceParser ).Name );

                    var fileInfo = new FileInfo( filename );
                    if( fileInfo.Length == 0 )
                    {
                        return;
                    }

                    Build( builder, fileInfo, parsers.Single() );
                });
        }

        private void Build( TraceModelBuilder builder, FileInfo file, IStreamTraceParser parser )
        {
            var lines = new List<TraceLineBase>( ( int )( file.Length / 120 ) );

            var factory = new TraceLineFactory( builder );

            using( var stream = new FileStream( file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            {
                parser.TraceLine = line => lines.Add( line );
                parser.TraceInfo = info =>
                    {
                        builder.SetCreationTime( info.CreationTimestamp );
                        builder.SetTraceDuration( info.TraceDuration );
                    };

                parser.Process( stream, factory );

                parser.TraceLine = null;
                parser.TraceInfo = null;
            }

            foreach( var group in lines.GroupBy( e => e.ProcessId ) )
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
