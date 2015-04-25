using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.Flames.Model
{
    public class TraceThread : BindableBase
    {
        private string myName;

        internal TraceThread( TraceProcess process, int threadId )
        {
            Process = process;
            ThreadId = threadId;
        }

        public TraceProcess Process { get; private set; }

        public int ThreadId { get; private set; }

        /// <summary>
        /// Either null or non-empty, non-whitespace string
        /// </summary>
        public string Name
        {
            get { return myName; }
            set
            {
                if( string.IsNullOrWhiteSpace( value ) )
                {
                    value = null;
                }

                SetProperty( ref myName, value );
            }
        }
    }
}
