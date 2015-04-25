using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.Flames.Infrastructure.Controls
{
    [DebuggerDisplay("{Name}, PID={ProcessId}")]
    public class TraceProcessNode : EditableTreeNode, IDisposable
    {
        private int myProcessId;
        private string myName;
        private IEnumerable<TraceThreadNode> myThreads;

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
        
        public IEnumerable<TraceThreadNode> Threads
        {
            get { return myThreads; }
            set
            {
                var oldThreads = myThreads;

                if( SetProperty( ref myThreads, value ) )
                {
                    if( oldThreads != null )
                    {
                        foreach( var t in oldThreads )
                        {
                            t.PropertyChanged -= OnThreadPropertyChanged;
                        }
                    }

                    if( myThreads != null )
                    {
                        foreach( var t in myThreads )
                        {
                            t.PropertyChanged += OnThreadPropertyChanged;
                        }
                    }
                }
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

        public virtual void Dispose()
        {
            if( myThreads != null )
            {
                foreach( var thread in myThreads.OfType<IDisposable>() )
                {
                    thread.Dispose();
                }
            }

            Threads = null;
        }
    }
}
