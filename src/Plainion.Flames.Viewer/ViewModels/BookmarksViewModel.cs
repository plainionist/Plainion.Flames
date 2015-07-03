using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Infrastructure.ViewModels;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Viewer.ViewModels
{
    [Export]
    class BookmarksViewModel : ViewModelBase
    {
        public BookmarksViewModel()
        {
            SelectedItems = new ObservableCollection<string>();
        }

        private void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Presentation == null)
            {
                return;
            }

            if (e.OldItems != null)
            {
                var names = e.OldItems
                    .OfType<string>()
                    .ToList();

                foreach (var flame in Presentation.Flames)
                {
                    var itemsToRemove = flame.Bookmarks.SelectedItems
                        .Where(i => names.Contains(i.Name))
                        .ToList();

                    foreach (var item in itemsToRemove)
                    {
                        flame.Bookmarks.SelectedItems.Remove(item);
                    }
                }
            }

            if (e.NewItems != null)
            {
                var names = e.NewItems
                    .OfType<string>()
                    .ToList();

                foreach (var flame in Presentation.Flames)
                {
                    foreach (var item in flame.Bookmarks.Items.Where(i => names.Contains(i.Name)))
                    {
                        flame.Bookmarks.SelectedItems.Add(item);
                    }
                }
            }
        }

        public string Description { get { return "Bookmarks"; } }

        public bool ShowTab { get { return true; } }

        protected override void OnPresentationChanged(FlameSetPresentation oldValue)
        {
            SelectedItems.CollectionChanged -= OnSelectedItemsChanged;
            SelectedItems.Clear();
            SelectedItems.CollectionChanged += OnSelectedItemsChanged;

            if (Presentation != null)
            {
                Items = Presentation.Model.AssociatedEvents
                    .All<IBookmarks>()
                    .Select(b => b.Name)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();
            }
            else
            {
                Items = null;
            }

            OnPropertyChanged("Items");
        }

        public IEnumerable<string> Items { get; private set; }

        public ObservableCollection<string> SelectedItems { get; private set; }
    }
}
