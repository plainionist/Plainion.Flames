using System.Runtime.Serialization;
using Prism.Mvvm;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Modules.Filters.Model
{
    [DataContract]
    abstract class NameFilterBase : SerializableBindableBase, INameFilter
    {
        private bool myIsShowFilter;
        private bool myIsApplied;

        protected NameFilterBase( FilterTarget target, string label )
        {
            Contract.RequiresNotNullNotEmpty( label, "label" );

            Target = target;
            Label = label;
        }

        [DataMember(Name = "Label")]
        public string Label { get; private set; }

        [DataMember(Name = "Target")]
        public FilterTarget Target { get; private set; }

        [DataMember(Name = "IsShowFilter")]
        public bool IsShowFilter
        {
            get { return myIsShowFilter; }
            set { SetProperty( ref myIsShowFilter, value ); }
        }

        [DataMember(Name = "IsApplied")]
        public bool IsApplied
        {
            get { return myIsApplied; }
            set { SetProperty( ref myIsApplied, value ); }
        }

        public abstract bool? IsVisible( FilterTarget target, string value );

        public abstract bool? IsVisible( Activity activity );
    }
}
