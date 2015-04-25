using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Model;
using Plainion;
using Plainion.Progress;

namespace Plainion.Flames.Modules.BFlames
{
    [Export( typeof( ITraceWriter ) )]
    [Export( typeof( BFlamesWriter ) )]
    class BFlamesWriter : ITraceWriter
    {
        public static byte Version = 4;

        public BFlamesWriter()
        {
            FileFilters = new[] { new FileFilter( ".bflames", "Binary flames (*.bflames)" ) };
        }

        public IEnumerable<FileFilter> FileFilters { get; private set; }

        [ImportMany]
        public IEnumerable<Lazy<IAssociatedEventsSerializer>> AssociatedEventsSerializers { get; set; }

        public Task WriteAsync( ITraceLog traceLog, string filename, IProgress<IProgressInfo> progress )
        {
            return Task.Run( () => Write( traceLog, filename ) );
        }

        private void Write( ITraceLog traceLog, string filename )
        {
            using( var writer = new BinaryWriter( new FileStream( filename, FileMode.OpenOrCreate, FileAccess.Write ) ) )
            {
                writer.Write( Version );

                writer.Write( ( long )-1 );

                writer.Write( traceLog.CreationTime.Ticks );
                writer.Write( traceLog.TraceDuration );

                writer.Write( traceLog.Processes.Count );

                foreach( var process in traceLog.Processes )
                {
                    writer.Write( process.ProcessId );
                    writer.Write( process.Name != null ? process.Name : string.Empty );

                    var threads = traceLog.GetThreads( process );
                    writer.Write( threads.Count );

                    foreach( var thread in threads )
                    {
                        writer.Write( thread.ThreadId );

                        var callstacks = traceLog.GetCallstacks( thread );

                        writer.Write( callstacks.Count );

                        foreach( var callstack in callstacks )
                        {
                            WriteCallstack( writer, callstack );
                        }
                    }
                }

                WriteAssociatedEvents( writer, traceLog.AssociatedEvents );

                writer.Flush();

                var pos = writer.BaseStream.Position;

                traceLog.Symbols.Modules.Serialize( writer );
                traceLog.Symbols.Namespaces.Serialize( writer );
                traceLog.Symbols.Classes.Serialize( writer );
                traceLog.Symbols.Methods.Serialize( writer );

                writer.Seek( 1, SeekOrigin.Begin );
                writer.Write( pos );
            }
        }

        private void WriteCallstack( BinaryWriter writer, Call callstack )
        {
            writer.Write( callstack.Start );
            writer.Write( callstack.End );
            writer.Write( callstack.Duration );

            writer.Write( callstack.Method.Module == null ? 0 : callstack.Method.Module.GetHashCode() );
            writer.Write( callstack.Method.Namespace == null ? 0 : callstack.Method.Namespace.GetHashCode() );
            writer.Write( callstack.Method.Class == null ? 0 : callstack.Method.Class.GetHashCode() );
            writer.Write( callstack.Method.Name.GetHashCode() );

            writer.Write( callstack.Children.Count );

            foreach( var child in callstack.Children )
            {
                WriteCallstack( writer, child );
            }
        }

        private void WriteAssociatedEvents( BinaryWriter writer, IAssociatedEventsCollection associatedEventsCollection )
        {
            var eventsByType = associatedEventsCollection.All<IAssociatedEvents>()
                .GroupBy( e => e.GetType().FullName )
                .ToList();

            writer.Write( eventsByType.Count );

            foreach( var g in eventsByType )
            {
                var serializer = AssociatedEventsSerializers.SingleOrDefault( s => s.Value.CanSerialize( g.Key ) );

                Contract.Invariant( serializer != null, "No serializer found for associated events of type: {0}", g.Key );

                writer.Write( g.Key );

                var eventsOfType = g.ToList();
                writer.Write( eventsOfType.Count );
                foreach( var events in eventsOfType )
                {
                    serializer.Value.Write( writer, events );
                }
            }
        }
    }
}
