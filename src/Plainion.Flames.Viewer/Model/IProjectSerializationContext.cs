using System.IO;

namespace Plainion.Flames.Viewer.Model
{
    interface IProjectSerializationContext 
    {
        Stream CreateEntry( string providerId );
        Stream GetEntry( string providerId );
    }
}
