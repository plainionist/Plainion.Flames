using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Etlx;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Modules.ETW.Builders;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Progress;

namespace Plainion.Flames.Modules.ETW
{
    [Export(typeof(ITraceReader))]
    [Export(typeof(EtwTraceReader))]
    class EtwTraceReader : ITraceReader
    {
        private IAsyncWindowRequestFactory myWindowFactory;
        private TraceFile myLastTraceFile;
        private LoadSettings myLoadSettings;

#pragma warning disable 649
        [Import]
        private FlamesSettingsViewModel myFlamesSettings;
#pragma warning restore 649

        [ImportingConstructor]
        public EtwTraceReader(IAsyncWindowRequestFactory windowFactory)
        {
            myWindowFactory = windowFactory;

            FileFilters = new[] { new FileFilter(".etl", "ETW files (*.etl)"), new FileFilter(".etlx", "eXtended ETW files (*.etlx)") };
        }

        public IEnumerable<FileFilter> FileFilters { get; private set; }

        public Task ReadAsync(string filename, TraceModelBuilder builder, IProgress<IProgressInfo> progress)
        {
            var traceFile = new TraceFile(filename);

            // TODO: this is the bypass for the "interpolate broken stacks"
            if (myLastTraceFile != null && myLastTraceFile.Equals(traceFile))
            {
                return Task.Run(() => Read(builder, traceFile, progress));
            }

            var dialog = new Dialog();
            dialog.Width = 400;
            dialog.Height = 500;
            dialog.Title = "Open trace";

            myLoadSettings = builder.ReaderContextHints.OfType<LoadSettings>().SingleOrDefault();
            if (myLoadSettings == null)
            {
                myLoadSettings = new LoadSettings();
                myLoadSettings.SetDefaultSymbolPath(traceFile);

                builder.ReaderContextHints.Add(myLoadSettings);
            }

            return Task.Run<Task>(() =>
                    {
                        dialog.Content = OpenTraceViewModel.Create(traceFile, myLoadSettings, progress);

                        var request = myWindowFactory.CreateForView(typeof(OpenTraceView));
                        return request.Raise(dialog);
                    })
                .Unwrap()
                .ContinueWith(t =>
                    {
                        if (!dialog.Confirmed)
                        {
                            return;
                        }

                        Read(builder, traceFile, progress);

                        myLastTraceFile = traceFile;
                    });
        }

        private void Read(TraceModelBuilder builder, TraceFile traceFile, IProgress<IProgressInfo> progress)
        {
            if (myLoadSettings.UseDefaultWebProxy)
            {
                WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            }
            else
            {
                WebRequest.DefaultWebProxy.Credentials = null;
            }

            Microsoft.Diagnostics.Symbols.SymbolPath.SymbolPathFromEnvironment = myLoadSettings.SymbolPath;

            using (var handle = new TraceFileHandle(traceFile))
            {
                handle.Open(progress);

                //handle.Trace.SampleProfileInterval ( SampledProfileIntervalTraceData)
                ReadEventsWithStacks(builder, handle.Trace, myLoadSettings.SelectedProcesses.ToList(), progress);
            }

            GC.Collect();
        }

        private void ReadEventsWithStacks(TraceModelBuilder builder, TraceLog traceLog, IReadOnlyCollection<int> processesToLoad, IProgress<IProgressInfo> progress)
        {
            var progressInfo = new PercentageProgress("Step 3: Reading ETW events", traceLog.EventCount);
            progress.Report(progressInfo);

            builder.SetCreationTime(traceLog.SessionStartTime);
            builder.SetTraceDuration((long)(traceLog.SessionEndTimeRelativeMSec * 1000));

            double lastReportedProgress = 0;

            var consumers = GetConsumers(builder, processesToLoad).ToList();
            foreach (var evt in traceLog.Events)
            {
                progressInfo.Value++;
                if (lastReportedProgress + 5 < progressInfo.Progress)
                {
                    progress.Report(progressInfo);
                    lastReportedProgress = progressInfo.Progress;
                }

                foreach (var consumer in consumers)
                {
                    consumer.Consume(evt);
                }
            }

            foreach (var consumer in consumers)
            {
                consumer.Complete();
            }
        }

        private IEnumerable<IEventConsumer> GetConsumers(TraceModelBuilder builder, IReadOnlyCollection<int> processesToLoad)
        {
            yield return new CallBuilder(builder, processesToLoad)
                {
                    InterpolateBrokenStackSamples = myFlamesSettings.InterpolateBrokenStackSamples
                };

            yield return new CSwitchBookmarksBuilder(builder, processesToLoad);

            yield return new CpuSamplingBookmarksBuilder(builder, processesToLoad);

            yield return new ReadyThreadBookmarksBuilder(builder, processesToLoad);

            yield return new ClrExceptionBookmarksBuilder(builder, processesToLoad);

            yield return new ThreadLifecycleBookmarksBuilder(builder, processesToLoad);
        }
    }
}
