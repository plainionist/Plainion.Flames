using System.Windows;

namespace Plainion.Windows.Diagnostics
{
    public partial class InspectionWindow : Window
    {
        public InspectionWindow()
        {
            InitializeComponent();

            DataContext = new InspectionWindowModel();
        }
    }
}
