using System.IO;

namespace Plainion.Flames.Infrastructure.Services
{
    public interface IProjectSerializationContext 
    {
        bool HasEntry(string providerId);
        Stream CreateEntry(string providerId);
        Stream GetEntry(string providerId);
    }
}
