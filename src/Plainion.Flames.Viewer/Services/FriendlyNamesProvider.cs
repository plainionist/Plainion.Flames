using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Model;

namespace Plainion.Flames.Viewer.Services
{
    /// <summary>
    /// Adds persistancy of friendly names to the project
    /// </summary>
    class FriendlyNamesProvider : ProjectItemProviderBase
    {
        private const string ProviderId = "{866583EB-9C7C-4938-BDC8-FCCC77E42921}.FriendlyNames";

        [DataContract(Name = "FriendlyNames", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/FriendlyNames")]
        class FriendlyNames : IEnumerable<KeyValuePair<long, string>>
        {
            [DataMember(Name = "Version")]
            public const byte Version = 1;

            [DataMember(Name = "Names")]
            private Dictionary<long, string> myEntries;

            public FriendlyNames()
            {
                myEntries = new Dictionary<long, string>();
            }

            public string this[long key]
            {
                get
                {
                    string name;
                    return myEntries.TryGetValue(key, out name) ? name : null;
                }
                set
                {
                    myEntries[key] = value;
                }
            }

            public IEnumerator<KeyValuePair<long, string>> GetEnumerator()
            {
                return myEntries.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return myEntries.GetEnumerator();
            }
        }

        public override void OnTraceLogLoaded(IProject project, IProjectSerializationContext context)
        {
            var initialNames = new FriendlyNames();
            CollectInitialNames(project.TraceLog, initialNames);
            project.Items.Add(initialNames);

            if (context != null && context.HasEntry(ProviderId))
            {
                using (var stream = context.GetEntry(ProviderId))
                {
                    var serializer = new DataContractSerializer(typeof(FriendlyNames));
                    var friendlyNames = (FriendlyNames)serializer.ReadObject(stream);
                    ApplyFriendlyNames(project.TraceLog, friendlyNames);
                }
            }
            else
            {
                var legacyDeserializer = new FriendlyNamesDeserializerLegacy();
                var entries = legacyDeserializer.Deserialize(project);
                if (entries != null)
                {
                    ApplyFriendlyNames(project.TraceLog, entries);
                }
            }
        }

        private void ApplyFriendlyNames(ITraceLog traceLog, IEnumerable<KeyValuePair<long, string>> friendlyNames)
        {
            foreach (var entry in friendlyNames)
            {
                int pid;
                int tid;
                PidTid.Decode(entry.Key, out pid, out tid);

                var process = traceLog.Processes.SingleOrDefault(p => p.ProcessId == pid);
                if (process == null)
                {
                    // TODO: this should only happen if loading of trace has been aborted
                    continue;
                }

                if (tid == -1)
                {
                    process.Name = entry.Value;
                }
                else
                {
                    var thread = traceLog.GetThreads(process).Single(t => t.ThreadId == tid);
                    thread.Name = entry.Value;
                }
            }
        }

        /// <summary>
        /// Names of processes and threads can be changed directly in the model. Here we keep the initial 
        /// names so that we store only the user modified names on shutdown. This way we ensure that if we might later be
        /// able to detect proecess/thread names (better) the user can benefit from it automatically.
        /// </summary>
        private void CollectInitialNames(ITraceLog log, FriendlyNames repository)
        {
            foreach (var process in log.Processes)
            {
                long key = PidTid.Encode(process.ProcessId);

                repository[key] = process.Name;

                foreach (var thread in log.GetThreads(process))
                {
                    key = PidTid.Encode(process.ProcessId, thread.ThreadId);
                    repository[key] = thread.Name;
                }
            }
        }

        public override void OnProjectUnloading(IProject project, IProjectSerializationContext context)
        {
            if (project.TraceLog == null)
            {
                return;
            }

            var initialNames = project.Items.OfType<FriendlyNames>().Single();
            var friendlyNames = new FriendlyNames();

            foreach (var process in project.TraceLog.Processes)
            {
                var key = PidTid.Encode(process.ProcessId);

                var origName = initialNames[key];
                if (origName != process.Name)
                {
                    friendlyNames[key] = process.Name;
                }

                foreach (var thread in project.TraceLog.GetThreads(process))
                {
                    key = PidTid.Encode(process.ProcessId, thread.ThreadId);

                    origName = initialNames[key];
                    if (origName != thread.Name)
                    {
                        friendlyNames[key] = thread.Name;
                    }
                }
            }

            using (var stream = context.CreateEntry(ProviderId))
            {
                var serializer = new DataContractSerializer(typeof(FriendlyNames));
                serializer.WriteObject(stream, friendlyNames);
            }
        }
    }
}
