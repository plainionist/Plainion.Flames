﻿using System.ComponentModel.Composition;
using Plainion.Flames.Infrastructure.ViewModels;

namespace Plainion.Flames.Modules.Filters.ViewModels
{
    [Export]
    class OtherFiltersViewModel : ViewModelBase
    {
        private bool myInterpolateBrokenStackCalls;

        public bool InterpolateBrokenStackCalls
        {
            get { return myInterpolateBrokenStackCalls; }
            set
            {
                if (SetProperty(ref myInterpolateBrokenStackCalls, value))
                {
                    OnInterpolateBrokenStackCallsChanged();
                }
            }
        }

        private void OnInterpolateBrokenStackCallsChanged()
        {
            if (Presentation == null)
            {
                return;
            }

            var settings = new PresentationFactorySettings();
            settings.InterpolateBrokenStackCalls = myInterpolateBrokenStackCalls;
            var factory = new PresentationFactory();
            var presentation = factory.CreateFlameSetPresentation(TraceLog, settings);

            ProjectService.Project.Presentation = presentation;
        }

        protected override void OnProjectChanging()
        {
            // set presentation already to null to ensure that we do not recalc flames by accident
            Presentation = null;

            // do not remember this setting - always start new tracelog without interpolation
            InterpolateBrokenStackCalls = false;
        }
    }
}
