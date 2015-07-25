using System.ComponentModel.Composition;
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
                if( SetProperty( ref myInterpolateBrokenStackCalls, value ) )
                {
                    RemoveBrokenStacks();
                }
            }
        }

        private void RemoveBrokenStacks()
        {
            if( Presentation == null )
            {
                return;
            }

            var factory = new PresentationFactory();
            factory.InterpolateBrokenStackCalls = true;
            var presentation = factory.CreateFlameSetPresentation( TraceLog );

            ProjectService.Project.Presentation = presentation;
        }
    }
}
