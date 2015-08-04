using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames
{
    public class PresentationFactory
    {
        public FlameSetPresentation CreateFlameSetPresentation(TraceLog traceLog)
        {
            return CreateFlameSetPresentation(traceLog, new PresentationFactorySettings());
        }

        public FlameSetPresentation CreateFlameSetPresentation(TraceLog traceLog, PresentationFactorySettings settings)
        {
            var builder = CreateBuilder(settings);
            builder.HideEmptyFlames = settings.HideEmptyFlames;
            builder.ShowAbsoluteTimestamps = settings.ShowAbsoluteTimestamps;
            return builder.CreateFlameSetPresentation(traceLog);
        }

        private AbstractPresentationBuilder CreateBuilder(PresentationFactorySettings settings)
        {
            if (settings.InterpolateBrokenStackCalls)
            {
                return new InterpolatingBrokenStacksPresentationBuilder();
            }
            else
            {
                return new DefaultPresentationBuilder();
            }
        }
    }
}
