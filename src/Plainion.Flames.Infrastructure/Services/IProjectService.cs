using System;
using System.ComponentModel.Composition;

namespace Plainion.Flames.Infrastructure.Services
{
    [InheritedExport]
    public interface IProjectService
    {
        IProject Project { get; }

        event EventHandler ProjectChanging;
        event EventHandler ProjectChanged;
    }
}
