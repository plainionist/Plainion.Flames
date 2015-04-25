using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plainion.Flames.Model;

namespace Plainion.Flames.Modules.BFlames
{
    class BookmarksSerializer : IAssociatedEventsSerializer
    {
        public bool CanSerialize( string eventsTyp )
        {
            return eventsTyp == "Plainion.Flames.Model.Bookmarks";
        }

        public void Write( BinaryWriter writer, IAssociatedEvents events )
        {
            var bookmarks = ( Bookmarks )events;

            writer.Write( bookmarks.Name );

            writer.Write( bookmarks.ReferencedModel.ProcessId );
            writer.Write( bookmarks.ReferencedModel.ThreadId );

            writer.Write( bookmarks.Timestamps.Count );
            foreach( var v in bookmarks.Timestamps )
            {
                writer.Write( v );
            }
        }

        public IAssociatedEvents Read( BinaryReader reader )
        {
            var name = reader.ReadString();

            var modelRef = new ModelReference( reader.ReadInt32(), reader.ReadInt32() );

            var count = reader.ReadInt32();

            var bookmarks = new Bookmarks( modelRef, name, count );
            for( int i = 0; i < count; ++i )
            {
                bookmarks.Add( reader.ReadInt64() );
            }

            return bookmarks;
        }
    }
}
