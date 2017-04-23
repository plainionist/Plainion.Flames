using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.Flames.Infrastructure.Controls
{
    public class EditableTreeNode : BindableBase
    {
        private bool myIsInEditMode;

        public EditableTreeNode()
        {
            EditNodeCommand = new DelegateCommand( () => IsInEditMode = true );
        }

        public bool IsInEditMode
        {
            get { return myIsInEditMode; }
            set { SetProperty( ref myIsInEditMode, value ); }
        }

        public ICommand EditNodeCommand { get; private set; }
    }
}
