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
        private string myOpenTraceFilter;
        private string mySaveTraceFilter;

        [ImportingConstructor]
        public PersistencyService( IEventAggregator eventAggregator, Solution solution )
        {
            eventAggregator.GetEvent<ApplicationShutdownEvent>().Subscribe( x => SaveFriendlyNames( solution ) );
        }

        [ImportMany]
        public IEnumerable<ITraceReader> TraceReaders { get; private set; }

        [ImportMany]
        public IEnumerable<ITraceWriter> TraceWriters { get; private set; }

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

            await Load( builder, project.TraceFiles, progress );

            project.TraceLog = builder.Complete();

            OnTracesLoadCompleted( project.TraceFiles );

            var repository = new FriendlyNamesRepository( project.TraceLog,
                Path.GetFileNameWithoutExtension( project.TraceFiles.First() ), Path.GetDirectoryName( project.TraceFiles.First() ) );
            repository.Load();

            project.Items.Add( repository );
        }

        private async Task Load( TraceModelBuilder builder, IEnumerable<string> traceFiles, IProgress<IProgressInfo> progress )
        {
            foreach( var traceFile in traceFiles )
            {
                var ext = Path.GetExtension( traceFile );
                var reader = TryGetTraceReaderByExtension( ext );
                Contract.Requires( reader != null, "No Reader found for file extension: " + ext );

                await reader.ReadAsync( traceFile, builder, progress );
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

        internal Task SaveAsync( ITraceLog traceLog, string filename, IProgress<IProgressInfo> progress )
        {
            var writer = TraceWriters
                .Single( r => r.FileFilters
                    .Any( f => f.Extension.Equals( Path.GetExtension( filename ), StringComparison.OrdinalIgnoreCase ) ) );

            return writer.WriteAsync( traceLog, filename, progress );
        }

        private void SaveFriendlyNames( Solution solution )
        {
            var repositories = solution.Projects
                .SelectMany( p => p.Items )
                .OfType<FriendlyNamesRepository>();

            foreach( var repository in repositories )
            {
                repository.Save();
            }
        }

        internal void Unload( Project project )
        {
            if( project.TraceLog == null )
            {
                return;
            }

            var repository = project.Items.OfType<FriendlyNamesRepository>().Single();
            repository.Save();

            project.TraceLog.Dispose();
        }

        [Import]
        private TraceLoaderService TraceLoader { get; set; }

        private void OnTracesLoadCompleted( IEnumerable<string> traceFiles )
        {
            TraceLoader.LoadCompleted( traceFiles );
        }
    }
}
