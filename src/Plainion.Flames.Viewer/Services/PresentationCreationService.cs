using System;
using System.ComponentModel.Composition;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Viewer.Services
{
    [Export(typeof(IPresentationCreationService))]
    class PresentationCreationService : IPresentationCreationService
    {
        public PresentationCreationService()
        {
            Settings = new PresentationFactorySettings();
        }

        public PresentationFactorySettings Settings { get; private set; }

        public FlameSetPresentation CreateFlameSetPresentation(TraceLog traceLog)
        {
            var factory = new PresentationFactory();
            return factory.CreateFlameSetPresentation(traceLog, Settings);
        }
    }
}
