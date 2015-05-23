
namespace Plainion.Flames.Infrastructure
{
    public interface IProjectItemProvider
    {
        void OnTraceLogLoading( IProject project, IProjectSerializationContext context );
        void OnTraceLogLoaded( IProject project, IProjectSerializationContext context );

        void OnProjectUnloading( IProject project, IProjectSerializationContext context );
        void OnProjectUnloaded( IProject project, IProjectSerializationContext context );
    }
}
