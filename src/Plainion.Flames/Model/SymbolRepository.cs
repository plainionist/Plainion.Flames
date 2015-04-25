using System.Collections.Generic;

namespace Plainion.Flames.Model
{
    public class SymbolRepository
    {
        internal SymbolRepository()
        {
            Modules = new SymbolPool();
            Namespaces = new SymbolPool();
            Classes = new SymbolPool();
            Methods = new SymbolPool();
        }

        public SymbolPool Modules { get; private set; }
        public SymbolPool Namespaces { get; private set; }
        public SymbolPool Classes { get; private set; }
        public SymbolPool Methods { get; private set; }

        internal void Freeze()
        {
            Modules.Freeze();
            Namespaces.Freeze();
            Classes.Freeze();
            Methods.Freeze();
        }

        internal void Clear()
        {
            Modules.Clear();
            Namespaces.Clear();
            Classes.Clear();
            Methods.Clear();
        }
    }
}
