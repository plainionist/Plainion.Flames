using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Controls
{
    public class FlameHeader : DependencyObject, INotifyPropertyChanged
    {
        private bool myIsInEditMode;

        public FlameHeader()
        {
            EditCommand = new DelegateCommand( () => IsInEditMode = true );
        }

        public ICommand EditCommand { get; private set; }

        public bool IsInEditMode
        {
            get { return myIsInEditMode; }
            set { SetProperty( ref myIsInEditMode, value ); }
        }

        public Flame Flame
        {
            get { return ( Flame )GetValue( FlameProperty ); }
            set { SetValue( FlameProperty, value ); }
        }

        public static DependencyProperty FlameProperty = DependencyProperty.Register( "Flame", typeof( Flame ), typeof( FlameHeader ),
             new FrameworkPropertyMetadata( null, OnFlameChanged ) );

        private static void OnFlameChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( ( FlameHeader )d ).OnFlameChanged( ( Flame )e.OldValue );
        }

        private void OnFlameChanged( Flame oldValue )
        {
            if( oldValue != null )
            {
                oldValue.PropertyChanged -= OnFlamePropertyChanged;
            }

            if( Flame != null )
            {
                Flame.PropertyChanged += OnFlamePropertyChanged;
            }

            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( "ProcessId" ) );
                PropertyChanged( this, new PropertyChangedEventArgs( "ThreadId" ) );
                PropertyChanged( this, new PropertyChangedEventArgs( "Name" ) );
            }
        }

        private void OnFlamePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, e );
            }
        }

        public int ProcessId { get { return Flame != null ? Flame.ProcessId : -1; } }

        public int ThreadId { get { return Flame != null ? Flame.ThreadId : -1; } }

        public string Name
        {
            get { return Flame != null ? Flame.Name : null; }
            set { if( Flame != null ) Flame.Name = value; }
        }

        private bool SetProperty<T>( ref T storage, T value, [CallerMemberName] string propertyName = null )
        {
            if( object.Equals( storage, value ) )
            {
                return false;
            }

            storage = value;

            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
