using System;
using System.Globalization;
using System.IO;

namespace Plainion.Flames.Modules.Streams
{
    /// <summary>
    /// Sample trace parser implementation for the following formats:
    /// <pre>
    /// 12:49:06.417 11108/9232 #*[ entering  [namespace.class] [method] ~[instance id]~
    /// 12:49:06.417 11108/9232 #*] leaving   [namespace.class] [method] ~[instance id]~ -> duration=8 us
    /// </pre>
    /// </summary>
    public class SampleTraceParser : IStreamTraceParser
    {
        private static char[] Separator_Whitespaces = new char[] { ' ', '\t' };
        private static char[] Separator_Slash = new char[] { '/' };
        private static string[] Separator_Arrow = new string[] { "->" };
        private static string[] Separator_Duration = new string[] { "uration=", " " };
        private static char[] Separator_Tilde = new char[] { '~' };

        public void Process( Stream stream, IParserContext context )
        {
            DateTime? creationTime = null;
            // relative time from trace start in micro seconds
            long time = 0;
            var lastTimestamp = DateTime.MinValue;

            using( var reader = new StreamReader( stream ) )
            {
                while( !reader.EndOfStream )
                {
                    var line = reader.ReadLine();

                    var tokens = line.Split( Separator_Whitespaces, 5, StringSplitOptions.RemoveEmptyEntries );
                    if( tokens.Length != 5 || tokens[ 2 ][ 0 ] != '#' )
                    {
                        continue;
                    }

                    var timestamp = tokens[ 0 ].Length > 12
                        ? DateTime.ParseExact( tokens[ 0 ], "HH:mm:ss.ffffff", CultureInfo.InvariantCulture )
                        : DateTime.ParseExact( tokens[ 0 ], "HH:mm:ss.fff", CultureInfo.InvariantCulture );

                    if( creationTime == null )
                    {
                        creationTime = timestamp;
                        lastTimestamp = timestamp;
                    }

                    time = ( long )( ( timestamp - lastTimestamp ).TotalMilliseconds * 1000 );

                    var pidTid = tokens[ 1 ].Split( Separator_Slash );

                    var pid = Convert.ToInt32( pidTid[ 0 ] );
                    var tid = Convert.ToInt32( pidTid[ 1 ] );

                    var domainMethodInstance = tokens[ 4 ].Split( Separator_Arrow, 2, StringSplitOptions.RemoveEmptyEntries );
                    var comment = domainMethodInstance.Length > 1 ? domainMethodInstance[ 1 ].Trim() : string.Empty;

                    domainMethodInstance = domainMethodInstance[ 0 ].Split( Separator_Tilde, 3, StringSplitOptions.RemoveEmptyEntries );
                    domainMethodInstance = domainMethodInstance[ 0 ].Replace( ", ", ",_" ).Split( Separator_Whitespaces, 2, StringSplitOptions.RemoveEmptyEntries );

                    var domain = domainMethodInstance[ 0 ].Replace( ",_", ", " ).Trim();
                    var method = domainMethodInstance.Length > 1 ? domainMethodInstance[ 1 ].Trim() : string.Empty;

                    if( tokens[ 2 ] == "#*[" )
                    {
                        context.Emit( context.CreateEnteringLine( time, pid, tid, null, domain, null, method ) );
                    }
                    else if( tokens[ 2 ] == "#*]" )
                    {
                        var traceLine = context.CreateLeavingLine( time, pid, tid, null, domain, null, method );

                        domainMethodInstance = comment.Split( Separator_Duration, 3, StringSplitOptions.RemoveEmptyEntries );

                        if( domainMethodInstance.Length > 2 )
                        {
                            traceLine.Duration = Convert.ToInt64( domainMethodInstance[ 1 ] );

                            if( domainMethodInstance[ 2 ] == "ms" )
                            {
                                traceLine.Duration *= 1000;
                            }
                            else if( domainMethodInstance[ 2 ] == "s" )
                            {
                                traceLine.Duration = traceLine.Duration * 1000 * 1000;
                            }
                        }

                        context.Emit( traceLine );
                    }
                    else
                    {
                        // ignore the other lines e.g. info traces or warnings
                    }
                }
            }

            context.Emit( new TraceInfo
            {
                CreationTimestamp = creationTime.Value,
                TraceDuration = time
            } );
        }
    }
}
