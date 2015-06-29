using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.Flames.Infrastructure.Controls;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Infrastructure.ViewModels;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Viewer.ViewModels
{
    [Export]
    class FlamesSettingsViewModel : ViewModelBase
    {
        private int myProcessCount;
        private int myThreadCount;
        private int myCallCount;
        private int mySelectedTabIndex;

        [ImportingConstructor]
        internal FlamesSettingsViewModel(IProjectService projectService)
            : base(projectService)
        {
            TracesTreeSource = new TracesTree();
        }


        public TracesTree TracesTreeSource { get; private set; }

        protected override void OnPresentationChanged(FlameSetPresentation oldValue)
        {
            TracesTreeSource.Processes = Presentation.Flames
                .GroupBy(x => x.Model.Process)
                .OrderBy(x => x.Key.Name)
                .Select(x => new SelectableProcessAdapter(x.Key, x.AsEnumerable()))
                .ToList();

            ProcessCount = Presentation.Flames
                .Select(t => t.ProcessId)
                .Distinct()
                .Count();

            ThreadCount = Presentation.Flames.Count;

            CallCount = Presentation.Flames
                .SelectMany(t => t.Activities)
                .Count();

            OnPropertyChanged(() => TraceDuration);

            if (mySelectedTabContent != null)
            {
                InjectPresentation(mySelectedTabContent);
            }
        }

        public TimeSpan TraceDuration
        {
            get
            {
                return Presentation == null ? TimeSpan.MinValue : TimeSpan.FromMilliseconds(Presentation.Model.TraceDuration / 1000);
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

            presentationProperty.SetValue(mySelectedTabContent, Presentation);
        }
    }
}
