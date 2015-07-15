using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Infrastructure.Model;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;
using Plainion.Flames.Viewer.Model;
using Plainion.Prism.Events;
using Plainion.Progress;
using Plainion.Windows;
using Plainion.Windows.Diagnostics;

namespace Plainion.Flames.Viewer.Services
{
    [Export]
    class LoaderSerivce : IProjectService
    {
        private Project myProject;
        private string myOpenTraceFilter;
        private string mySaveTraceFilter;

        [ImportingConstructor]
        public LoaderSerivce( IEventAggregator eventAggregator )
        {
            eventAggregator.GetEvent<ApplicationShutdownEvent>().Subscribe( x =>
            {
                // enforce unload of project incl. ProjectChanging event
                Project = null;
            } );
        }

        IProject IProjectService.Project { get { return myProject; } }

        public Project Project
        {
            get { return myProject; }
            private set
            {
                if( myProject == value )
                {
                    return;
                }

                if( myProject != null )
                {
                    if( ProjectChanging != null )
                    {
                        ProjectChanging( this, EventArgs.Empty );
                    }

                    Unload( myProject );
                }

                myProject = value;

                if( ProjectChanged != null )
                {
                    ProjectChanged( this, EventArgs.Empty );
                }
            }
        }

        public event EventHandler ProjectChanging;

        public event EventHandler ProjectChanged;

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

        [ImportMany( typeof( IDocument ) )]
        public IEnumerable<ExportFactory<IDocument, IDocumentMetadata>> Documents { get; private set; }

        // TODO: what would be the contract if nothing could be loaded?
        public async Task LoadAsync( IEnumerable<string> traceFiles, IProgress<IProgressInfo> progress )
        {
            var project = new Project( traceFiles );

            var file = GetProjectFile( project );

            project.WasDeserialized = File.Exists( file );

            // unload old project
            Project = null;

            if( File.Exists( file ) )
            {
                using( var stream = new FileStream( file, FileMode.Open ) )
                {
                    using( var archive = new ZipArchive( stream, ZipArchiveMode.Read ) )
                    {
                        foreach( var entry in archive.Entries )
                        {
                            var factory = Documents.SingleOrDefault( doc => doc.Metadata.Name == entry.Name );
                            var document = factory.CreateExport();
                            document.Value.Deserialize( entry.Open() );
                            project.Items.Add( document.Value );
                        }
                    }
                }
            }

            await LoadTraceLogAsync( project, progress );
        }

        private async Task LoadTraceLogAsync( Project project, IProgress<IProgressInfo> progress )
        {
            // now all project items are loaded - publish the new project
            Project = project;

            var builder = new TraceModelBuilder();

            foreach( var item in project.Items )
            {
                builder.ReaderContextHints.Add( item );
            }

            foreach( var traceFile in project.TraceFiles )
            {
                var ext = Path.GetExtension( traceFile );
                var reader = TryGetTraceReaderByExtension( ext );
                Contract.Requires( reader != null, "No Reader found for file extension: " + ext );

                await reader.ReadAsync( traceFile, builder, progress );
            }

            // first copy back the project items
            project.Items.Clear();
            foreach( var hint in builder.ReaderContextHints )
            {
                project.Items.Add( hint );
            }

            // ... then set the tracelog - some view models react on that and expect the project items to be there again
            project.TraceLog = builder.Complete();
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

        private void Unload( Project project )
        {
            if( project == null )
            {
                return;
            }

            var file = GetProjectFile( project );

            if( File.Exists( file ) )
            {
                File.Delete( file );
            }

            using( var stream = new FileStream( file, FileMode.OpenOrCreate ) )
            {
                using( var archive = new ZipArchive( stream, ZipArchiveMode.Create ) )
                {
                    foreach( var doc in Project.Items.OfType<IDocument>() )
                    {
                        var attr = ( DocumentAttribute )doc.GetType().GetCustomAttributes( typeof( DocumentAttribute ), false ).Single();
                        using( var entryStream = archive.CreateEntry( attr.Name, CompressionLevel.Fastest ).Open() )
                        {
                            doc.Serialize( entryStream );
                        }
                    }
                }
            }

            project.TraceLog.Dispose();
            project.TraceLog = null;

            // http://stackoverflow.com/questions/13026826/execute-command-after-view-is-loaded-wpf-mvvm
            // trigger GC after application does idle so that all Unload handlers could cleanup so that
            // we can free all memory no longer needed
            Application.Current.Dispatcher.BeginInvoke( DispatcherPriority.ApplicationIdle, new Action( () =>
            {
                GC.Collect();
            } ) );

        }

        private static string GetProjectFile( Project project )
        {
            var mainTraceFile = project.TraceFiles.First();
            return Path.Combine( Path.GetDirectoryName( mainTraceFile ), Path.GetFileNameWithoutExtension( mainTraceFile ) + ".pfp" );
        }

        public Task CreatePresentationAsync( IProgress<IProgressInfo> progress )
        {
            Contract.Invariant( Project != null, "No project exists" );

            return Task.Run<FlameSetPresentation>( () =>
                {
                    progress.Report( new UndefinedProgress( "Creating presentation" ) );

                    var factory = new PresentationFactory();
                    return factory.CreateFlameSetPresentation( Project.TraceLog );
                } )
                .RethrowExceptionsInUIThread()
                .ContinueWith( t =>
                {
                    Project.Presentation = t.Result;
                }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext() )
                .RethrowExceptionsInUIThread();
        }

        public void UnloadAll()
        {
            // trigger unload of the current projec
            Project = null;

            WpfStatics.CollectStatisticsOnIdle();
        }
    }
}
