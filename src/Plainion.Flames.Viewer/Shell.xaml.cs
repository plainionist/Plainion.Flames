using System.ComponentModel.Composition;
using System.Windows;

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
        }
    }
}
