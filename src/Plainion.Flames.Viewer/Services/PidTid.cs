
namespace Plainion.Flames.Viewer.Services
{
    static class PidTid
    {
        public static long Encode( int pid, int tid = -1 )
        {
            long b = tid;
            b = b << 32;
            b = b | ( uint )pid;
            return b;
        }

        public static void Decode( long value, out int pid, out int tid )
        {
            pid = ( int )( value & uint.MaxValue );
            tid = ( int )( value >> 32 );
        }
    }
}
