using Microsoft.Diagnostics.Tracing;

namespace Plainion.Flames.Modules.ETW
{
    interface IEventConsumer
    {
        void Consume( TraceEvent evt );

        void Complete();
    }
}
