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
        private bool myShowSumFlames;

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

        public bool ShowSumFlames
        {
            get { return myShowSumFlames; }
            set
            {
                if (SetProperty(ref myShowSumFlames, value))
                {
                    OnShowSumFlamesChanged();
                }
            }
        }

        private void OnShowSumFlamesChanged()
        {
            if (Presentation == null)
            {
                return;
            }

            var factory = new PresentationFactory();
            factory.ShowSumFlames = myShowSumFlames;
            var presentation = factory.CreateFlameSetPresentation(TraceLog);

            ProjectService.Project.Presentation = presentation;
        }

        protected override void OnProjectChanging()
        {
            // set presentation already to null to ensure that we do not recalc flames by accident
            Presentation = null;

            // do not remember this setting - always start new tracelog without interpolation
            ShowSumFlames = false;
        }
    }
}
