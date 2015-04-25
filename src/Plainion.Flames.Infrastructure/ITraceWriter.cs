using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plainion.Flames.Model;
using Plainion.Progress;

namespace Plainion.Flames.Infrastructure
{
    public interface ITraceWriter
    {
        IEnumerable<FileFilter> FileFilters { get; }
        Task WriteAsync( ITraceLog traceLog, string filename, IProgress<IProgressInfo> progress );
    }
}
