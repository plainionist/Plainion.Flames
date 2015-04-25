using Plainion.Flames.Model;

namespace Plainion.Flames.Presentation
{
    public class Activity
    {
        private int? myVisibleDepth;

        internal Activity( Flame flame, Call call )
        {
            Flame = flame;
            Model = call;

            Parent = null;
        }

        public Flame Flame { get; private set; }

        public Call Model { get; private set; }

        public string Name { get { return Model.Method.Name; } }

        public long StartTime { get { return Model.Start; } }

        public long EndTime { get { return Model.End; } }

        public long Duration { get { return Model.Duration; } }

        public string StartTimeString { get { return Flame.TimelineViewport.GetTimeString( StartTime ); } }

        public string EndTimeString { get { return Flame.TimelineViewport.GetTimeString( EndTime ); } }

        /// <summary>
        /// Used by the "application" to flag different visiblities directly on call.
        /// The renderer considers this call only if this mask is zero.
        /// </summary>
        public uint VisiblityMask { get; internal set; }

        public bool IsVisible
        {
            get { return VisiblityMask == 0; }
        }

        /// <summary>
        /// Defines the stack level considering own visibility and the one from parents.
        /// </summary>
        public int VisibleDepth
        {
            get
            {
                if( !myVisibleDepth.HasValue )
                {
                    var parentLevel = Parent != null ? Parent.VisibleDepth : -1;
                    var offset = IsVisible ? 1 : 0;

                    myVisibleDepth = parentLevel + offset;
                }

                return myVisibleDepth.Value;
            }
        }

        internal void ResetVisibleDepth()
        {
            myVisibleDepth = null;
        }

        public Activity Parent { get; internal set; }

        /// <summary>
        /// Hint for renderer. used to indicate that call is visible to the user after rendering.
        /// E.g. if too small in width or overlapped by child it will be set by false and so not drawn.
        /// 
        /// Attention: only up-to-date inside current TimelineViewport and after rendering has happened.
        /// </summary>
        internal bool IsRenderingRelevant { get; set; }

        public int X1 { get; set; }
        public int X2 { get; set; }

        public void CalculateYandHeight( out int y1, out int height )
        {
            y1 = 0;

            if( Flame.IsExpanded )
            {
                int heightPerLevel = ( int )( Flame.Height / ( Flame.MaxVisibleDepth + 1 ) );
                if( VisibleDepth > 0 )
                {
                    y1 += VisibleDepth * heightPerLevel;
                    height = heightPerLevel;
                }
                else
                {
                    height = heightPerLevel;
                }
            }
            else
            {
                int indent = VisibleDepth < Flame.MaxYIndentDepth ? VisibleDepth * 4 : Flame.MaxYIndentDepth * 4;

                y1 += indent / 2;
                height = Flame.Height - indent;
            }
        }
    }
}
