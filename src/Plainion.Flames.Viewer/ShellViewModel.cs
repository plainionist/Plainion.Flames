using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;
using Plainion.Flames.Viewer.Services;
using Plainion.Flames.Viewer.ViewModels;
using Plainion.Logging;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Progress;
using Plainion.Windows.Interactivity.DragDrop;
using Plainion.Prism.Events;

namespace Plainion.Flames.Viewer
{
    [Export]
    class ShellViewModel : BindableBase, IDropable
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger( typeof( ShellViewModel ) );

        private PersistencyService myPersistencyService;
        private FlamesBrowserViewModel myFlamesBrowserViewModel;
        private FlameSetPresentation myPresentation;
        private bool myIsBusy;
        private IProgressInfo myCurrentProgress;

        [ImportingConstructor]
        internal ShellViewModel( IEventAggregator eventAggregator, PersistencyService persistencyService, TraceLoaderService traceLoader )
        {
            myPersistencyService = persistencyService;

            traceLoader.UILoadAction = LoadTraces;

            OpenCommand = new DelegateCommand( OnOpen );
            SaveAsCommand = new DelegateCommand( OnSaveAs, () => myPresentation != null );
            SaveSnapshotCommand = new DelegateCommand( OnSaveSnapshot, () => myPresentation != null );
            CloseCommand = new DelegateCommand( () => Application.Current.Shutdown() );

            OpenFileRequest = new InteractionRequest<OpenFileDialogNotification>();
            SaveFileRequest = new InteractionRequest<SaveFileDialogNotification>();

            ShowLogRequest = new InteractionRequest<INotification>();
            ShowLogCommand = new DelegateCommand( OnShowLog );

            eventAggregator.GetEvent<ApplicationReadyEvent>().Subscribe( x => LoadTraceFromCommandLine() );
        }

        public FlamesBrowserViewModel FlamesBrowserViewModel
        {
            get { return myFlamesBrowserViewModel; }
            set { SetProperty( ref myFlamesBrowserViewModel, value ); }
        }

        public bool IsBusy
        {
            get { return myIsBusy; }
            set { SetProperty( ref myIsBusy, value ); }
        }

        public IProgressInfo CurrentProgress
        {
            get { return myCurrentProgress; }
            set { SetProperty( ref myCurrentProgress, value ); }
        }

        public DelegateCommand OpenCommand { get; private set; }

        public InteractionRequest<OpenFileDialogNotification> OpenFileRequest { get; private set; }

        private void OnOpen()
        {
            var notification = new OpenFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = myPersistencyService.OpenTraceFilter;
            notification.FilterIndex = 0;
            notification.MultiSelect = true;

            OpenFileRequest.Raise( notification,
                n =>
                {
                    if( n.Confirmed )
                    {
                        LoadTraces( n.FileNames );
                    }
                } );
        }

        public DelegateCommand SaveAsCommand { get; private set; }

        public InteractionRequest<SaveFileDialogNotification> SaveFileRequest { get; private set; }

        private void OnSaveAs()
        {
            var notification = new SaveFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = myPersistencyService.SaveTraceFilter;
            notification.FilterIndex = 0;

            SaveFileRequest.Raise( notification, async n =>
            {
                if( n.Confirmed )
                {
                    IsBusy = true;
                    var progress = new Progress<IProgressInfo>( pi => CurrentProgress = pi );
                    await myPersistencyService.SaveAsync( myPresentation.Model, n.FileName, progress );
                    IsBusy = false;
                }
            } );
        }

        public DelegateCommand SaveSnapshotCommand { get; private set; }

        private void OnSaveSnapshot()
        {
            var notification = new SaveFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = myPersistencyService.SaveTraceFilter;
            notification.FilterIndex = 0;

            SaveFileRequest.Raise( notification, async n =>
            {
                if( n.Confirmed )
                {
                    IsBusy = true;
                    var progress = new Progress<IProgressInfo>( pi => CurrentProgress = pi );
                    await myPersistencyService.SaveAsync( CreateSnapshot( myPresentation ), n.FileName, progress );
                    IsBusy = false;
                }
            } );
        }

        private ITraceLog CreateSnapshot( FlameSetPresentation presentation )
        {
            var builder = new TraceModelViewBuilder( presentation.Model );

            builder.SetCreationTime( presentation.Model.CreationTime );
            builder.SetTraceDuration( presentation.Model.TraceDuration );

            var flames = presentation.Flames
                .Where( t => t.Visibility == ContentVisibility.Visible );

            foreach( var flame in flames )
            {
                builder.Add( flame.Model );

                foreach( var events in presentation.Model.AssociatedEvents.GetAllFor<IAssociatedEvents>( flame.Model ) )
                {
                    builder.Add( events );
                }
            }

            return builder.Complete();
        }

        public ICommand CloseCommand { get; private set; }

        private void LoadTraceFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            if( args.Length < 2 )
            {
                return;
            }

            string traceFile = args[ 1 ];

            if( File.Exists( traceFile ) )
            {
                Application.Current.Dispatcher.BeginInvoke( new Action<string>( f => LoadTraces( f ) ), traceFile );
            }
        }

        private async void LoadTraces( params string[] traceFiles )
        {
            if( myPresentation != null )
            {
                myPersistencyService.Unload( myPresentation.Model );
            }

            myLogger.Info( "Loading {0}", string.Join( ",", traceFiles ) );

            IsBusy = true;

            var progress = new Progress<IProgressInfo>( pi => CurrentProgress = pi );

            var log = await myPersistencyService.LoadAsync( traceFiles, progress );

            var factory = new PresentationFactory();
            myPresentation = factory.CreateFlameSetPresentation( log );

            if( myFlamesBrowserViewModel == null )
            {
                FlamesBrowserViewModel = new FlamesBrowserViewModel();
            }

            FlamesBrowserViewModel.Presentation = myPresentation;

            SaveAsCommand.RaiseCanExecuteChanged();
            SaveSnapshotCommand.RaiseCanExecuteChanged();

            Application.Current.Dispatcher.Invoke( new Action( () => IsBusy = false ) );
        }

        string IDropable.DataFormat
        {
            get { return DataFormats.FileDrop; }
        }

        bool IDropable.IsDropAllowed( object data, DropLocation location )
        {
            return ( ( string[] )data ).All( f => myPersistencyService.CanLoad( f ) );
        }

        void IDropable.Drop( object data, DropLocation location )
        {
            LoadTraces( ( string[] )data );
        }

        public InteractionRequest<INotification> ShowLogRequest { get; private set; }

        public ICommand ShowLogCommand { get; private set; }

        private void OnShowLog()
        {
            var notification = new Notification();
            notification.Title = "Log";

            ShowLogRequest.Raise( notification, n => { } );
        }
    }
}
