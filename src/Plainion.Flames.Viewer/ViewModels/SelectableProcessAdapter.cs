using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Infrastructure.Controls;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Viewer.ViewModels
{
    class SelectableProcessAdapter : TraceProcessNode
    {
        private TraceProcess myModel;

        public SelectableProcessAdapter( TraceProcess traceProcess, IEnumerable<Flame> flames )
        {
            myModel = traceProcess;

            ProcessId = traceProcess.ProcessId;
            Name = traceProcess.Name;

            Threads = flames.Select( f => new SelectableThreadAdapter( f ) );

            myModel.PropertyChanged += OnModelPropertyChanged;
        }

        private void OnModelPropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "Name" )
            {
                Name = myModel.Name;
            }
        }

        protected override void OnNameChanged()
        {
            myModel.Name = Name;
        }
    }
}
