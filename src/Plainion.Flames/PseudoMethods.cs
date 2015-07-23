
namespace Plainion.Flames.Model
{
    /// <summary>
    /// Pseudo methods used to indicate certain category for the method calls in the call stack below that method.
    /// </summary>
    public static class PseudoMethods
    {
        public static Method BrokenCallstackMethod(this TraceModelBuilder self)
        {
            // we need to route it through the builder in order to have it fully integrated (e.g. Symbols cache gets updated)
            return self.CreateMethod("BROKEN", "BROKEN", "BROKEN", "BROKEN");
        }

        public static bool IsBrokenCallstack(this Method self)
        {
            return self.Module == "BROKEN" && self.Namespace == "BROKEN" && self.Class == "BROKEN" && self.Name == "BROKEN";
        }
    }
}
