using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure;

namespace Plainion.Flames.Modules.Filters
{
    /// <summary>
    /// Adds persistancy of filters to the project
    /// </summary>
    class FiltersProvider : ProjectItemProviderBase
    {
        private const string ProviderId = "{FFBBE09A-C73A-43EC-AF77-8E146B078E01}.Filters";

        [DataContract(Name = "Filters", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/Filters")]
        class Filters
        {
            [DataMember(Name = "Version")]
            public const byte Version = 1;

            public Filters()
            {
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
                var serializer = new DataContractSerializer(typeof(Filters));
                project.Items.Add((Filters)serializer.ReadObject(stream));
            }
        }

        public override void OnProjectUnloading(IProject project, IProjectSerializationContext context)
        {
            if (project.Presentation == null)
            {
                return;
            }

            //using (var stream = context.CreateEntry(ProviderId))
            //{
            //    var serializer = new DataContractSerializer(typeof(Filters));
            //    serializer.WriteObject(stream, selectedThreads);
            //}
        }

        public override void OnPresentationCreated(IProject project)
        {
            var filters = project.Items.OfType<Filters>().SingleOrDefault();
            if (filters == null)
            {
                return;
            }

            //foreach (var flame in project.Presentation.Flames)
            //{
            //    if (!filters.IsVisible(flame.ProcessId, flame.ThreadId))
            //    {
            //        flame.Visibility = Presentation.ContentVisibility.Invisible;
            //    }
            //}
        }
    }
}
