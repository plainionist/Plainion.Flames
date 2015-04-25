
namespace Plainion.Flames.Presentation
{
    public enum ContentVisibility : byte
    {
        /// <summary>
        /// Explicit marked as "show"
        /// </summary>
        Visible = 0,

        /// <summary>
        /// Explicit marked as "hide"
        /// </summary>
        Invisible = 1,

        /// <summary>
        /// Implicitly marked as "hide" - e.g. due to VisibilityMask
        /// </summary>
        Hidden = 2
    }
}
