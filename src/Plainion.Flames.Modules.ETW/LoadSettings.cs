using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Plainion.Flames.Infrastructure.Model;

namespace Plainion.Flames.Modules.ETW
{
    [Document( "{DE31E0F0-5068-4638-A343-731C108AA91B}.LoadSettings" )]
    [DataContract( Name = "EtwLoadSettings", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/EtwLoadSettings" )]
    class LoadSettings : DataContractDocumentBase<LoadSettings>
    {
        [DataMember( Name = "Version" )]
        public const byte Version = 1;

        public LoadSettings()
        {
            SelectedProcesses = new HashSet<int>();
        }

        [DataMember( Name = "SymbolPath" )]
        public string SymbolPath { get; set; }

        [DataMember( Name = "UseDefaultWebProxy" )]
        public bool UseDefaultWebProxy { get; set; }

        [DataMember( Name = "Processes" )]
        public HashSet<int> SelectedProcesses { get; private set; }

        public void SetDefaultSymbolPath( TraceFile traceFile )
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

        protected override void OnDeserialized( LoadSettings document )
        {
            SymbolPath = document.SymbolPath;
            UseDefaultWebProxy = document.UseDefaultWebProxy;
            SelectedProcesses = document.SelectedProcesses;
        }

        [DataMember( Name = "StartStopEventsOnly" )]
        public bool StartStopEventsOnly { get; set; }
    }
}
