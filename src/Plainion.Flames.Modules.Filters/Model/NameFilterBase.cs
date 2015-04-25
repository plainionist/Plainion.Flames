using Plainion.Flames.Presentation;
using Microsoft.Practices.Prism.Mvvm;
using Plainion;

namespace Plainion.Flames.Modules.Filters.Model
{
    abstract class NameFilterBase : BindableBase, INameFilter
    {
        private bool myIsShowFilter;
        private bool myIsApplied;

        protected NameFilterBase( FilterTarget target, string label )
        {
            Contract.RequiresNotNullNotEmpty( label, "label" );

            Target = target;
            Label = label;
        }

        public string Label { get; private set; }

        public FilterTarget Target { get; private set; }

        public bool IsShowFilter
        {
            get { return myIsShowFilter; }
            set { SetProperty( ref myIsShowFilter, value ); }
        }

        public bool IsApplied
        {
            get { return myIsApplied; }
            set { SetProperty( ref myIsApplied, value ); }
        }

        public abstract bool? IsVisible( FilterTarget target, string value );

        public abstract bool? IsVisible( Activity call );
    }
}
