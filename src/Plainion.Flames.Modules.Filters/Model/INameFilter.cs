using System.ComponentModel;

namespace Plainion.Flames.Modules.Filters.Model
{
    interface INameFilter : ICallFilter, INotifyPropertyChanged
    {
        FilterTarget Target { get; }

        new bool IsShowFilter { get; set; }

        new bool IsApplied { get; set; }
    
        bool? IsVisible( FilterTarget target, string value );
    }
}
