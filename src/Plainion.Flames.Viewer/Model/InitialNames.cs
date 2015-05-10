using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plainion.Flames.Model;

namespace Plainion.Flames.Viewer.Model
{
    /// <summary>
    /// Names of processes and threads can be changed directly in the model. Here we keep the initial 
    /// names so that we store only the user modified names on shutdown. This way we ensure that if we might later be
    /// able to detect proecess/thread names (better) the user can benefit from it automatically.
    /// </summary>
    class InitialNames
    {
        private static byte Version = 1;

        private Dictionary<long, string> myInitialNames;

        public InitialNames( TraceLog log, string name, string location )
        {
            Contract.RequiresNotNull( log, "log" );
            Contract.RequiresNotNullNotEmpty( name, "name" );
            Contract.RequiresNotNullNotEmpty( location, "location" );

            TraceLog = log;
            Name = name;
            Location = location;

            myInitialNames = new Dictionary<long, string>();
            CollectInitialNames();
        }

        public TraceLog TraceLog { get; private set; }

        public string Name { get; private set; }

        public string Location { get; private set; }

        private void CollectInitialNames()
        {
            foreach( var process in TraceLog.Processes )
            {
                long key = ToKey( process.ProcessId );

                if( !string.IsNullOrEmpty( process.Name ) )
                {
                    myInitialNames[ key ] = process.Name;
                }

                foreach( var thread in TraceLog.GetThreads( process ) )
                {
                    if( !string.IsNullOrEmpty( thread.Name ) )
                    {
                        key = ToKey( process.ProcessId, thread.ThreadId );
                        myInitialNames[ key ] = thread.Name;
                    }
                }
            }
        }

        internal void Save()
        {
            var file = Path.Combine( Location, Name + ".bffn" );
            long pos = 0;

            using( var writer = new BinaryWriter( new FileStream( file, FileMode.OpenOrCreate, FileAccess.Write ) ) )
            {
                writer.Write( Version );

                long key;
                string origName = null;

                foreach( var process in TraceLog.Processes )
                {
                    key = ToKey( process.ProcessId );

                    myInitialNames.TryGetValue( key, out origName );
                    if( origName != process.Name )
                    {
                        writer.Write( key );
                        writer.Write( process.Name );
                    }

                    foreach( var thread in TraceLog.GetThreads( process ) )
                    {
                        key = ToKey( process.ProcessId, thread.ThreadId );
                        
                        myInitialNames.TryGetValue( key, out origName );
                        if( origName != thread.Name )
                        {
                            writer.Write( key );
                            writer.Write( thread.Name );
                        }
                    }
                }

                writer.Flush();
                pos = writer.BaseStream.Position;
            }

            if( pos == 0 )
            {
                File.Delete( file );
            }
        }

        private static long ToKey( int pid, int tid = -1 )
        {
            long b = tid;
            b = b << 32;
            b = b | ( uint )pid;
            return b;
        }

        private static void FromKey( long a, out int pid, out int tid )
        {
            pid = ( int )( a & uint.MaxValue );
            tid = ( int )( a >> 32 );
        }

        internal void Load()
        {
            var file = Path.Combine( Location, Name + ".bffn" );
            if( !File.Exists( file ) )
            {
                return;
            }

            using( var reader = new BinaryReader( new FileStream( file, FileMode.Open, FileAccess.Read ) ) )
            {
                var version = reader.ReadByte();

                Contract.Invariant( version == 1, "Invalid version" );

                while( reader.BaseStream.Position != reader.BaseStream.Length )
                {
                    long key = reader.ReadInt64();
                    var name = reader.ReadString();

                    int pid;
                    int tid;
                    FromKey( key, out pid, out tid );

                    var process = TraceLog.Processes.SingleOrDefault( p => p.ProcessId == pid );
                    if( process == null )
                    {
                        // TODO: this should only happen if loading of trace has been aborted
                        continue;
                    }

                    if( tid == -1 )
                    {
                        process.Name = name;
                    }
                    else
                    {
                        var thread = TraceLog.GetThreads( process ).Single( t => t.ThreadId == tid );
                        thread.Name = name;
                    }

                    myInitialNames[ key ] = name;
                }
            }
        }
    }
}
