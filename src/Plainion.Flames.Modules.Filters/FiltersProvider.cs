using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Modules.Filters.Model;

namespace Plainion.Flames.Modules.Filters
{
    /// <summary>
    /// Adds persistancy of filters to the project
    /// </summary>
    class FiltersProvider : ProjectItemProviderBase
    {
        private const string ProviderId = "{FFBBE09A-C73A-43EC-AF77-8E146B078E01}.Filters";

        // TODO: actually we just need the eariest possible trigger that project was loaded
        // -> we use TraceLog loaded as workaround here
        public override void OnTraceLogLoaded(IProject project, IProjectSerializationContext context)
        {
            if (context == null || !context.HasEntry(ProviderId))
            {
                return;
            }

            //using (var stream = context.GetEntry(ProviderId))
            //{
            //    var serializer = new DataContractSerializer(typeof(FiltersDocument));
            //    project.Items.Add((FiltersDocument)serializer.ReadObject(stream));
            //}
        }

        public override void OnProjectUnloading(IProject project, IProjectSerializationContext context)
        {
            var callFilterModule = project.Items.OfType<CallFilterModule>().SingleOrDefault();
            if (callFilterModule == null)
            {
                return;
            }

            var document = new FiltersDocument();
            document.DurationFilter = callFilterModule.DurationFilter;
            document.NameFilters.AddRange(callFilterModule.NameFilters.Where(f => !(f is AllCallsFilter)));

            //using (var stream = context.CreateEntry(ProviderId))
            //{
            //    var serializer = new DataContractSerializer(typeof(FiltersDocument), GetKnownDataContractTypes());
            //    serializer.WriteObject(stream, document);
            //}
        }

        private IEnumerable<Type> GetKnownDataContractTypes()
        {
            return GetType().Assembly.GetTypes()
                .Where(type => !type.IsAbstract)
                .Where(type => type.GetCustomAttributes(typeof(DataContractAttribute), false).Any())
                .ToList();
        }
    }
}
