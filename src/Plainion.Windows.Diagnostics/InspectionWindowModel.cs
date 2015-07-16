using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.Windows.Diagnostics
{
    class InspectionWindowModel : BindableBase
    {
        public InspectionWindowModel()
        {
            RefreshCommand = new DelegateCommand(() => WpfStatics.CollectStatistics());
        }

        public ObservableCollection<DiagnosticFinding> Findings
        {
            get { return WpfStatics.Findings; }
        }

        public ICommand RefreshCommand { get; private set; }
    }
}
