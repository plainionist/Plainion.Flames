using System.Runtime.Serialization;
using Prism.Mvvm;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Modules.Filters.Model
{
    [DataContract(Name = "DurationFilter", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/Filters")]
    class DurationFilter : SerializableBindableBase, ICallFilter
    {
        private long myThreshold;
        private double myMaximum;

        public DurationFilter()
        {
            Maximum = 1000;
        }

        [DataMember(Name = "Maximum")]
        public double Maximum
        {
            get { return myMaximum; }
            set { SetProperty( ref myMaximum, value ); }
        }

        [DataMember(Name = "Threshold")]
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

        public bool? IsVisible( Activity activity )
        {
            if( !IsApplied )
            {
                return null;
            }

            if( activity.Duration >= myThreshold )
            {
                return null;
            }

            return IsShowFilter;
        }
    }
}
