using System.Collections.Generic;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure.Model;

namespace Plainion.Flames.Viewer.Model
{
    [Document("{866583EB-9C7C-4938-BDC8-FCCC77E42921}.FriendlyNames")]
    [DataContract( Name = "FriendlyNames", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/FriendlyNames" )]
    class FriendlyNamesDocument : DataContractDocumentBase<FriendlyNamesDocument>
    {
        [DataMember( Name = "Version" )]
        public const byte Version = 1;

        [DataMember( Name = "Names" )]
        private Dictionary<long, string> myEntries;

        public FriendlyNamesDocument()
        {
            myEntries = new Dictionary<long, string>();
        }

        public void Add( int pid, string name )
        {
            myEntries[ Encode( pid ) ] = name;
        }

        public void Add( int pid, int tid, string name )
        {
            myEntries[ Encode( pid, tid ) ] = name;
        }

        private static long Encode( int pid, int tid = -1 )
        {
            long b = tid;
            b = b << 32;
            b = b | ( uint )pid;
            return b;
        }

        private static void Decode( long value, out int pid, out int tid )
        {
            pid = ( int )( value & uint.MaxValue );
            tid = ( int )( value >> 32 );
        }

        public bool TryGetName( int pid, out string name )
        {
            var key = Encode( pid, -1 );
            return myEntries.TryGetValue( key, out name );
        }

        public bool TryGetName( int pid, int tid, out string name )
        {
            var key = Encode( pid, tid );
            return myEntries.TryGetValue( key, out name );
        }

        protected override void OnDeserialized(FriendlyNamesDocument document)
        {
            myEntries = document.myEntries;
        }
    }
}
