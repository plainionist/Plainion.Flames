using System.Collections.Generic;
using System.Runtime.Serialization;
using Plainion.Flames.Modules.Filters.Model;

namespace Plainion.Flames.Modules.Filters
{
    [DataContract(Name = "Filters", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/Filters")]
    class FiltersDocument
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
    }
}
