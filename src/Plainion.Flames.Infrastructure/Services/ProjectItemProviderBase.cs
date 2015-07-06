using Plainion.Flames.Infrastructure.Model;

namespace Plainion.Flames.Infrastructure.Services
{
    public class ProjectItemProviderBase : IProjectItemProvider
    {
        protected ProjectItemProviderBase() { }

        public virtual void OnProjectDeserialized(IProject project, IProjectSerializationContext context) { }

        public virtual void OnProjectSerializing(IProject project, IProjectSerializationContext context) { }
    }
}
