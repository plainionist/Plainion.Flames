using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.Flames.Infrastructure.Controls
{
    // TODO: we merge input/result and UI interaction together here ... lets find a better design
    public class TracesTree : BindableBase
    {
        private ICollectionView myVisibleProcesses;
        private string myFilter;

        // http://referencesource.microsoft.com/#PresentationFramework/Framework/MS/Internal/Data/ViewManager.cs
        // we use ObservableCollection here to avoid that ViewManager holds a strong ref to our collection.
        // this will be removed in some time so it is not a real leak (see comments in linked source code) but as
        // we hold havy data here (flames) we want to avoid holding it longer than necessary
        private ObservableCollection<TraceProcessNode> myProcesses;

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

        /// <summary>
        /// Creates a copy
        /// </summary>
        public IEnumerable<TraceProcessNode> Processes
        {
            get { return myProcesses; }
            set
            {
                if( myProcesses != null )
                {
                    foreach( var process in myProcesses )
                    {
                        process.PropertyChanged -= OnProcessPropertyChanged;
                    }
                }

                myProcesses = value == null ? null : new ObservableCollection<TraceProcessNode>( value );

                if( myProcesses != null )
                {
                    foreach( var process in myProcesses )
                    {
                        process.PropertyChanged += OnProcessPropertyChanged;
                    }
                }

                OnPropertyChanged( "Processes" );

                myVisibleProcesses = null;
                OnPropertyChanged( "VisibleProcesses" );
            }
        }

        private void OnProcessPropertyChanged( object sender, PropertyChangedEventArgs e )
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
