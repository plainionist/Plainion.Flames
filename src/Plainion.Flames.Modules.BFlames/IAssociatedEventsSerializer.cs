using System;
using System.ComponentModel.Composition;
using System.IO;
using Plainion.Flames.Model;

namespace Plainion.Flames.Modules.BFlames
{
    [InheritedExport]
    public interface IAssociatedEventsSerializer
    {
        bool CanSerialize( string eventsTyp );

        void Write( BinaryWriter writer, IAssociatedEvents events );

        IAssociatedEvents Read( BinaryReader reader );
    }
}
