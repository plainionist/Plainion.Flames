using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plainion.Flames.Model;
using Plainion;

namespace Plainion.Flames.Modules.BFlames
{
    class Parser
    {
        private BinaryReader myReader;
        private int myVersion;
        private TraceModelBuilder myBuilder;

        public Parser( BinaryReader reader, TraceModelBuilder builder )
        {
            myReader = reader;
            myBuilder = builder;
        }

        public IEnumerable<Lazy<IAssociatedEventsSerializer>> AssociatedEventsSerializers { get; set; }

        public void Read()
        {
            myVersion = myReader.ReadByte();

            Contract.Invariant( myVersion >= 3, "Only BFlames format version >= 3 supported. Found version: " + myVersion );

            var namesPos = myReader.ReadInt64();

            myReader.BaseStream.Seek( namesPos, SeekOrigin.Begin );

            myBuilder.Symbols.Modules.Deserialize( myReader );
            myBuilder.Symbols.Namespaces.Deserialize( myReader );
            myBuilder.Symbols.Classes.Deserialize( myReader );
            myBuilder.Symbols.Methods.Deserialize( myReader );

            myReader.BaseStream.Seek( 1 + 8, SeekOrigin.Begin );

            myBuilder.SetCreationTime( new DateTime( myReader.ReadInt64() ) );
            myBuilder.SetTraceDuration( myReader.ReadInt64() );

            var count = myReader.ReadInt32();

            for( int i = 0; i < count; ++i )
            {
                var process = myBuilder.CreateProcess( myReader.ReadInt32() );
                var processName = myReader.ReadString();
                process.Name = !string.IsNullOrEmpty( processName ) ? processName : null;

                var threadCount = myReader.ReadInt32();

                for( int j = 0; j < threadCount; ++j )
                {
                    var thread = myBuilder.CreateThread( process, myReader.ReadInt32() );

                    var callsCount = myReader.ReadInt32();

                    for( int k = 0; k < callsCount; ++k )
                    {
                        var call = ReadCallstack( thread );
                        myBuilder.AddCallstackRoot( call );
                    }
                }
            }

            if( myVersion >= 4 )
            {
                ReadAssociatedEvents();
            }
        }

        private Call ReadCallstack( TraceThread trace )
        {
            var start = myReader.ReadInt64();
            var end = myReader.ReadInt64();

            var duration = myReader.ReadInt64();

            var method = myBuilder.CreateMethod(
                myBuilder.Symbols.Modules.Get( myReader.ReadInt32() ),
                myBuilder.Symbols.Namespaces.Get( myReader.ReadInt32() ),
                myBuilder.Symbols.Classes.Get( myReader.ReadInt32() ),
                myBuilder.Symbols.Methods.Get( myReader.ReadInt32() ) );

            var call = myBuilder.CreateCall( trace, start, method );
            call.SetEnd( end, duration );

            var childrenCount = myReader.ReadInt32();

            for( int i = 0; i < childrenCount; ++i )
            {
                var child = ReadCallstack( trace );
                call.AddChild( child );
            }

            return call;
        }

        private void ReadAssociatedEvents()
        {
            var eventTypesCount = myReader.ReadInt32();

            for( int i = 0; i < eventTypesCount; ++i )
            {
                var eventsType = myReader.ReadString();
                var serializer = AssociatedEventsSerializers.SingleOrDefault( s => s.Value.CanSerialize( eventsType ) );

                Contract.Invariant( serializer != null, "No serializer found for associated events of type: {0}", eventsType );

                var eventsOfTypeCount = myReader.ReadInt32();
                for( int k = 0; k < eventsOfTypeCount; ++k )
                {
                    var events = serializer.Value.Read( myReader );
                    myBuilder.AddAssociatedEvents( events );
                }
            }
        }
    }
}
