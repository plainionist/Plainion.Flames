using System.Windows;

namespace Plainion.Flames.Viewer
{
    public partial class App : Application
    {
        protected override void OnStartup( StartupEventArgs e )
        {
            base.OnStartup( e );

            ShutdownMode = ShutdownMode.OnMainWindowClose;

            new Bootstrapper().Run();
        }
    }
}
