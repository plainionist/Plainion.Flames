using System.Linq;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure.Model;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Viewer.Model;

namespace Plainion.Flames.Viewer.Services
{
    /// <summary>
    /// Adds persistancy of friendly names to the project
    /// </summary>
    class FriendlyNamesProvider : ProjectItemProviderBase
    {
        private const string ProviderId = "{866583EB-9C7C-4938-BDC8-FCCC77E42921}.FriendlyNames";

        public override void OnProjectDeserialized(IProject project, IProjectSerializationContext context)
        {
            if (!context.HasEntry(ProviderId))
            {
                return;
            }

            using (var stream = context.GetEntry(ProviderId))
            {
                var serializer = new DataContractSerializer(typeof(FriendlyNamesDocument));
                project.Items.Add(serializer.ReadObject(stream));
            }
        }

        public override void OnProjectSerializing(IProject project, IProjectSerializationContext context)
        {
            var document = project.Items.OfType<FriendlyNamesDocument>().SingleOrDefault();
            if (document == null)
            {
                return;
            }

            using (var stream = context.CreateEntry(ProviderId))
            {
                var serializer = new DataContractSerializer(typeof(FriendlyNamesDocument));
                serializer.WriteObject(stream, document);
            }
        }
    }
}
