using System.Collections.Generic;

namespace Plainion.Flames.Model
{
    public class Bookmarks : IBookmarks
    {
        private List<long> myTimestamps;

        public Bookmarks( ModelReference modelRef, string name )
            : this( modelRef, name, 0 )
        {
        }

        public Bookmarks( ModelReference modelRef, string name, int capacity )
        {
            ReferencedModel = modelRef;
            Name = name;

            myTimestamps = new List<long>( capacity );
        }

        public ModelReference ReferencedModel { get; private set; }

        public string Name { get; private set; }

        public IReadOnlyCollection<long> Timestamps { get { return myTimestamps; } }

        public void Add( long timestamp )
        {
            myTimestamps.Add( timestamp );
        }
    }
}
