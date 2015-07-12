using System.ComponentModel;
using Plainion.Flames.Infrastructure.Controls;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Viewer.ViewModels
{
    class SelectableThreadAdapter : TraceThreadNode
    {
        private Flame myFlame;

        public SelectableThreadAdapter( Flame flame )
        {
            myFlame = flame;

            ThreadId = myFlame.ThreadId;
            Name = myFlame.Model.Name;
            IsVisible = myFlame.Visibility == ContentVisibility.Visible;

            myFlame.PropertyChanged += OnFlamePropertyChanged;
            myFlame.Model.PropertyChanged += OnModelPropertyChanged;
        }

        private void OnModelPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "Name" )
            {
                Name = myFlame.Model.Name;
            }
        }

        private void OnFlamePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "Visibility" && myFlame.Visibility != ContentVisibility.Hidden )
            {
                // we want to handle the "hide" from context menu here
                // do NOT change the visibility here if the trace was just "hidden" and not visibilty changed explictly
                IsVisible = myFlame.Visibility == ContentVisibility.Visible;
            }
        }

        protected override void OnIsVisibleChanged()
        {
            myFlame.Visibility = IsVisible ? ContentVisibility.Visible : ContentVisibility.Invisible;
        }

        protected override void OnNameChanged()
        {
            myFlame.Model.Name = Name;
        }
    }
}
