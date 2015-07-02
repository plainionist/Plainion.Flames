using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Plainion.Flames.Viewer.Model
{
    [DataContract(Name = "SelectedThreads", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/SelectedThreads")]
    class SelectedThreadsDocument
    {
        [DataMember(Name = "Version")]
        public const byte Version = 1;

        [DataMember(Name = "ProcessThreadTree")]
        private Dictionary<int, IList<int>> myEntries;

        public SelectedThreadsDocument()
        {
            myEntries = new Dictionary<int, IList<int>>();
        }

        public void Add(int processId, int threadId)
        {
            IList<int> threads;
            if (!myEntries.TryGetValue(processId, out threads))
            {
                threads = new List<int>();
                myEntries.Add(processId, threads);
            }

            threads.Add(threadId);
        }

        public bool IsVisible(int processId, int threadId)
        {
            IList<int> threads;
            if (!myEntries.TryGetValue(processId, out threads))
            {
                return false;
            }

            return threads.Contains(threadId);
        }
    }
}
