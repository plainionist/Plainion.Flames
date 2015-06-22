using System.ComponentModel.Composition;

namespace Plainion.Flames.Infrastructure
{
    [InheritedExport]
    public interface IProjectItemProvider
    {
        void OnTraceLogLoading(IProject project, IProjectSerializationContext context);
        void OnTraceLogLoaded(IProject project, IProjectSerializationContext context);

        void OnProjectUnloading(IProject project, IProjectSerializationContext context);
        void OnProjectUnloaded(IProject project, IProjectSerializationContext context);

        void OnPresentationCreated(IProject project);
    }
}
