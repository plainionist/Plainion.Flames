using Plainion.Flames.Presentation;
using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.Flames.Modules.Filters.Model
{
    class DurationFilter : BindableBase, ICallFilter
    {
        private long myThreshold;
        private double myMaximum;

        public DurationFilter()
        {
            Maximum = 1000;
        }

        public double Maximum
        {
            get { return myMaximum; }
            set { SetProperty( ref myMaximum, value ); }
        }

        public double Threshold
        {
            get { return myThreshold / 1000; }
            set { SetProperty( ref myThreshold, ( long )( value * 1000 ) ); }
        }

        public string Label { get { return "Call duration"; } }

        public bool IsShowFilter
        {
            get { return false; }
        }

        public bool IsApplied
        {
            get { return myThreshold > 0; }
        }

        public bool? IsVisible( Activity call )
        {
            if( !IsApplied )
            {
                return null;
            }

            if( call.Duration >= myThreshold )
            {
                return null;
            }

            return IsShowFilter;
        }
    }
}
