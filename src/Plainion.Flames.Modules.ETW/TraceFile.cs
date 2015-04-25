using System;
using System.IO;

namespace Plainion.Flames.Modules.ETW
{
    class TraceFile : IEquatable<TraceFile>
    {
        public TraceFile( string filename )
        {
            filename = Path.GetFullPath( filename );
            var baseFilename = Path.Combine( Path.GetDirectoryName( filename ), Path.GetFileNameWithoutExtension( filename ) );

            Etl = baseFilename + ".etl";
            Etlx = baseFilename + ".etlx";
        }

        public string Etl { get; private set; }
        
        public string Etlx { get; private set; }

        public bool EtlxExists { get { return File.Exists( Etlx ); } }

        public bool Equals( TraceFile other )
        {
            return Etl.Equals( other.Etl, StringComparison.OrdinalIgnoreCase );
        }
    }
}
