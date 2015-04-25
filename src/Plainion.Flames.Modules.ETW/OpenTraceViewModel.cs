using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Plainion.Flames.Infrastructure.Controls;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.Progress;
using Plainion.Prism.Interactivity.InteractionRequest;
using Microsoft.Diagnostics.Tracing.Etlx;

namespace Plainion.Flames.Modules.ETW
{
    class OpenTraceViewModel : BindableBase, IInteractionRequestAware
    {
        private string mySymbolPath;
        private bool myUseDefaultWebProxy;
        private bool myHasCpuSamples;
        private bool myHasCSwitches;

        public OpenTraceViewModel()
        {
            LoadCommand = new DelegateCommand<Window>( w => { Notification.TrySetConfirmed( true ); FinishInteraction(); } );
            CancelCommand = new DelegateCommand<Window>( w => { Notification.TrySetConfirmed( false ); FinishInteraction(); } );
        }

        public ICommand LoadCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public string SymbolPath
        {
            get { return mySymbolPath; }
            set
            {
                if( SetProperty( ref mySymbolPath, value ) )
                {
                    Microsoft.Diagnostics.Symbols.SymbolPath.SymbolPathFromEnvironment = mySymbolPath;
                }
            }
        }

        public bool UseDefaultWebProxy
        {
            get { return myUseDefaultWebProxy; }
            set { SetProperty( ref myUseDefaultWebProxy, value ); }
        }

        public TracesTree TracesTreeSource { get; private set; }

        public Visibility SymbolSettingsVisibility { get; private set; }

        public bool HasCpuSamples
        {
            get { return myHasCpuSamples; }
            set { SetProperty( ref myHasCpuSamples, value ); }
        }

        public bool HasCSwitches
        {
            get { return myHasCSwitches; }
            set { SetProperty( ref myHasCSwitches, value ); }
        }

        public string MissingEvents
        {
            get
            {
                var events = new List<string>( 2 );
                if( !HasCpuSamples )
                {
                    events.Add( "CPU samples" );
                }
                if( !HasCSwitches )
                {
                    events.Add( "CSwitches" );
                }

                return string.Join( ",", events );
            }
        }

        internal static OpenTraceViewModel Create( TraceFile traceFile, IProgress<IProgressInfo> progress )
        {
            var dataContext = new OpenTraceViewModel();

            // set it in any case - in case TraceEvent library decides to create new etlx - e.g. because of 
            // newer version 
            dataContext.SetDefaultSymbolPath( traceFile );

            if( traceFile.EtlxExists )
            {
                dataContext.SymbolSettingsVisibility = Visibility.Collapsed;
            }
            else
            {
                dataContext.SymbolSettingsVisibility = Visibility.Visible;
            }

            dataContext.Initialize( traceFile, progress );

            return dataContext;
        }

        private void SetDefaultSymbolPath( TraceFile traceFile )
        {
            var symbolPath = Environment.GetEnvironmentVariable( "_NT_SYMBOL_PATH" );
            if( !string.IsNullOrEmpty( symbolPath ) )
            {
                symbolPath += ";";
            }

            if( Directory.Exists( traceFile.Etl + ".NGENPDB" ) )
            {
                symbolPath += @"srv*" + traceFile.Etl + ".NGENPDB;";
            }
            symbolPath += @"c:\windows\symbols\dll";
            symbolPath += @";srv*d:\Symbols*http://msdl.microsoft.com/download/symbols";

            SymbolPath = symbolPath;
        }

        private void Initialize( TraceFile traceFile, IProgress<IProgressInfo> progress )
        {
            var processes = new Dictionary<int, TraceProcessNode>();

            if( File.Exists( traceFile.Etl ) )
            {
                GetProcessesFromEtl( traceFile.Etl, processes, progress );
            }
            else
            {
                // handle the case that only ETLX was saved
                GetProcessesFromEtlx( traceFile.Etlx, processes, progress );
            }

            TracesTreeSource = new TracesTree
            {
                Processes = processes.Values
                    .OrderBy( p => p.Name )
                    .ThenBy( p => p.ProcessId )
                    .ToList()
            };
        }

        private void GetProcessesFromEtl( string etl, Dictionary<int, TraceProcessNode> processes, IProgress<IProgressInfo> progress )
        {
            using( var source = new ETWTraceEventSource( etl ) )
            {
                var progressInfo = new PercentageProgress( "Step 1: Getting all processes", source.SessionDuration.TotalMilliseconds );
                progress.Report( progressInfo );

                Action<ProcessTraceData> OnProcessStartEnd = e =>
                {
                    progressInfo.Value = e.TimeStampRelativeMSec;
                    progress.Report( progressInfo );

                    AddProcess( processes, e );
                };

                source.Kernel.ProcessStartGroup += OnProcessStartEnd;
                source.Kernel.ProcessEndGroup += OnProcessStartEnd;

                Action<SampledProfileTraceData> OnCpuSample = null;
                OnCpuSample = e =>
                {
                    HasCpuSamples = true;
                    source.Kernel.PerfInfoSample -= OnCpuSample;
                };
                source.Kernel.PerfInfoSample += OnCpuSample;

                Action<CSwitchTraceData> OnCSwitch = null;
                OnCSwitch = e =>
                {
                    HasCSwitches = true;
                    source.Kernel.ThreadCSwitch -= OnCSwitch;
                };
                source.Kernel.ThreadCSwitch += OnCSwitch;

                source.Process();

                progressInfo.Value = progressInfo.Maximum;
                progress.Report( progressInfo );
            }
        }

        private static void AddProcess( Dictionary<int, TraceProcessNode> processes, ProcessTraceData e )
        {
            if( !processes.ContainsKey( e.ProcessID ) )
            {
                processes[ e.ProcessID ] = new TraceProcessNode
                {
                    ProcessId = e.ProcessID,
                    Name = e.ImageFileName
                };
            }
        }

        private void GetProcessesFromEtlx( string etlx, Dictionary<int, TraceProcessNode> processes, IProgress<IProgressInfo> progress )
        {
            var source = TraceLog.OpenOrConvert( etlx );

            var progressInfo = new PercentageProgress( "Step 1: Getting all processes", source.SessionDuration.TotalMilliseconds );
            progress.Report( progressInfo );

            foreach( var p in source.Processes )
            {
                processes[ p.ProcessID ] = new TraceProcessNode
                {
                    ProcessId = p.ProcessID,
                    Name = p.ImageFileName
                };
            }

            foreach( var evt in source.Events )
            {
                if( evt is SampledProfileIntervalTraceData )
                {
                    HasCpuSamples = true;
                }
                else if( evt is CSwitchTraceData )
                {
                    HasCSwitches = true;
                }

                if( HasCpuSamples && HasCSwitches )
                {
                    break;
                }
            }

            progressInfo.Value = progressInfo.Maximum;
            progress.Report( progressInfo );
        }

        public Action FinishInteraction { get; set; }

        public INotification Notification { get; set; }
    }
}
