using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using Plainion.Windows.Diagnostics;

namespace Plainion.Flames.Viewer
{
    [Export]
    public partial class Shell : Window
    {
        [ImportingConstructor]
        Shell( ShellViewModel model )
        {
            InitializeComponent();

            DataContext = model;

            if (Debugger.IsAttached)
            {
                new InspectionWindow().Show();
            }
        }
    }
}
