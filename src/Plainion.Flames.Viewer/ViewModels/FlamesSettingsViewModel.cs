using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Plainion.Flames.Infrastructure.Controls;
using Plainion.Flames.Presentation;
using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.Flames.Viewer.ViewModels
{
    class FlamesSettingsViewModel : BindableBase
    {
        private FlameSetPresentation myPresentation;
        private int myProcessCount;
        private int myThreadCount;
        private int myCallCount;
        private int mySelectedTabIndex;

        public FlamesSettingsViewModel()
        {
            TracesTreeSource = new TracesTree();
        }

        public TracesTree TracesTreeSource { get; private set; }

        public FlameSetPresentation Presentation
        {
            get { return myPresentation; }
            set
            {
                if( SetProperty( ref myPresentation, value ) )
                {
                    TracesTreeSource.Processes = myPresentation.Flames
                        .GroupBy( x => x.Model.Process )
                        .OrderBy( x => x.Key.Name )
                        .Select( x => new SelectableProcessAdapter( x.Key, x.AsEnumerable() ) )
                        .ToList();

                    ProcessCount = myPresentation.Flames
                        .Select( t => t.ProcessId )
                        .Distinct()
                        .Count();

                    ThreadCount = myPresentation.Flames.Count;

                    CallCount = myPresentation.Flames
                        .SelectMany( t => t.Activities )
                        .Count();

                    if( mySelectedTabContent != null )
                    {
                        InjectPresentation( mySelectedTabContent );
                    }
                }
            }
        }

        public TimeSpan TraceDuration
        {
            get
            {
                return myPresentation == null ? TimeSpan.MinValue : TimeSpan.FromMilliseconds( myPresentation.Model.TraceDuration / 1000 );
            }
        }

        public int ProcessCount
        {
            get { return myProcessCount; }
            set { SetProperty( ref myProcessCount, value ); }
        }

        public int ThreadCount
        {
            get { return myThreadCount; }
            set { SetProperty( ref myThreadCount, value ); }
        }

        public int CallCount
        {
            get { return myCallCount; }
            set { SetProperty( ref myCallCount, value ); }
        }

        public int SelectedTabIndex
        {
            get { return mySelectedTabIndex; }
            set { SetProperty( ref mySelectedTabIndex, value ); }
        }

        // TODO: workaround to inject presentations
        private object mySelectedTabContent;
        public object SelectedTabItem
        {
            set
            {
                var frameworkElement = value as FrameworkElement;
                if( frameworkElement == null || frameworkElement.DataContext == null )
                {
                    return;
                }

                mySelectedTabContent = frameworkElement.DataContext;

                InjectPresentation( mySelectedTabContent );
            }
        }

        private void InjectPresentation( object dataContext )
        {
            var presentationProperty = mySelectedTabContent.GetType().GetProperty( "Presentation" );
            if( presentationProperty == null )
            {
                return;
            }

            presentationProperty.SetValue( mySelectedTabContent, myPresentation );
        }
    }
}
