using Prism.Mvvm;

namespace Plainion.Flames.Infrastructure.Controls
{
    public class TraceThreadNode : EditableTreeNode
    {
        private bool myIsVisible;
        private int myThreadId;
        private string myName;

        public int ThreadId
        {
            get { return myThreadId; }
            set { SetProperty( ref myThreadId, value ); }
        }

        public string Name
        {
            get { return myName ?? "Unknown"; }
            set
            {
                if( string.IsNullOrWhiteSpace( value ) || value == "Unknown" )
                {
                    value = null;
                }

                if( SetProperty( ref myName, value ) )
                {
                    OnNameChanged();
                }
            }
        }

        protected virtual void OnNameChanged()
        {
        }

        public bool IsVisible
        {
            get { return myIsVisible; }
            set
            {
                if( SetProperty( ref myIsVisible, value ) )
                {
                    OnIsVisibleChanged();
                }
            }
        }

        protected virtual void OnIsVisibleChanged()
        {
        }
    }
}
