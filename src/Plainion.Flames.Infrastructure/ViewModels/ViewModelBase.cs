using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Infrastructure.ViewModels
{
    public class ViewModelBase : BindableBase
    {
        private ITraceLog myTraceLog;
        private FlameSetPresentation myPresentation;
        private IProjectService myProjectService;

        // we dont go for ctor injection here by intention because then
        // we would might have the need to call virtual methods from ctor (e.g. OnProjectChanged).
        // this would cause method calls on derived classes for which the ctor didnt completed yet
        [Import]
        protected IProjectService ProjectService
        {
            get { return myProjectService; }
            set
            {
                if( myProjectService == value )
                {
                    return;

                }

                if( myProjectService != null )
                {
                    myProjectService.ProjectChanged -= ProjectService_ProjectChanged;
                }

                myProjectService = value;

                myProjectService.ProjectChanged += ProjectService_ProjectChanged;

                ProjectService_ProjectChanged( myProjectService, EventArgs.Empty );
            }
        }

        private void ProjectService_ProjectChanged( object sender, EventArgs e )
        {
            OnProjectChanged();

            if( myProjectService.Project != null )
            {
                TraceLog = ProjectService.Project.TraceLog;

                PropertyChangedEventManager.AddHandler( ProjectService.Project, Project_TraceLogChanged,
                    PropertySupport.ExtractPropertyName( () => ProjectService.Project.TraceLog ) );

                Presentation = ProjectService.Project.Presentation;

                PropertyChangedEventManager.AddHandler( ProjectService.Project, Project_PresentationChanged,
                    PropertySupport.ExtractPropertyName( () => ProjectService.Project.Presentation ) );
            }
        }

        protected virtual void OnProjectChanged() { }

        private void Project_TraceLogChanged( object sender, PropertyChangedEventArgs e )
        {
            TraceLog = ProjectService.Project.TraceLog;
        }

        private void Project_PresentationChanged( object sender, PropertyChangedEventArgs e )
        {
            Presentation = ProjectService.Project.Presentation;
        }

        public ITraceLog TraceLog
        {
            get { return myTraceLog; }
            set
            {
                var oldValue = myTraceLog;
                if( SetProperty( ref myTraceLog, value ) )
                {
                    OnTraceLogChanged( oldValue );
                }
            }
        }

        protected virtual void OnTraceLogChanged( ITraceLog oldValue ) { }

        public FlameSetPresentation Presentation
        {
            get { return myPresentation; }
            set
            {
                var oldValue = myPresentation;
                if( SetProperty( ref myPresentation, value ) )
                {
                    OnPresentationChanged( oldValue );
                }
            }
        }

        protected virtual void OnPresentationChanged( FlameSetPresentation oldValue ) { }
    }
}
