using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Plainion.Flames.Modules.Filters.Model;
using Plainion.Flames.Presentation;
using Microsoft.Practices.Prism.Commands;
using Plainion;
using Plainion.Prism.Mvvm;

namespace Plainion.Flames.Modules.Filters.ViewModels
{
    [Export]
    class NameFilterViewModel : ValidatableBindableBase
    {
        private CallFilterModule myModule;
        private string myFilter;
        private ICollectionView myPreviewItems;
        private string mySelectedPreviewItem;
        private INameFilter mySelectedFilter;
        private FilterTarget myFilterTarget;
        private bool myPreviewUnmatchedItemsOnly;

        public NameFilterViewModel()
        {
            myPreviewUnmatchedItemsOnly = true;

            AddCommand = new DelegateCommand( OnAdd, () => Filter != null );
            MouseDownCommand = new DelegateCommand<MouseButtonEventArgs>( OnMouseDown );

            DeleteFilterCommand = new DelegateCommand( OnDeleteFilter );
            MoveFilterUpCommand = new DelegateCommand( OnMoveFilterUp );
            MoveFilterDownCommand = new DelegateCommand( OnMoveFilterDown );

            FilterTarget = FilterTarget.Module;
        }

        public CallFilterModule Module
        {
            get { return myModule; }
            set
            {
                var oldModule = myModule;

                if( SetProperty( ref myModule, value ) )
                {
                    if( oldModule != null )
                    {
                        oldModule.NameFilterApplianceChanged -= OnNameFilterApplianceChanged;
                    }

                    myModule.NameFilterApplianceChanged += OnNameFilterApplianceChanged;

                    // update the view
                    OnPropertyChanged( "Filters" );

                    SelectedFilter = null;
                    myPreviewItems = null;
                    Filter = null;
                    PreviewItems.Refresh();
                }
            }
        }

        private void OnNameFilterApplianceChanged( object sender, EventArgs e )
        {
            myPreviewItems = null;
            PreviewItems.Refresh();
        }

        public ObservableCollection<INameFilter> Filters
        {
            get { return myModule != null ? myModule.NameFilters : null; }
        }

        public INameFilter SelectedFilter
        {
            get { return mySelectedFilter; }
            set { SetProperty( ref mySelectedFilter, value ); }
        }

        public FilterTarget FilterTarget
        {
            get { return myFilterTarget; }
            set
            {
                if( SetProperty( ref myFilterTarget, value ) )
                {
                    myPreviewItems = null;
                    OnPropertyChanged( "PreviewItems" );
                }
            }
        }

        public string SelectedPreviewItem
        {
            get { return mySelectedPreviewItem; }
            set { SetProperty( ref mySelectedPreviewItem, value ); }
        }

        public DelegateCommand AddCommand { get; private set; }

        private void OnAdd()
        {
            var filter = new StringContainsFilter( FilterTarget, myFilter );
            filter.IsApplied = true;
            filter.IsShowFilter = true;

            myModule.Push( filter );

            Filter = null;
        }

        public ICommand DeleteFilterCommand { get; private set; }

        private void OnDeleteFilter()
        {
            if( SelectedFilter == null )
            {
                return;
            }

            myModule.Remove( SelectedFilter );

            SelectedFilter = myModule.NameFilters.FirstOrDefault();
        }

        public ICommand MoveFilterUpCommand { get; private set; }

        private void OnMoveFilterUp()
        {
            if( SelectedFilter == null )
            {
                return;
            }

            myModule.MoveUp( SelectedFilter );

            SelectedFilter = myModule.NameFilters.Single( e => e == SelectedFilter );
        }

        public ICommand MoveFilterDownCommand { get; private set; }

        private void OnMoveFilterDown()
        {
            if( SelectedFilter == null )
            {
                return;
            }

            myModule.MoveDown( SelectedFilter );

            SelectedFilter = myModule.NameFilters.Single( e => e == SelectedFilter );
        }

        public ICommand MouseDownCommand { get; private set; }

        private void OnMouseDown( MouseButtonEventArgs args )
        {
            if( args.ClickCount == 2 )
            {
                Filter = SelectedPreviewItem;
            }
        }

        public string Filter
        {
            get { return myFilter; }
            set
            {
                if( string.IsNullOrEmpty( value ) )
                {
                    value = null;
                }

                if( ( myFilter == null ) != ( value == null ) )
                {
                    AddCommand.RaiseCanExecuteChanged();
                }

                if( SetProperty( ref myFilter, value ) )
                {
                    ClearErrors();
                    PreviewItems.Refresh();
                }
            }
        }

        public ICollectionView PreviewItems
        {
            get
            {
                if( myPreviewItems == null && myModule != null )
                {
                    IEnumerable<string> preview = null;

                    switch( FilterTarget )
                    {
                        case FilterTarget.Module: preview = myModule.Presentation.Model.Methods.Select( m => m.Module ); break;
                        case FilterTarget.Namespace: preview = myModule.Presentation.Model.Methods.Select( m => m.Namespace ); break;
                        case FilterTarget.Class: preview = myModule.Presentation.Model.Methods.Select( m => m.Class ); break;
                        case FilterTarget.Method: preview = myModule.Presentation.Model.Methods.Select( m => m.Name ); break;
                        default: throw new NotSupportedException( FilterTarget.ToString() );
                    }

                    preview = preview
                        .Where( i => !string.IsNullOrWhiteSpace( i ) )
                        .Distinct()
                        .OrderBy( n => n )
                        .ToList();

                    myPreviewItems = CollectionViewSource.GetDefaultView( preview );
                    myPreviewItems.Filter = FilterPreviewItems;

                    OnPropertyChanged( "PreviewItems" );
                }
                return myPreviewItems;
            }
        }

        private bool FilterPreviewItems( object item )
        {
            var previewItem = ( string )item;

            if( myPreviewUnmatchedItemsOnly )
            {
                var isMatched = Filters
                    .Where( f => !( f is AllCallsFilter ) )
                    .Select( f => f.IsVisible( FilterTarget, previewItem ) )
                    .FirstOrDefault( r => r != null );

                // matched by any filter ==> ignore in preview
                if( isMatched != null )
                {
                    return false;
                }
            }

            if( string.IsNullOrEmpty( Filter ) )
            {
                return true;
            }

            return previewItem.Contains( Filter, StringComparison.OrdinalIgnoreCase );
        }

        public bool PreviewUnmatchedItemsOnly
        {
            get { return myPreviewUnmatchedItemsOnly; }
            set
            {
                if( SetProperty( ref myPreviewUnmatchedItemsOnly, value ) )
                {
                    PreviewItems.Refresh();
                }
            }
        }
    }
}
