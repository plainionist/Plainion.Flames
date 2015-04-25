using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace Plainion.Flames.Modules.ETW
{
    class QueueTextWriter : TextWriter
    {
        private StringBuilder myLine;
        private char myLastChar;

        public QueueTextWriter()
        {
            Queue = new BlockingCollection<string>();
            myLine = new StringBuilder( 100 );
        }

        public override void Write( char value )
        {
            myLine.Append( value );

            if( myLastChar == '\r' && value == '\n' && myLine.Length > 0 )
            {
                Queue.Add( myLine.ToString() );
                myLine.Clear();
            }

            myLastChar = value;
        }

        public override void Close()
        {
            Queue.CompleteAdding();

            base.Close();
        }

        public BlockingCollection<string> Queue { get; private set; }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
}
