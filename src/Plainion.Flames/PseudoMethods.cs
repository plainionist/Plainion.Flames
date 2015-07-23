
namespace Plainion.Flames.Model
{
    /// <summary>
    /// Pseudo methods used to indicate certain category for the method calls in the call stack below that method.
    /// </summary>
    public static class PseudoMethods
    {
        private const string Broken = "BROKEN";

        public static Method BrokenCallstackMethod(this TraceModelBuilder self)
        {
            // we need to route it through the builder in order to have it fully integrated (e.g. Symbols cache gets updated)
            return self.CreateMethod(Broken, Broken, Broken, Broken);
        }

        public static bool IsBrokenCallstack(this Method self)
        {
            // ATTENTION: take care of backward compatibility here if u change the strings
            return self.Module == Broken && self.Namespace == Broken && self.Class == Broken && self.Name == Broken;
        }
    }
}
