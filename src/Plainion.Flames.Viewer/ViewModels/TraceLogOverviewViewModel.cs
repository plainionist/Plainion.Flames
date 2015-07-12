using System;
using System.ComponentModel.Composition;
using System.Linq;
using Plainion.Flames.Infrastructure.ViewModels;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Viewer.ViewModels
{
    [Export]
    class TraceLogOverviewViewModel : ViewModelBase
    {
        private int myProcessCount;
        private int myThreadCount;
        private int myCallCount;

        public string Description { get { return "General"; } }

        public bool ShowTab { get { return true; } }

        protected override void OnPresentationChanged()
        {
            if (Presentation == null)
            {
                ProcessCount = -1;
                ThreadCount = -1;
                CallCount = -1;
            }
            else
            {
                ProcessCount = Presentation.Flames
                    .Select(t => t.ProcessId)
                    .Distinct()
                    .Count();

                ThreadCount = Presentation.Flames.Count();

                CallCount = Presentation.Flames
                    .SelectMany(t => t.Activities)
                    .Count();
            }

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
    }
}
