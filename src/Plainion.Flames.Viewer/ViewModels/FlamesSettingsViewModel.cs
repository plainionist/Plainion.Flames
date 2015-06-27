using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.Flames.Infrastructure.Controls;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Viewer.ViewModels
{
    [Export]
    class FlamesSettingsViewModel : BindableBase
    {
        private FlameSetPresentation myPresentation;
        private int myProcessCount;
        private int myThreadCount;
        private int myCallCount;
        private int mySelectedTabIndex;
        private IProjectService myProjectService;

        [ImportingConstructor]
        internal FlamesSettingsViewModel( IProjectService projectService )
        {
            myProjectService = projectService;
            myProjectService.ProjectChanged += OnProjectChanged;

            TracesTreeSource = new TracesTree();
        }

        private void OnProjectChanged( object sender, EventArgs e )
        {
            if( myProjectService.Project.Presentation != null )
            {
                Presentation = myProjectService.Project.Presentation;
            }

            PropertyChangedEventManager.AddHandler( myProjectService.Project, OnPresentationChanged,
                PropertySupport.ExtractPropertyName( () => myProjectService.Project.Presentation ) );
        }

        private void OnPresentationChanged( object sender, PropertyChangedEventArgs e )
        {
            Presentation = myProjectService.Project.Presentation;
        }
        
        public TracesTree TracesTreeSource { get; private set; }

        public FlameSetPresentation Presentation
        {
            get { return myPresentation; }
            private set
            {
                if (SetProperty(ref myPresentation, value))
                {
                    TracesTreeSource.Processes = myPresentation.Flames
                        .GroupBy(x => x.Model.Process)
                        .OrderBy(x => x.Key.Name)
                        .Select(x => new SelectableProcessAdapter(x.Key, x.AsEnumerable()))
                        .ToList();

                    ProcessCount = myPresentation.Flames
                        .Select(t => t.ProcessId)
                        .Distinct()
                        .Count();

                    ThreadCount = myPresentation.Flames.Count;

                    CallCount = myPresentation.Flames
                        .SelectMany(t => t.Activities)
                        .Count();

                    OnPropertyChanged( () => TraceDuration );

                    if (mySelectedTabContent != null)
                    {
                        InjectPresentation(mySelectedTabContent);
                    }
                }
            }
        }

        public TimeSpan TraceDuration
        {
            get
            {
                return myPresentation == null ? TimeSpan.MinValue : TimeSpan.FromMilliseconds(myPresentation.Model.TraceDuration / 1000);
            }
        }

        public int ProcessCount
        {
            get { return myProcessCount; }
            set { SetProperty(ref myProcessCount, value); }
        }

        public int ThreadCount
        {
            get { return myThreadCount; }
            set { SetProperty(ref myThreadCount, value); }
        }

        public int CallCount
        {
            get { return myCallCount; }
            set { SetProperty(ref myCallCount, value); }
        }

        public int SelectedTabIndex
        {
            get { return mySelectedTabIndex; }
            set { SetProperty(ref mySelectedTabIndex, value); }
        }

        // TODO: workaround to inject presentations
        private object mySelectedTabContent;
        public object SelectedTabItem
        {
            set
            {
                var frameworkElement = value as FrameworkElement;
                if (frameworkElement == null || frameworkElement.DataContext == null)
                {
                    return;
                }

                mySelectedTabContent = frameworkElement.DataContext;

                InjectPresentation(mySelectedTabContent);
            }
        }

        private void InjectPresentation(object dataContext)
        {
            var presentationProperty = mySelectedTabContent.GetType().GetProperty("Presentation");
            if (presentationProperty == null)
            {
                return;
            }

            presentationProperty.SetValue(mySelectedTabContent, myPresentation);
        }
    }
}
