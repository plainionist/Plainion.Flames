using System;

namespace Plainion.Flames.Modules.Streams
{
    public class TraceInfo
    {
        public DateTime CreationTimestamp { get; set; }

        /// <summary>
        /// In micro seconds
        /// </summary>
        public long TraceDuration { get; set; }
    }
}
