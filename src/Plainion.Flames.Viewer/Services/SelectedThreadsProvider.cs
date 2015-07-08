using System.Linq;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure.Model;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Viewer.Model;

namespace Plainion.Flames.Viewer.Services
{
    /// <summary>
    /// Adds persistancy of selected processes and threads to the project
    /// </summary>
    class SelectedThreadsProvider : ProjectItemProviderBase
    {
        private const string ProviderId = "{6E3426EF-676D-48B1-AA5D-E9661A5C6CCD}.SelectedThreads";

        public override void OnProjectDeserialized(IProject project, IProjectSerializationContext context)
        {
            if (!context.HasEntry(ProviderId))
            {
                return;
            }

            using (var stream = context.GetEntry(ProviderId))
            {
                var serializer = new DataContractSerializer(typeof(SelectedThreadsDocument));
                project.Items.Add((SelectedThreadsDocument)serializer.ReadObject(stream));
            }
        }

        public override void OnProjectSerializing(IProject project, IProjectSerializationContext context)
        {
            var document = project.Items.OfType<SelectedThreadsDocument>().SingleOrDefault();
            if (document == null)
            {
                return;
            }

            using (var stream = context.CreateEntry(ProviderId))
            {
                var serializer = new DataContractSerializer(typeof(SelectedThreadsDocument));
                serializer.WriteObject(stream, document);
            }
        }
    }
}
