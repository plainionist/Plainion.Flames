using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure;

namespace Plainion.Flames.Viewer.Services
{
    /// <summary>
    /// Adds persistancy of selected processes and threads to the project
    /// </summary>
    class SelectedThreadsProvider : ProjectItemProviderBase
    {
        private const string ProviderId = "{6E3426EF-676D-48B1-AA5D-E9661A5C6CCD}.SelectedThreads";

        [DataContract(Name = "SelectedThreads", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/SelectedThreads")]
        class SelectedThreads
        {
            [DataMember(Name = "Version")]
            public const byte Version = 1;

            [DataMember(Name = "ProcessThreadTree")]
            private Dictionary<int, IList<int>> myEntries;

            public SelectedThreads()
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

        public override void OnTraceLogLoaded(IProject project, IProjectSerializationContext context)
        {
            if (context == null || !context.HasEntry(ProviderId))
            {
                return;
            }

            using (var stream = context.GetEntry(ProviderId))
            {
                var serializer = new DataContractSerializer(typeof(SelectedThreads));
                project.Items.Add((SelectedThreads)serializer.ReadObject(stream));
            }
        }

        public override void OnProjectUnloading(IProject project, IProjectSerializationContext context)
        {
            if (project.Presentation == null)
            {
                return;
            }

            var selectedThreads = new SelectedThreads();

            foreach (var flame in project.Presentation.Flames)
            {
                if (flame.Visibility != Presentation.ContentVisibility.Invisible)
                {
                    selectedThreads.Add(flame.ProcessId, flame.ThreadId);
                }
            }

            using (var stream = context.CreateEntry(ProviderId))
            {
                var serializer = new DataContractSerializer(typeof(SelectedThreads));
                serializer.WriteObject(stream, selectedThreads);
            }
        }

        public override void OnPresentationCreated(IProject project)
        {
            var selectedThreads = project.Items.OfType<SelectedThreads>().SingleOrDefault();
            if (selectedThreads == null)
            {
                return;
            }

            foreach (var flame in project.Presentation.Flames)
            {
                if (!selectedThreads.IsVisible(flame.ProcessId, flame.ThreadId))
                {
                    flame.Visibility = Presentation.ContentVisibility.Invisible;
                }
            }
        }
    }
}
