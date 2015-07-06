using System.ComponentModel.Composition;
using Plainion.Flames.Infrastructure.Model;

namespace Plainion.Flames.Infrastructure.Services
{
    [InheritedExport]
    public interface IProjectItemProvider
    {
        void OnProjectDeserialized(IProject project, IProjectSerializationContext context);

        void OnProjectSerializing(IProject project, IProjectSerializationContext context);
    }
}
