using System.ComponentModel.Composition;
using Plainion.Flames.Model;

namespace Plainion.Flames.Viewer.Model
{
    /// <summary>
    /// TODO: just a starting point - do we want to continue keeping entity and persistency separated?
    /// con: it requires a different service which calls the extensions when dealing with a project AND if 
    /// s.o. else is modifying the project the extensions will not notice that
    /// </summary>
    [InheritedExport]
    interface IProjectItemProvider
    {
        //void OnProjectCreated( Project project );
        void OnTraceLogLoaded( Project project, IProjectSerializationContext context );
        void OnProjectUnloading( Project project, IProjectSerializationContext context );
    }
}
