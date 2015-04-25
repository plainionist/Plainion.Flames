using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Plainion;

namespace Plainion.Flames.Infrastructure.Controls
{
    // TODO: we merge input/result and UI interaction together here ... lets find a better design
    public class TracesTree : BindableBase, IDisposable
    {
        private ICollectionView myVisibleProcesses;
        private string myFilter;
        private IEnumerable<TraceProcessNode> myProcesses;

        public TracesTree()
        {
            ShowAllCommand = new DelegateCommand( () => ShowHideAllVisible( true ) );
            HideAllCommand = new DelegateCommand( () => ShowHideAllVisible( false ) );
        }

        private void ShowHideAllVisible( bool isMarked )
        {
            foreach( TraceProcessNode node in VisibleProcesses )
            {
                node.IsVisible = isMarked;
            }
        }

        public ICommand ShowAllCommand { get; private set; }

        public ICommand HideAllCommand { get; private set; }

        public IEnumerable<TraceProcessNode> Processes
        {
            get { return myProcesses; }
            set
            {
                var oldProcesses = myProcesses;

                if( SetProperty( ref myProcesses, value ) )
                {
                    if( oldProcesses != null )
                    {
                        foreach( var process in oldProcesses )
                        {
                            process.PropertyChanged -= OnProcessPropertyChanged;
                        }
                    }

                    if( myProcesses != null )
                    {
                        foreach( var process in myProcesses )
                        {
                            process.PropertyChanged += OnProcessPropertyChanged;
                        }
                    }

                    myVisibleProcesses = null;
                    OnPropertyChanged( "VisibleProcesses" );
                }
            }
        }

        private void OnProcessPropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsVisible" )
            {
                OnPropertyChanged( "MarkAll" );
            }
        }

        public bool? MarkAll
        {
            get
            {
                if( myProcesses == null )
                {
                    return null;
                }

                if( Processes.All( p => p.IsVisible == true ) )
                {
                    return true;
                }

                if( Processes.All( p => p.IsVisible == false ) )
                {
                    return false;
                }

                return null;
            }
            set
            {
                foreach( var t in Processes )
                {
                    t.IsVisible = value.HasValue && value.Value;
                }

                OnPropertyChanged( "MarkAll" );
            }
        }

        public virtual void Dispose()
        {
            if( myProcesses != null )
            {
                foreach( var process in myProcesses )
                {
                    process.Dispose();
                }
            }

            Processes = null;
            myVisibleProcesses = null;
        }

        public string Filter
        {
            get { return myFilter; }
            set
            {
                if( SetProperty( ref myFilter, value ) )
                {
                    VisibleProcesses.Refresh();
                }
            }
        }

        public ICollectionView VisibleProcesses
        {
            get
            {
                if( myVisibleProcesses == null && myProcesses != null )
                {
                    myVisibleProcesses = CollectionViewSource.GetDefaultView( Processes );
                    myVisibleProcesses.Filter = x => FilterNodes( ( TraceProcessNode )x );

                    OnPropertyChanged( "VisibleProcesses" );
                }
                return myVisibleProcesses;
            }
        }

        private bool FilterNodes( TraceProcessNode item )
        {
            if( string.IsNullOrWhiteSpace( myFilter ) )
            {
                return true;
            }

            return item.Name.Contains( myFilter, StringComparison.OrdinalIgnoreCase )
                || item.ProcessId.ToString().Contains( myFilter );
        }
    }
}
