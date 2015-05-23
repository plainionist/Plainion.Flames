using System.IO;

namespace Plainion.Flames.Infrastructure
{
    public interface IProjectSerializationContext 
    {
        Stream CreateEntry( string providerId );
        Stream GetEntry( string providerId );
    }
}
