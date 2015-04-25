using System;
using System.Collections.Generic;

namespace Plainion.Flames.Infrastructure.Services
{
    public interface ITraceLoaderService
    {
        IEnumerable<string> LoadedTraceFiles { get;}

        event EventHandler LoadingCompleted;

        void ReloadCurrentTrace();
    }
}
