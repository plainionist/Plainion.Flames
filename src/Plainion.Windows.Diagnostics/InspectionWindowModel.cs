using System.IO;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.Windows.Diagnostics
{
    class InspectionWindowModel : BindableBase
    {
        private StringWriter myLog;

        public InspectionWindowModel()
        {
            RefreshCommand = new DelegateCommand(OnRefresh);

            myLog = new StringWriter();
            MemoryLeakUtils.Writer = myLog;
        }

        public string Log
        {
            get { return myLog.ToString(); }
        }

        public ICommand RefreshCommand { get; private set; }

        private void OnRefresh()
        {
            MemoryLeakUtils.GenerateLeakStats();
            OnPropertyChanged("Log");
        }
    }
}
