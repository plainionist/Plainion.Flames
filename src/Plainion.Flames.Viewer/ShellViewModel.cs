using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;
using Plainion.Flames.Viewer.Model;
using Plainion.Flames.Viewer.Services;
using Plainion.Flames.Viewer.ViewModels;
using Plainion.Logging;
using Plainion.Prism.Events;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Progress;
using Plainion.Windows.Interactivity.DragDrop;

namespace Plainion.Flames.Viewer
{
    [Export]
    class ShellViewModel : BindableBase, IDropable
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(ShellViewModel));

        private LoaderSerivce myLoaderService;
        private FlamesBrowserViewModel myFlamesBrowserViewModel;
        private bool myIsBusy;
        private IProgressInfo myCurrentProgress;
        private Solution mySolution;

        [ImportingConstructor]
        internal ShellViewModel(IEventAggregator eventAggregator, LoaderSerivce loaderService, TraceLoaderService traceLoader,
            Solution solution)
        {
            myLoaderService = loaderService;
            mySolution = solution;

            traceLoader.UILoadAction = LoadTraces;

            OpenCommand = new DelegateCommand(OnOpen);
            SaveAsCommand = new DelegateCommand(OnSaveAs, () => Project != null);
            SaveSnapshotCommand = new DelegateCommand(OnSaveSnapshot, () => Project != null && Project.Presentation != null);
            CloseCommand = new DelegateCommand(() => Application.Current.Shutdown());

            OpenFileRequest = new InteractionRequest<OpenFileDialogNotification>();
            SaveFileRequest = new InteractionRequest<SaveFileDialogNotification>();

            ShowLogRequest = new InteractionRequest<INotification>();
            ShowLogCommand = new DelegateCommand(OnShowLog);

            eventAggregator.GetEvent<ApplicationReadyEvent>().Subscribe(x => LoadTraceFromCommandLine());
        }

        // currently only one project supported
        private Project Project
        {
            get { return mySolution.Projects.Count == 0 ? null : mySolution.Projects.Single(); }
        }

        public FlamesBrowserViewModel FlamesBrowserViewModel
        {
            get { return myFlamesBrowserViewModel; }
            set { SetProperty(ref myFlamesBrowserViewModel, value); }
        }

        public bool IsBusy
        {
            get { return myIsBusy; }
            set { SetProperty(ref myIsBusy, value); }
        }

        public IProgressInfo CurrentProgress
        {
            get { return myCurrentProgress; }
            set { SetProperty(ref myCurrentProgress, value); }
        }

        public DelegateCommand OpenCommand { get; private set; }

        public InteractionRequest<OpenFileDialogNotification> OpenFileRequest { get; private set; }

        private void OnOpen()
        {
            var notification = new OpenFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = myLoaderService.OpenTraceFilter;
            notification.FilterIndex = 0;
            notification.MultiSelect = true;

            OpenFileRequest.Raise(notification,
                n =>
                {
                    if (n.Confirmed)
                    {
                        LoadTraces(n.FileNames);
                    }
                });
        }

        public DelegateCommand SaveAsCommand { get; private set; }

        public InteractionRequest<SaveFileDialogNotification> SaveFileRequest { get; private set; }

        private void OnSaveAs()
        {
            var notification = new SaveFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = myLoaderService.SaveTraceFilter;
            notification.FilterIndex = 0;

            SaveFileRequest.Raise(notification, async n =>
            {
                if (n.Confirmed)
                {
                    IsBusy = true;
                    var progress = new Progress<IProgressInfo>(pi => CurrentProgress = pi);
                    await myLoaderService.ExportAsync(Project.TraceLog, n.FileName, progress);
                    IsBusy = false;
                }
            });
        }

        public DelegateCommand SaveSnapshotCommand { get; private set; }

        private void OnSaveSnapshot()
        {
            var notification = new SaveFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = myLoaderService.SaveTraceFilter;
            notification.FilterIndex = 0;

            SaveFileRequest.Raise(notification, async n =>
            {
                if (n.Confirmed)
                {
                    IsBusy = true;
                    var progress = new Progress<IProgressInfo>(pi => CurrentProgress = pi);
                    await myLoaderService.ExportAsync(CreateSnapshot(Project.Presentation), n.FileName, progress);
                    IsBusy = false;
                }
            });
        }

        private ITraceLog CreateSnapshot(FlameSetPresentation presentation)
        {
            var builder = new TraceModelViewBuilder(presentation.Model);

            builder.SetCreationTime(presentation.Model.CreationTime);
            builder.SetTraceDuration(presentation.Model.TraceDuration);

            var flames = presentation.Flames
                .Where(t => t.Visibility == ContentVisibility.Visible);

            foreach (var flame in flames)
            {
                builder.Add(flame.Model);

                foreach (var events in presentation.Model.AssociatedEvents.GetAllFor<IAssociatedEvents>(flame.Model))
                {
                    builder.Add(events);
                }
            }

            return builder.Complete();
        }

        public ICommand CloseCommand { get; private set; }

        private void LoadTraceFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
            {
                return;
            }

            string traceFile = args[1];

            if (File.Exists(traceFile))
            {
                Application.Current.Dispatcher.BeginInvoke(new Action<string>(f => LoadTraces(f)), traceFile);
            }
        }

        private async void LoadTraces(params string[] traceFiles)
        {
            var oldProject = Project;
            if (oldProject != null)
            {
                myLoaderService.Unload(oldProject);
                mySolution.Projects.Remove(oldProject);
            }

            myLogger.Info("Loading {0}", string.Join(",", traceFiles));

            IsBusy = true;

            var project = new Project(traceFiles);
            var progress = new Progress<IProgressInfo>(pi => CurrentProgress = pi);

            await myLoaderService.LoadAsync(project, progress);

            mySolution.Projects.Add(project);

            await myLoaderService.CreatePresentationAsync(project, progress);

            if (myFlamesBrowserViewModel == null)
            {
                FlamesBrowserViewModel = new FlamesBrowserViewModel();
            }

            FlamesBrowserViewModel.Presentation = Project.Presentation;

            SaveAsCommand.RaiseCanExecuteChanged();
            SaveSnapshotCommand.RaiseCanExecuteChanged();
        
            IsBusy = false;

            if (project.WasDeserialized)
            {
                // http://stackoverflow.com/questions/13026826/execute-command-after-view-is-loaded-wpf-mvvm
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                    {
                        // we loaded user settings from disk which might filter out certain threads or calls.
                        // lets display settings window to the user so that it is more obvious that everything
                        // is visible in the flames.
                        FlamesBrowserViewModel.SpawnSettingsWindowCommand.Execute(null);

                        // TODO: we want to navigate to process-threads-view but currently we cannot access viewmodel :(
                        FlamesBrowserViewModel.Settings.SelectedTabIndex = 1;
                    }));
            }
        }

        string IDropable.DataFormat
        {
            get { return DataFormats.FileDrop; }
        }

        bool IDropable.IsDropAllowed(object data, DropLocation location)
        {
            return ( ( string[] )data ).All(f => myLoaderService.CanLoad(f));
        }

        void IDropable.Drop(object data, DropLocation location)
        {
            LoadTraces(( string[] )data);
        }

        public InteractionRequest<INotification> ShowLogRequest { get; private set; }

        public ICommand ShowLogCommand { get; private set; }

        private void OnShowLog()
        {
            var notification = new Notification();
            notification.Title = "Log";

            ShowLogRequest.Raise(notification, n => { });
        }
    }
}
