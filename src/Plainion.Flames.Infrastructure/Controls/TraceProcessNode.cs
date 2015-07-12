using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Plainion.Flames.Infrastructure.Controls
{
    [DebuggerDisplay( "{Name}, PID={ProcessId}" )]
    public class TraceProcessNode : EditableTreeNode
    {
        private int myProcessId;
        private string myName;
        // http://referencesource.microsoft.com/#PresentationFramework/Framework/MS/Internal/Data/ViewManager.cs
        // we use ObservableCollection here to avoid that ViewManager holds a strong ref to our collection.
        // this will be removed in some time so it is not a real leak (see comments in linked source code) but as
        // we hold havy data here (flames) we want to avoid holding it longer than necessary
        private ObservableCollection<TraceThreadNode> myThreads;

        // only used if here are no threads
        private bool myIsVisible;

        public TraceProcessNode()
        {
            // default if there are no threads
            myIsVisible = true;
        }

        public int ProcessId
        {
            get { return myProcessId; }
            set { SetProperty( ref myProcessId, value ); }
        }

        public string Name
        {
            get { return myName; }
            set
            {
                if( string.IsNullOrWhiteSpace( value ) )
                {
                    value = "Unknown";
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

        /// <summary>
        /// Creates a copy
        /// </summary>
        public IEnumerable<TraceThreadNode> Threads
        {
            get { return myThreads; }
            set
            {
                if( myThreads != null )
                {
                    foreach( var t in myThreads )
                    {
                        t.PropertyChanged -= OnThreadPropertyChanged;
                    }
                }

                myThreads = value == null ? null : new ObservableCollection<TraceThreadNode>( value );
                
                if( myThreads != null )
                {
                    foreach( var t in myThreads )
                    {
                        t.PropertyChanged += OnThreadPropertyChanged;
                    }
                }

                OnPropertyChanged( "Threads" );
            }
        }

        private void OnThreadPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsVisible" )
            {
                OnPropertyChanged( "IsVisible" );
            }
        }

        public bool? IsVisible
        {
            get
            {
                if( myThreads == null )
                {
                    return myIsVisible;
                }

                if( Threads.All( t => t.IsVisible ) )
                {
                    return true;
                }

                if( Threads.All( t => !t.IsVisible ) )
                {
                    return false;
                }

                return null;
            }
            set
            {
                if( myThreads == null )
                {
                    myIsVisible = value == null ? false : value.Value;
                }
                else
                {
                    foreach( var t in Threads )
                    {
                        t.IsVisible = value.HasValue && value.Value;
                    }
                }

                OnPropertyChanged( "IsVisible" );
            }
        }
    }
}
