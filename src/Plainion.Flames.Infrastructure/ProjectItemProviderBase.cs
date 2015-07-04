
namespace Plainion.Flames.Infrastructure
{
    public class ProjectItemProviderBase : IProjectItemProvider
    {
        protected ProjectItemProviderBase() { }

        public virtual void OnProjectLoaded(IProject project, IProjectSerializationContext context) { }

        public virtual void OnProjectUnloading(IProject project, IProjectSerializationContext context) { }
    }
}
