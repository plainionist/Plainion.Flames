using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure.Model;
using Plainion.Flames.Modules.Filters.Model;

namespace Plainion.Flames.Modules.Filters
{
    [Document("{FFBBE09A-C73A-43EC-AF77-8E146B078E01}.Filters")]
    [DataContract(Name = "Filters", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/Filters")]
    class FiltersDocument : DataContractDocumentBase<FiltersDocument>
    {
        [DataMember(Name = "Version")]
        public const byte Version = 1;

        public FiltersDocument()
        {
            NameFilters = new List<INameFilter>();
        }

        [DataMember(Name = "DurationFilter")]
        public DurationFilter DurationFilter { get; set; }

        [DataMember(Name = "NameFilters")]
        public List<INameFilter> NameFilters { get; private set; }

        protected override void OnDeserialized(FiltersDocument document)
        {
            DurationFilter = document.DurationFilter;
            NameFilters = document.NameFilters;
        }

        protected override IEnumerable<System.Type> GetKnownTypes()
        {
            return GetType().Assembly.GetTypes()
                .Where(type => !type.IsAbstract)
                .Where(type => type.GetCustomAttributes(typeof(DataContractAttribute), false).Any())
                .ToList();
        }
    }
}
