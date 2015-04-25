using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plainion.Progress;

namespace Plainion.Flames.Infrastructure
{
    public interface ITraceReader
    {
        IEnumerable<FileFilter> FileFilters { get; }
        Task ReadAsync( string filename, TraceModelBuilder builder, IProgress<IProgressInfo> progress );
    }
}
