using System;
using System.Collections.Generic;
using System.Diagnostics;
using Plainion.Flames.Model;
using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.Flames.Presentation
{
    public class TimelineViewport : BindableBase
    {
        private const int ScaleLinesDensity = 100;

        private TraceLog myTraceLog;
        private bool myShowAbsoluteTimestamps;

        public TimelineViewport( TraceLog traceLog )
        {
            myTraceLog = traceLog;

            Start = Min;
            End = Max;

            Width = End - Start;
        }

        public long Min { get { return 0; } }

        public long Max { get { return myTraceLog.TraceDuration; } }

        public long Start { get; private set; }

        public long End { get; private set; }

        public long Width { get; private set; }

        public void Move( int delta )
        {
            if( delta == 0 )
            {
                return;
            }

            var args = Changed != null ? new TimelineViewportChangedEventArgs( Start, End, Start + delta, End + delta ) : null;

            Start += delta;
            End += delta;

            if( Changed != null )
            {
                Changed( this, args );
            }
        }

        public void Move( double delta )
        {
            Move( ( int )delta );
        }

        public void Set( long start, long end )
        {
            if( start > end )
            {
                var tmp = start;
                end = start;
                start = tmp;
            }
            if( start == end )
            {
                end += 1;
            }

            if( start == Start && end == End )
            {
                return;
            }

            var args = Changed != null ? new TimelineViewportChangedEventArgs( Start, End, start, end ) : null;

            Start = start;
            End = end;
            Width = End - Start;

            if( Changed != null )
            {
                Changed( this, args );
            }
        }

        public event EventHandler<TimelineViewportChangedEventArgs> Changed;

        public IEnumerable<long> GetTimeScaleSteps( double contentWidth )
        {
            double timePerPixel = Width / contentWidth;

            long stepSize = ( long )( timePerPixel * ScaleLinesDensity );

            stepSize = Math.Max( 1, stepSize );

            long precision = 1;

            if( stepSize > Time.OneMinute ) precision = Time.OneMinute;
            else if( stepSize > Time.OneSecond ) precision = Time.OneSecond;
            else if( stepSize > Time.OneMSecond ) precision = Time.OneMSecond;
            else if( stepSize > Time.OneUSecond * 10 ) precision = Time.OneUSecond * 10;

            stepSize /= precision;
            stepSize *= precision;

            long scaleStart = Start - Start % stepSize;
            scaleStart += ( scaleStart % precision );

            for( long time = scaleStart; time < End; time += stepSize )
            {
                yield return time;
            }
        }

        public int CalculateX( double contentWidth, long time )
        {
            double relativeTime = time - Start;
            double x = relativeTime / ( double )Width * contentWidth;

            const double Tolerance = 10;
            if( x < -Tolerance )
            {
                x = -Tolerance;
            }
            else if( x > contentWidth + Tolerance )
            {
                x = contentWidth + Tolerance;
            }

            return ( int )x;
        }

        public long CalculateTime( double contentWidth, int x )
        {
            double time = Start + ( x / contentWidth * Width );
            return ( long )time;
        }

        public bool ShowAbsoluteTimestamps
        {
            get { return myShowAbsoluteTimestamps; }
            set { SetProperty( ref myShowAbsoluteTimestamps, value ); }
        }

        public string GetTimeString( long time )
        {
            if( ShowAbsoluteTimestamps )
            {
                time += ( long )( myTraceLog.CreationTime.TimeOfDay.TotalMilliseconds * 1000 );
            }

            long timeH = time / Time.OneHour;
            time -= timeH * Time.OneHour;
            long timeM = time / Time.OneMinute;
            time -= timeM * Time.OneMinute;
            long timeS = time / Time.OneSecond;
            time -= timeS * Time.OneSecond;
            long timeMS = time / Time.OneMSecond;

            if( Width < 10000 )
            {
                time -= timeMS * Time.OneMSecond;
                return string.Format( "{0:D2}:{1:D2}:{2:D2}.{3:D3}{4:D3}", timeH, timeM, timeS, timeMS, time );
            }
            else
            {
                return string.Format( "{0:D2}:{1:D2}:{2:D2}.{3:D3}", timeH, timeM, timeS, timeMS );
            }
        }
    }
}
