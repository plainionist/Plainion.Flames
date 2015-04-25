using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows;
using Plainion.Logging;

namespace Plainion.Flames.Viewer.Services
{
    [Export]
    class LoggingSink : ILoggingSink
    {
        public LoggingSink()
        {
            Messages = new ObservableCollection<ILogEntry>();
        }

        public ObservableCollection<ILogEntry> Messages { get; private set; }

        public void Write( ILogEntry entry )
        {
            Application.Current.Dispatcher.Invoke( new Action( () => Messages.Add( entry ) ) );
        }
    }
}
