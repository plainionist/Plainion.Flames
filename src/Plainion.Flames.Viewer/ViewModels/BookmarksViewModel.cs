using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;
using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.Flames.Viewer.ViewModels
{
    [Export]
    class BookmarksViewModel : BindableBase
    {
        private FlameSetPresentation myPresentation;

        public BookmarksViewModel()
        {
            SelectedItems = new ObservableCollection<string>();
        }

        private void OnSelectedItemsChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( myPresentation == null )
            {
                return;
            }

            if( e.OldItems != null )
            {
                var names = e.OldItems
                    .OfType<string>()
                    .ToList();

                foreach( var flame in myPresentation.Flames )
                {
                    var itemsToRemove = flame.Bookmarks.SelectedItems
                        .Where( i => names.Contains( i.Name ) )
                        .ToList();

                    foreach( var item in itemsToRemove )
                    {
                        flame.Bookmarks.SelectedItems.Remove( item );
                    }
                }
            }

            if( e.NewItems != null )
            {
                var names = e.NewItems
                    .OfType<string>()
                    .ToList();

                foreach( var flame in myPresentation.Flames )
                {
                    foreach( var item in flame.Bookmarks.Items.Where( i => names.Contains( i.Name ) ) )
                    {
                        flame.Bookmarks.SelectedItems.Add( item );
                    }
                }
            }
        }

        public string Description { get { return "Bookmarks"; } }

        public bool ShowTab { get { return true; } }

        public FlameSetPresentation Presentation
        {
            get { return myPresentation; }
            set
            {
                if( SetProperty( ref myPresentation, value ) )
                {
                    SelectedItems.CollectionChanged -= OnSelectedItemsChanged;
                    SelectedItems.Clear();
                    SelectedItems.CollectionChanged += OnSelectedItemsChanged;

                    Items = myPresentation.Model.AssociatedEvents
                        .All<IBookmarks>()
                        .Select( b => b.Name )
                        .Distinct()
                        .OrderBy( n => n )
                        .ToList();

                    OnPropertyChanged( "Items" );
                }
            }
        }

        public IEnumerable<string> Items { get; private set; }

        public ObservableCollection<string> SelectedItems { get; private set; }
    }
}
