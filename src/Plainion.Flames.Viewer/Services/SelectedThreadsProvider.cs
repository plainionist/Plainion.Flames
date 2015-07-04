using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Viewer.Model;

namespace Plainion.Flames.Viewer.Services
{
    /// <summary>
    /// Adds persistancy of selected processes and threads to the project
    /// </summary>
    class SelectedThreadsProvider : ProjectItemProviderBase
    {
        private const string ProviderId = "{6E3426EF-676D-48B1-AA5D-E9661A5C6CCD}.SelectedThreads";

        public override void OnProjectLoaded(IProject project, IProjectSerializationContext context)
        {
            if (context == null || !context.HasEntry(ProviderId))
            {
                return;
            }

            using (var stream = context.GetEntry(ProviderId))
            {
                var serializer = new DataContractSerializer(typeof(SelectedThreadsDocument));
                project.Items.Add((SelectedThreadsDocument)serializer.ReadObject(stream));
            }
        }

        public override void OnProjectUnloading(IProject project, IProjectSerializationContext context)
        {
            if (project.Presentation == null)
            {
                return;
            }

            var selectedThreads = new SelectedThreadsDocument();

            foreach (var flame in project.Presentation.Flames)
            {
                if (flame.Visibility != Presentation.ContentVisibility.Invisible)
                {
                    selectedThreads.Add(flame.ProcessId, flame.ThreadId);
                }
            }

            using (var stream = context.CreateEntry(ProviderId))
            {
                var serializer = new DataContractSerializer(typeof(SelectedThreadsDocument));
                serializer.WriteObject(stream, selectedThreads);
            }
        }
    }
}
