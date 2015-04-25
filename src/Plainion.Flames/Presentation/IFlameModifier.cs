using System;

namespace Plainion.Flames.Presentation
{
    public interface IFlameModifier : IDisposable
    {
        void SetVisibilityMask( Activity call, uint mask );

        bool HasVisibilityMaskChanged { get; }
    }
}
