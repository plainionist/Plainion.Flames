
namespace Plainion.Flames.Infrastructure
{
    public class ProjectItemProviderBase : IProjectItemProvider
    {
        protected ProjectItemProviderBase() { }

        public virtual void OnTraceLogLoading(IProject project, IProjectSerializationContext context) { }

        public virtual void OnTraceLogLoaded(IProject project, IProjectSerializationContext context) { }

        public virtual void OnProjectUnloading(IProject project, IProjectSerializationContext context) { }

        public virtual void OnProjectUnloaded(IProject project, IProjectSerializationContext context) { }
    }
}
