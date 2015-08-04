
namespace Plainion.Flames
{
    public class PresentationFactorySettings
    {
        public PresentationFactorySettings()
        {
            InterpolateBrokenStackCalls = false;
            ShowSumFlames = false;
            HideEmptyFlames = true;
            ShowAbsoluteTimestamps = false;
        }

        public bool InterpolateBrokenStackCalls { get; set; }

        public bool ShowSumFlames { get; set; }

        public bool HideEmptyFlames { get; set; }

        public bool ShowAbsoluteTimestamps { get; set; }

    }
}
