using System.ComponentModel;
using System.ComponentModel.Composition;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Infrastructure.ViewModels;

namespace Plainion.Flames.Modules.Filters.ViewModels
{
    [Export]
    class OtherFiltersViewModel : ViewModelBase
    {
        private IPresentationCreationService myPresentationCreationService;

        [ImportingConstructor]
        public OtherFiltersViewModel(IPresentationCreationService presentationService)
        {
            myPresentationCreationService = presentationService;

            PropertyChangedEventManager.AddHandler(myPresentationCreationService.Settings, OnInterpolateBrokenStackCallsChanged, "InterpolateBrokenStackCalls");
        }

        public bool InterpolateBrokenStackCalls
        {
            get { return myPresentationCreationService.Settings.InterpolateBrokenStackCalls; }
            set { myPresentationCreationService.Settings.InterpolateBrokenStackCalls = value; }
        }

        private void OnInterpolateBrokenStackCallsChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);

            if (Presentation == null)
            {
                return;
            }

            ProjectService.Project.Presentation = myPresentationCreationService.CreateFlameSetPresentation(TraceLog);
        }

        protected override void OnProjectChanging()
        {
            // set presentation already to null to ensure that we do not recalc flames by accident
            Presentation = null;
        }
    }
}
