using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.Flames.Model
{
    public class TraceProcess : BindableBase
    {
        private string myName;

        internal TraceProcess( TraceLog log, int processId )
        {
            Log = log;
            ProcessId = processId;
        }

        public TraceLog Log { get; internal set; }

        public int ProcessId { get; private set; }

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
