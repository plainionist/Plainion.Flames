using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Plainion.Flames.Viewer.Model
{
    [DataContract( Name = "FriendlyNames", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/FriendlyNames" )]
    class FriendlyNamesDocument : IEnumerable<KeyValuePair<long, string>>
    {
        [DataMember( Name = "Version" )]
        public const byte Version = 1;

        [DataMember( Name = "Names" )]
        private Dictionary<long, string> myEntries;

        public FriendlyNamesDocument()
        {
            myEntries = new Dictionary<long, string>();
        }

        public string this[ long key ]
        {
            get
            {
                string name;
                return myEntries.TryGetValue( key, out name ) ? name : null;
            }
            set
            {
                myEntries[ key ] = value;
            }
        }

        public IEnumerator<KeyValuePair<long, string>> GetEnumerator()
        {
            return myEntries.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return myEntries.GetEnumerator();
        }
    }
}
