using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Plainion.Flames.Model;
using Plainion.Flames.Viewer.Model;

namespace Plainion.Flames.Viewer.Services
{
    /// <summary>
    /// Adds persistancy of friendly names to the project
    /// </summary>
    class FriendlyNamesProvider : IProjectItemProvider
    {
        private const string ProviderId = "{866583EB-9C7C-4938-BDC8-FCCC77E42921}.FriendlyNames";

        /// <summary>
        /// Names of processes and threads can be changed directly in the model. Here we keep the initial 
        /// names so that we store only the user modified names on shutdown. This way we ensure that if we might later be
        /// able to detect proecess/thread names (better) the user can benefit from it automatically.
        /// </summary>
        [DataContract( Name = "InitialNames", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/FriendlyNames" )]
        class InitialNames : IEnumerable<KeyValuePair<long, string>>
        {
            [DataMember( Name = "Version" )]
            public const byte Version = 1;

            [DataMember( Name = "Names" )]
            private Dictionary<long, string> myInitialNames;

            public InitialNames()
            {
                myInitialNames = new Dictionary<long, string>();
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

            public IEnumerator<KeyValuePair<long, string>> GetEnumerator()
            {
                return myInitialNames.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return myInitialNames.GetEnumerator();
            }
        }

        public void OnTraceLogLoaded( Project project, IProjectSerializationContext context )
        {
            InitialNames repository = null;

            if( context != null )
            {
                using( var stream = context.GetEntry( ProviderId ) )
                {
                    var serializer = new DataContractSerializer( typeof( InitialNames ) );
                    repository = ( InitialNames )serializer.ReadObject( stream );
                    ApplyInitialNames( project, repository );
                }
            }

            if( repository == null )
            {
                repository = new InitialNames();

                var legacyDeserializer = new FriendlyNamesDeserializerLegacy();
                var entries = legacyDeserializer.Deserialize( project );
                if( entries != null )
                {
                    foreach( var entry in entries )
                    {
                        repository[ entry.Key ] = entry.Value;
                    }

                    ApplyInitialNames( project, repository );
                }
                else
                {
                    CollectInitialNames( project.TraceLog, repository );
                }
            }

            project.Items.Add( repository );
        }

        private void ApplyInitialNames( Project project, InitialNames repository )
        {
            foreach( var entry in repository )
            {
                int pid;
                int tid;
                PidTid.Decode( entry.Key, out pid, out tid );

                var process = project.TraceLog.Processes.SingleOrDefault( p => p.ProcessId == pid );
                if( process == null )
                {
                    // TODO: this should only happen if loading of trace has been aborted
                    continue;
                }

                if( tid == -1 )
                {
                    process.Name = entry.Value;
                }
                else
                {
                    var thread = project.TraceLog.GetThreads( process ).Single( t => t.ThreadId == tid );
                    thread.Name = entry.Value;
                }
            }
        }

        private void CollectInitialNames( TraceLog log, InitialNames repository )
        {
            foreach( var process in log.Processes )
            {
                long key = PidTid.Encode( process.ProcessId );

                if( !string.IsNullOrEmpty( process.Name ) )
                {
                    repository[ key ] = process.Name;
                }

                foreach( var thread in log.GetThreads( process ) )
                {
                    if( !string.IsNullOrEmpty( thread.Name ) )
                    {
                        key = PidTid.Encode( process.ProcessId, thread.ThreadId );
                        repository[ key ] = thread.Name;
                    }
                }
            }
        }

        public void OnProjectUnloading( Project project, IProjectSerializationContext context )
        {
            if( project.TraceLog == null )
            {
                return;
            }

            using( var stream = context.CreateEntry( ProviderId ) )
            {
                var serializer = new DataContractSerializer( typeof( InitialNames ) );
                serializer.WriteObject( stream, project.Items.OfType<InitialNames>().Single() );
            }
        }
    }
}
