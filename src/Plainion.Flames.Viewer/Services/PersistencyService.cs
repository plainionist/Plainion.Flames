using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Model;
using Microsoft.Practices.Prism.PubSubEvents;
using Plainion;
using Plainion.Prism.Events;
using Plainion.Progress;

namespace Plainion.Flames.Viewer.Services
{
    [Export]
    class PersistencyService
    {
        private string myOpenTraceFilter;
        private string mySaveTraceFilter;
        private IList<FriendlyNamesRepository> myFriendlyNames;

        [ImportingConstructor]
        public PersistencyService( IEventAggregator eventAggregator )
        {
            myFriendlyNames = new List<FriendlyNamesRepository>();

            eventAggregator.GetEvent<ApplicationShutdownEvent>().Subscribe( x => SaveFriendlyNames() );
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
        public async Task<TraceLog> LoadAsync( IEnumerable<string> traceFiles, IProgress<IProgressInfo> progress )
        {
            var builder = new TraceModelBuilder();

            await Load( builder, traceFiles, progress );

            var log = builder.Complete();

            OnTracesLoadCompleted( traceFiles );

            var repository = new FriendlyNamesRepository( log, Path.GetFileNameWithoutExtension( traceFiles.First() ), Path.GetDirectoryName( traceFiles.First() ) );
            repository.Load();

            myFriendlyNames.Add( repository );

            return log;
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

        private void SaveFriendlyNames()
        {
            foreach( var repository in myFriendlyNames )
            {
                repository.Save();
            }
        }

        internal void Unload( TraceLog log )
        {
            var repository = myFriendlyNames.Single( r => r.TraceLog == log );
            repository.Save();
            myFriendlyNames.Remove( repository );

            log.Dispose();
        }

        [Import]
        private TraceLoaderService TraceLoader { get; set; }

        private void OnTracesLoadCompleted( IEnumerable<string> traceFiles )
        {
            TraceLoader.LoadCompleted( traceFiles );
        }
    }
}
