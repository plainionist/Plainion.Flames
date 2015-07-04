using System.ComponentModel.Composition;

namespace Plainion.Flames.Infrastructure
{
    [InheritedExport]
    public interface IProjectItemProvider
    {
        void OnProjectLoaded(IProject project, IProjectSerializationContext context);

        void OnProjectUnloading(IProject project, IProjectSerializationContext context);
    }
}
