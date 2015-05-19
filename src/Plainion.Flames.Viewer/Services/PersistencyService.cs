using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Model;
using Plainion.Flames.Viewer.Model;
using Plainion.Prism.Events;
using Plainion.Progress;

namespace Plainion.Flames.Viewer.Services
{
    [Export]
    class PersistencyService
    {
        private Solution mySolution;
        private string myOpenTraceFilter;
        private string mySaveTraceFilter;

        [ImportingConstructor]
        public PersistencyService( IEventAggregator eventAggregator, Solution solution )
        {
            mySolution = solution;

            eventAggregator.GetEvent<ApplicationShutdownEvent>().Subscribe( x => UnloadSolution() );
        }

        [ImportMany]
        public IEnumerable<ITraceReader> TraceReaders { get; private set; }

        [ImportMany]
        public IEnumerable<ITraceWriter> TraceWriters { get; private set; }

        [ImportMany]
        public IEnumerable<IProjectItemProvider> ProjectItemProviders { get; private set; }

        public string OpenTraceFilter
        {
            get
            {
                if( myOpenTraceFilter == null )
                {
                    myOpenTraceFilter = "All files (*.*)|*.*|" + string.Join( "|", TraceReaders
                        .SelectMany( r => r.FileFilters )
                        .Select( f => string.Format( "{0}|*{1}", f.Description, f.Extension ) ) );
                }

                return myOpenTraceFilter;
            }
        }

        public string SaveTraceFilter
        {
            get
            {
                if( mySaveTraceFilter == null )
                {
                    mySaveTraceFilter = string.Join( "|", TraceWriters
                        .SelectMany( r => r.FileFilters )
                        .Select( f => string.Format( "{0}|*{1}", f.Description, f.Extension ) ) );
                }

                return mySaveTraceFilter;
            }
        }

        // TODO: what would be the contract if nothing could be loaded?
        public async Task LoadAsync( Project project, IProgress<IProgressInfo> progress )
        {
            var builder = new TraceModelBuilder();

            foreach( var traceFile in project.TraceFiles )
            {
                var ext = Path.GetExtension( traceFile );
                var reader = TryGetTraceReaderByExtension( ext );
                Contract.Requires( reader != null, "No Reader found for file extension: " + ext );

                await reader.ReadAsync( traceFile, builder, progress );
            }

            project.TraceLog = builder.Complete();

            OnTracesLoadCompleted( project.TraceFiles );

            foreach( var provider in ProjectItemProviders )
            {
                provider.OnTraceLogLoaded( project );
            }
        }

        private ITraceReader TryGetTraceReaderByExtension( string ext )
        {
            return TraceReaders
                .SingleOrDefault( r => r.FileFilters
                    .Any( f => f.Extension.Equals( ext, StringComparison.OrdinalIgnoreCase ) ) );
        }

        public bool CanLoad( string f )
        {
            return TryGetTraceReaderByExtension( Path.GetExtension( f ) ) != null;
        }

        /// <summary>
        /// Saves certain aspects (provided by ITraceLog) of the project.
        /// </summary>
        public Task ExportAsync( ITraceLog traceLog, string filename, IProgress<IProgressInfo> progress )
        {
            Contract.RequiresNotNull( traceLog, "traceLog" );

            var writer = TraceWriters
                .Single( r => r.FileFilters
                    .Any( f => f.Extension.Equals( Path.GetExtension( filename ), StringComparison.OrdinalIgnoreCase ) ) );

            return Task.Run( () =>
                {
                    var task = writer.WriteAsync( traceLog, filename, progress );

                    task.Wait();
                } );
        }

        private void UnloadSolution()
        {
            foreach( var project in mySolution.Projects )
            {
                Unload( project );
            }
        }

        public void Unload( Project project )
        {
            foreach( var provider in ProjectItemProviders )
            {
                provider.OnProjectUnloading( project );
            }

            project.TraceLog.Dispose();
            project.TraceLog = null;
        }

        [Import]
        private TraceLoaderService TraceLoader { get; set; }

        private void OnTracesLoadCompleted( IEnumerable<string> traceFiles )
        {
            TraceLoader.LoadCompleted( traceFiles );
        }
    }
}
