using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Plainion.Flames.Infrastructure;
using Microsoft.Diagnostics.Tracing.Etlx;
using Plainion.Progress;
using EtwTraceLog = Microsoft.Diagnostics.Tracing.Etlx.TraceLog;

namespace Plainion.Flames.Modules.ETW
{
    class TraceFileHandle : IDisposable
    {
        [DllImport( "kernel32.dll" )]
        static extern bool CreateSymbolicLink( string lpSymlinkFileName, string lpTargetFileName, int dwFlags );

        public TraceFileHandle( TraceFile traceFile )
        {
            TraceFile = traceFile;
            WorkspaceRoot = @"c:\flames.db";
            AlwaysGenerateNewEtlx = false;
        }

        public TraceFile TraceFile { get; private set; }

        public TraceLog Trace { get; private set; }

        /// <summary>
        /// ETL is opened and converted to ETLx through a symbolic link to the actual directory of the ETL file because 
        /// of MAX_PATH issues when loading symbols from .NGENPDB folder
        /// </summary>
        public string WorkspaceRoot { get; private set; }

        public bool AlwaysGenerateNewEtlx { get; set; }

        internal void Open( IProgress<IProgressInfo> progress )
        {
            if( Trace != null )
            {
                // already open
                return;
            }

            SetupWorkspace();

            var options = new TraceLogOptions();
            options.AlwaysResolveSymbols = false;

            //myLog = new StreamWriter( Path.Combine( WorkspaceRoot, "flames.log" ) );
            using( var log = new QueueTextWriter() )
            {
                options.ConversionLog = log;

                options.ShouldResolveSymbols = ( s ) => true;

                var task = StartProgressObserverAsync( log, progress );

                Trace = EtwTraceLog.OpenOrConvert( Path.Combine( WorkspaceRoot, Path.GetFileName( TraceFile.Etl ) ), options );

                log.Close();

                task.Wait();
            }
        }

        private static Task StartProgressObserverAsync( QueueTextWriter log, IProgress<IProgressInfo> progress )
        {
            var progressInfo = new CountingProgress( "Step 2: Preparing ETLx" );
            progress.Report( progressInfo );

            var task = Task.Run( () =>
            {
                var query = log.Queue.GetConsumingEnumerable()
                    .Where( l => l.StartsWith( "[", StringComparison.OrdinalIgnoreCase ) )
                    .Select( l => l.TrimStart( '[' ).TrimEnd( ']', '\r', '\n' ) );

                foreach( var line in query )
                {
                    progressInfo.IncrementBy( 1 );
                    progressInfo.Details = line;

                    progress.Report( progressInfo );
                }
            } );
            return task;
        }

        private void SetupWorkspace()
        {
            if( Directory.Exists( WorkspaceRoot ) )
            {
                System.Console.WriteLine( "Removing analysis workspace" );
                Directory.Delete( WorkspaceRoot );
            }

            if( !CreateSymbolicLink( WorkspaceRoot, Path.GetDirectoryName( TraceFile.Etl ), 0x1 ) )
            {
                throw new Exception( "Failed to setup workspace" );
            }

            if( AlwaysGenerateNewEtlx && TraceFile.EtlxExists )
            {
                File.Delete( TraceFile.Etlx );
            }
        }

        public void Close()
        {
            if( Trace != null )
            {
                Trace.Dispose();
                Trace = null;
            }

            if( Directory.Exists( WorkspaceRoot ) )
            {
                System.Console.WriteLine( "Removing analysis workspace" );
                Directory.Delete( WorkspaceRoot );
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
