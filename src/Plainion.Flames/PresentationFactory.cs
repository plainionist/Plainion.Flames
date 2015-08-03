using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames
{
    public class PresentationFactory
    {
        public bool InterpolateBrokenStackCalls { get; set; }

        public bool ShowSumFlames { get; set; }

        public FlameSetPresentation CreateFlameSetPresentation(TraceLog traceLog)
        {
            if( InterpolateBrokenStackCalls )
            {
                var builder = new InterpolatingBrokenStacksPresentationBuilder();
                return builder.CreateFlameSetPresentation( traceLog );
            }
            else
            {
                var builder = new DefaultPresentationBuilder();
                return builder.CreateFlameSetPresentation( traceLog );
            }
        }
    }
}
