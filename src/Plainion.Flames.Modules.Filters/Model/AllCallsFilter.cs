using Plainion.Flames.Presentation;

namespace Plainion.Flames.Modules.Filters.Model
{
    class AllCallsFilter : NameFilterBase
    {
        public AllCallsFilter()
            : base( FilterTarget.Method, "All calls" )
        {
        }

        public override bool? IsVisible( Activity call )
        {
            if( !IsApplied )
            {
                return null;
            }

            return IsShowFilter;
        }

        public override bool? IsVisible( FilterTarget target, string value )
        {
            if( !IsApplied )
            {
                return null;
            }

            return IsShowFilter;
        }
    }
}
