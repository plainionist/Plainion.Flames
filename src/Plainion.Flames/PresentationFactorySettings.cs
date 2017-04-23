
using Prism.Mvvm;
namespace Plainion.Flames
{
    public class PresentationFactorySettings : BindableBase
    {
        private bool myInterpolateBrokenStackCalls;
        private bool myShowSumFlames;
        private bool myHideEmptyFlames;
        private bool myShowAbsoluteTimestamps;

        public PresentationFactorySettings()
        {
            Reset();
        }

        public bool InterpolateBrokenStackCalls
        {
            get { return myInterpolateBrokenStackCalls; }
            set { SetProperty(ref myInterpolateBrokenStackCalls, value); }
        }

        public bool ShowSumFlames
        {
            get { return myShowSumFlames; }
            set { SetProperty(ref myShowSumFlames, value); }
        }

        public bool HideEmptyFlames
        {
            get { return myHideEmptyFlames; }
            set { SetProperty(ref myHideEmptyFlames, value); }
        }

        public bool ShowAbsoluteTimestamps
        {
            get { return myShowAbsoluteTimestamps; }
            set { SetProperty(ref myShowAbsoluteTimestamps, value); }
        }

        public void Reset()
        {
            InterpolateBrokenStackCalls = false;
            ShowSumFlames = false;
            HideEmptyFlames = true;
            ShowAbsoluteTimestamps = false;
        }
    }
}
