using System;
using System.ComponentModel.Composition;
using Plainion.Flames.Infrastructure.Model;

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
