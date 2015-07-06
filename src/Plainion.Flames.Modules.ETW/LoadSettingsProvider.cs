using System.Linq;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Infrastructure.Model;
using Plainion.Flames.Infrastructure.Services;

namespace Plainion.Flames.Modules.ETW
{
    /// <summary>
    /// Gets user settings regarding ETW trace loading from project file.
    /// The TraceReader gets it there from TraceLogBuilder.ReaderContextHints
    /// </summary>
    class LoadSettingsProvider : ProjectItemProviderBase
    {
        private const string ProviderId = "{DE31E0F0-5068-4638-A343-731C108AA91B}.LoadSettings";

        public override void OnProjectDeserialized(IProject project, IProjectSerializationContext context)
        {
            if (!context.HasEntry(ProviderId))
            {
                return;
            }

            using (var stream = context.GetEntry(ProviderId))
            {
                var serializer = new DataContractSerializer(typeof(LoadSettings));
                project.Items.Add((LoadSettings)serializer.ReadObject(stream));
            }
        }

        public override void OnProjectSerializing(IProject project, IProjectSerializationContext context)
        {
            var settings = project.Items.OfType<LoadSettings>().SingleOrDefault();
            if (settings == null)
            {
                return;
            }

            using (var stream = context.CreateEntry(ProviderId))
            {
                var serializer = new DataContractSerializer(typeof(LoadSettings));
                serializer.WriteObject(stream, settings);
            }
        }
    }
}
