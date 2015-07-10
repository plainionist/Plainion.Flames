using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure.Model;

namespace Plainion.Flames.Viewer.Model
{
    [Document("{6E3426EF-676D-48B1-AA5D-E9661A5C6CCD}.SelectedThreads")]
    [DataContract(Name = "SelectedThreads", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/SelectedThreads")]
    class SelectedThreadsDocument : DataContractDocumentBase<SelectedThreadsDocument>
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

        protected override void OnDeserialized(SelectedThreadsDocument document)
        {
            myEntries = document.myEntries;
        }
    }
}
