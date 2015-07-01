using System;
using System.ComponentModel.Composition;
using System.Linq;
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

        internal FlamesSettingsViewModel()
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
    }
}
