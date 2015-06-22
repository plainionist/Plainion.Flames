using System.IO;
using System.IO.Compression;
using System.Linq;
using Plainion.Flames.Infrastructure;

namespace Plainion.Flames.Viewer.Services
{
    class ProjectSerializationContext : IProjectSerializationContext
    {
        private ZipArchive myArchive;

        public ProjectSerializationContext(ZipArchive archive)
        {
            myArchive = archive;
        }

        public Stream CreateEntry(string providerId)
        {
            return myArchive.CreateEntry(providerId, CompressionLevel.Fastest).Open();
        }

        public Stream GetEntry(string providerId)
        {
            return myArchive.Entries.Single(e => e.Name == providerId).Open();
        }

        public bool HasEntry(string providerId)
        {
            return myArchive.Entries.Any(e => e.Name == providerId);
        }
    }
}
