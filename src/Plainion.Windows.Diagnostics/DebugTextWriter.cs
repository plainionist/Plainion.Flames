using System.Diagnostics;
using System.IO;
using System.Text;

namespace Plainion.Windows.Diagnostics
{
    public class DebugTextWriter : TextWriter
    {
        public override void Write(char value)
        {
            Debug.Write(value);
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
}
