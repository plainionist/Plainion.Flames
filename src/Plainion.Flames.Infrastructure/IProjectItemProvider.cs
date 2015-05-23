
namespace Plainion.Flames.Infrastructure
{
    public interface IProjectItemProvider
    {
        void OnTraceLogLoaded( IProject project, IProjectSerializationContext context );
        void OnProjectUnloading( IProject project, IProjectSerializationContext context );
    }
}
