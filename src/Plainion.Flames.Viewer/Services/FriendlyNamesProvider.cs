using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Model;
using Plainion.Flames.Viewer.Model;

namespace Plainion.Flames.Viewer.Services
{
    /// <summary>
    /// Adds persistancy of friendly names to the project
    /// </summary>
    class FriendlyNamesProvider : ProjectItemProviderBase
    {
        private const string ProviderId = "{866583EB-9C7C-4938-BDC8-FCCC77E42921}.FriendlyNames";

        public override void OnProjectLoaded( IProject project, IProjectSerializationContext context )
        {
            if( context != null && context.HasEntry( ProviderId ) )
            {
                using( var stream = context.GetEntry( ProviderId ) )
                {
                    var serializer = new DataContractSerializer( typeof( FriendlyNamesDocument ) );
                    var friendlyNames = ( FriendlyNamesDocument )serializer.ReadObject( stream );
                    project.Items.Add( friendlyNames );
                }
            }
        }

        public override void OnProjectUnloading( IProject project, IProjectSerializationContext context )
        {
            if( project.TraceLog == null )
            {
                return;
            }

            var initialNames = project.Items.OfType<FriendlyNamesDocument>().Single();
            var friendlyNames = new FriendlyNamesDocument();

            // only save what was really changed!
            string origName = null;
            foreach( var process in project.TraceLog.Processes )
            {
                if( initialNames.TryGetName( process.ProcessId, out origName ) && origName != process.Name )
                {
                    friendlyNames.Add( process.ProcessId, process.Name );
                }

                foreach( var thread in project.TraceLog.GetThreads( process ) )
                {
                    if( initialNames.TryGetName( process.ProcessId, thread.ThreadId, out origName ) && origName != thread.Name )
                    {
                        friendlyNames.Add( process.ProcessId, thread.ThreadId, thread.Name );
                    }
                }
            }

            using( var stream = context.CreateEntry( ProviderId ) )
            {
                var serializer = new DataContractSerializer( typeof( FriendlyNamesDocument ) );
                serializer.WriteObject( stream, friendlyNames );
            }
        }
    }
}
