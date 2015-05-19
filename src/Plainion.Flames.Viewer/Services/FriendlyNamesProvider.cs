using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plainion.Flames.Model;
using Plainion.Flames.Viewer.Model;

namespace Plainion.Flames.Viewer.Services
{
    /// <summary>
    /// Adds persistancy of friendly names to the project
    /// </summary>
    class FriendlyNamesProvider : IProjectItemProvider
    {
        private static byte Version = 1;

        /// <summary>
        /// Names of processes and threads can be changed directly in the model. Here we keep the initial 
        /// names so that we store only the user modified names on shutdown. This way we ensure that if we might later be
        /// able to detect proecess/thread names (better) the user can benefit from it automatically.
        /// </summary>
        class InitialNames
        {
            private Dictionary<long, string> myInitialNames;

            public InitialNames()
            {
                myInitialNames = new Dictionary<long, string>();
            }

            public void CollectInitialNames( TraceLog log )
            {
                foreach( var process in log.Processes )
                {
                    long key = ToKey( process.ProcessId );

                    if( !string.IsNullOrEmpty( process.Name ) )
                    {
                        myInitialNames[ key ] = process.Name;
                    }

                    foreach( var thread in log.GetThreads( process ) )
                    {
                        if( !string.IsNullOrEmpty( thread.Name ) )
                        {
                            key = ToKey( process.ProcessId, thread.ThreadId );
                            myInitialNames[ key ] = thread.Name;
                        }
                    }
                }
            }

            public string this[ long key ]
            {
                get
                {
                    string name;
                    return myInitialNames.TryGetValue( key, out name ) ? name : null;
                }
                set
                {
                    myInitialNames[ key ] = value;
                }
            }

            public static long ToKey( int pid, int tid = -1 )
            {
                long b = tid;
                b = b << 32;
                b = b | ( uint )pid;
                return b;
            }

            public static void FromKey( long a, out int pid, out int tid )
            {
                pid = ( int )( a & uint.MaxValue );
                tid = ( int )( a >> 32 );
            }
        }

        public void OnTraceLogLoaded( Project project )
        {
            var repository = new InitialNames();
            repository.CollectInitialNames( project.TraceLog );

            LoadIfExists( project, repository );

            project.Items.Add( repository );
        }

        private void LoadIfExists( Project project, InitialNames repository )
        {
            var mainTraceFile = project.TraceFiles.First();
            var file = Path.Combine( Path.GetDirectoryName( mainTraceFile ), Path.GetFileNameWithoutExtension( mainTraceFile ) + ".bffn" );

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
                    InitialNames.FromKey( key, out pid, out tid );

                    var process = project.TraceLog.Processes.SingleOrDefault( p => p.ProcessId == pid );
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
                        var thread = project.TraceLog.GetThreads( process ).Single( t => t.ThreadId == tid );
                        thread.Name = name;
                    }

                    repository[ key ] = name;
                }
            }
        }

        public void OnProjectUnloading( Project project )
        {
            if( project.TraceLog == null )
            {
                return;
            }
            
            Save( project );
        }

        private void Save( Project project )
        {
            long pos = 0;

            var mainTraceFile = project.TraceFiles.First();
            var file = Path.Combine( Path.GetDirectoryName( mainTraceFile ), Path.GetFileNameWithoutExtension( mainTraceFile ) + ".bffn" );

            var repository = project.Items.OfType<InitialNames>().Single();

            using( var writer = new BinaryWriter( new FileStream( file, FileMode.OpenOrCreate, FileAccess.Write ) ) )
            {
                writer.Write( Version );

                long key;
                string origName = null;

                foreach( var process in project.TraceLog.Processes )
                {
                    key = InitialNames.ToKey( process.ProcessId );

                    origName = repository[ key ];
                    if( origName != process.Name )
                    {
                        writer.Write( key );
                        writer.Write( process.Name );
                    }

                    foreach( var thread in project.TraceLog.GetThreads( process ) )
                    {
                        key = InitialNames.ToKey( process.ProcessId, thread.ThreadId );

                        origName = repository[ key ];
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
    }
}
