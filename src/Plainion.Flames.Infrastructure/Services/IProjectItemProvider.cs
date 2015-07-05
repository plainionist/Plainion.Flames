using System.ComponentModel.Composition;
using Plainion.Flames.Infrastructure.Model;

namespace Plainion.Flames.Infrastructure.Services
{
    [InheritedExport]
    public interface IProjectItemProvider
    {
        void OnProjectLoaded(IProject project, IProjectSerializationContext context);

        void OnProjectUnloading(IProject project, IProjectSerializationContext context);
    }
}
