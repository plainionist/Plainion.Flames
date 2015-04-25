using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Plainion.Flames.Model;
using Plainion;

namespace Plainion.Flames.Presentation
{
    public class BookmarksModule
    {
        public BookmarksModule( Flame presentation )
        {
            Contract.RequiresNotNull( presentation, "presentation" );

            Flame = presentation;

            Items = Flame.Model.Process.Log.AssociatedEvents
                .GetAllFor<IBookmarks>( Flame.Model )
                .OrderBy( b => b.Name )
                .ToList();

            SelectedItems = new ObservableCollection<IBookmarks>();
        }

        public Flame Flame { get; private set; }

        public IReadOnlyList<IBookmarks> Items { get; private set; }

        public ObservableCollection<IBookmarks> SelectedItems { get; private set; }
    }
}
