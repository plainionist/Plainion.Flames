using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Plainion.Flames.Modules.Filters.Model
{
    /// <summary>
    /// Supports INotifyPropertyChanged for model entities.
    /// </summary>
    /// <remarks>
    /// BindableBase from Prism cannot be used with DataContractSerializer because it does not
    /// have DataContractAttribute applied which is mandatory
    /// </remarks>
    [DataContract]
    abstract class SerializableBindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>( ref T storage, T value, [CallerMemberName] string propertyName = null )
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
    }
}
