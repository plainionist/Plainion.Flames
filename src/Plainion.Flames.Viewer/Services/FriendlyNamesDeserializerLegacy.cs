using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plainion.Flames.Viewer.Model;

namespace Plainion.Flames.Viewer.Services
{
    /// <summary>
    /// Adds persistancy of friendly names to the project
    /// </summary>
    class FriendlyNamesDeserializerLegacy 
    {
        public IDictionary<long, string> Deserialize( Project project )
        {
            var mainTraceFile = project.TraceFiles.First();
            var file = Path.Combine( Path.GetDirectoryName( mainTraceFile ), Path.GetFileNameWithoutExtension( mainTraceFile ) + ".bffn" );

            if( !File.Exists( file ) )
            {
                return null;
            }

            var repository = new Dictionary<long, string>();

            using( var reader = new BinaryReader( new FileStream( file, FileMode.Open, FileAccess.Read ) ) )
            {
                var version = reader.ReadByte();

                Contract.Invariant( version == 1, "Invalid version" );

                while( reader.BaseStream.Position != reader.BaseStream.Length )
                {
                    repository[ reader.ReadInt64() ] = reader.ReadString();
                }
            }

            return repository;
        }
    }
}
