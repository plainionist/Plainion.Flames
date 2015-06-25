using System;

namespace Plainion.Flames.Infrastructure.Services
{
    public interface IProjectService
    {
        IProject Project { get; }

        event EventHandler ProjectChanging;
        event EventHandler ProjectChanged;
    }
}
