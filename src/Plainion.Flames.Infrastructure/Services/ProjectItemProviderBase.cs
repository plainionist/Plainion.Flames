using Plainion.Flames.Infrastructure.Model;

namespace Plainion.Flames.Infrastructure.Services
{
    public class ProjectItemProviderBase : IProjectItemProvider
    {
        protected ProjectItemProviderBase() { }

        public virtual void OnProjectLoaded(IProject project, IProjectSerializationContext context) { }

        public virtual void OnProjectUnloading(IProject project, IProjectSerializationContext context) { }
    }
}
