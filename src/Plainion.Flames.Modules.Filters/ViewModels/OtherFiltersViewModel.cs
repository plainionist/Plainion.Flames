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
                if (SetProperty(ref myInterpolateBrokenStackCalls, value))
                {

                }
            }
        }
    }
}
