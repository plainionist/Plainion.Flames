using System;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Infrastructure.Services
{
    public interface IPresentationCreationService
    {
        PresentationFactorySettings Settings { get; }
        
        FlameSetPresentation CreateFlameSetPresentation(TraceLog traceLog);
    }
}
