using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;
using Plainion.Flames.Viewer.Model;
using Plainion.Prism.Events;
using Plainion.Progress;

namespace Plainion.Flames.Viewer.Services
{
    [Export]
    class PersistencyService
    {
        private Solution mySolution;
        private string myOpenTraceFilter;
        private string mySaveTraceFilter;

        [ImportingConstructor]
        public PersistencyService(IEventAggregator eventAggregator, Solution solution)
        {
            mySolution = solution;

            eventAggregator.GetEvent<ApplicationShutdownEvent>().Subscribe(x => UnloadSolution());
        }

        [ImportMany]
        public IEnumerable<ITraceReader> TraceReaders { get; private set; }

        [ImportMany]
        public IEnumerable<ITraceWriter> TraceWriters { get; private set; }

        [ImportMany]
        public IEnumerable<IProjectItemProvider> ProjectItemProviders { get; private set; }

        public string OpenTraceFilter
        {
            get
            {
                if (myOpenTraceFilter == null)
                {
                    myOpenTraceFilter = "All files (*.*)|*.*|" + string.Join("|", TraceReaders
                        .SelectMany(r => r.FileFilters)
                        .Select(f => string.Format("{0}|*{1}", f.Description, f.Extension)));
                }

                return myOpenTraceFilter;
            }
        }

        public string SaveTraceFilter
        {
            get
            {
                if (mySaveTraceFilter == null)
                {
                    mySaveTraceFilter = string.Join("|", TraceWriters
                        .SelectMany(r => r.FileFilters)
                        .Select(f => string.Format("{0}|*{1}", f.Description, f.Extension)));
                }

                return mySaveTraceFilter;
            }
        }

        // TODO: what would be the contract if nothing could be loaded?
        public async Task LoadAsync(Project project, IProgress<IProgressInfo> progress)
        {
            var file = GetProjectFile(project);

            if (File.Exists(file))
            {
                using (var stream = new FileStream(file, FileMode.Open))
                {
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        project.WasDeserialized = true;
                        await LoadTraceLogAsync(project, new ProjectSerializationContext(archive), progress);
                    }
                }
            }
            else
            {
                project.WasDeserialized = false;
                await LoadTraceLogAsync(project, null, progress);
            }
        }

        private async Task LoadTraceLogAsync(Project project, IProjectSerializationContext context, IProgress<IProgressInfo> progress)
        {
            foreach (var provider in ProjectItemProviders)
            {
                provider.OnTraceLogLoading(project, context);
            }

            var builder = new TraceModelBuilder();

            foreach (var item in project.Items)
            {
                builder.ReaderContextHints.Add(item);
            }

            foreach (var traceFile in project.TraceFiles)
            {
                var ext = Path.GetExtension(traceFile);
                var reader = TryGetTraceReaderByExtension(ext);
                Contract.Requires(reader != null, "No Reader found for file extension: " + ext);

                await reader.ReadAsync(traceFile, builder, progress);
            }

            project.TraceLog = builder.Complete();

            foreach (var hint in builder.ReaderContextHints)
            {
                project.Items.Add(hint);
            }

            foreach (var provider in ProjectItemProviders)
            {
                provider.OnTraceLogLoaded(project, context);
            }

            OnTracesLoadCompleted(project.TraceFiles);
        }

        private ITraceReader TryGetTraceReaderByExtension(string ext)
        {
            return TraceReaders
                .SingleOrDefault(r => r.FileFilters
                    .Any(f => f.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase)));
        }

        public bool CanLoad(string f)
        {
            return TryGetTraceReaderByExtension(Path.GetExtension(f)) != null;
        }

        /// <summary>
        /// Saves certain aspects (provided by ITraceLog) of the project.
        /// </summary>
        public Task ExportAsync(ITraceLog traceLog, string filename, IProgress<IProgressInfo> progress)
        {
            Contract.RequiresNotNull(traceLog, "traceLog");

            var writer = TraceWriters
                .Single(r => r.FileFilters
                    .Any(f => f.Extension.Equals(Path.GetExtension(filename), StringComparison.OrdinalIgnoreCase)));

            return Task.Run(() =>
                {
                    var task = writer.WriteAsync(traceLog, filename, progress);

                    task.Wait();
                });
        }

        private void UnloadSolution()
        {
            foreach (var project in mySolution.Projects)
            {
                Unload(project);
            }
        }

        public void Unload(Project project)
        {
            var file = GetProjectFile(project);

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            using (var stream = new FileStream(file, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    var context = new ProjectSerializationContext(archive);

                    foreach (var provider in ProjectItemProviders)
                    {
                        provider.OnProjectUnloading(project, context);
                    }

                    project.TraceLog.Dispose();
                    project.TraceLog = null;

                    foreach (var provider in ProjectItemProviders)
                    {
                        provider.OnProjectUnloaded(project, context);
                    }
                }
            }
        }

        private static string GetProjectFile(Project project)
        {
            var mainTraceFile = project.TraceFiles.First();
            return Path.Combine(Path.GetDirectoryName(mainTraceFile), Path.GetFileNameWithoutExtension(mainTraceFile) + ".pfp");
        }

        [Import]
        private TraceLoaderService TraceLoader { get; set; }

        private void OnTracesLoadCompleted(IEnumerable<string> traceFiles)
        {
            TraceLoader.LoadCompleted(traceFiles);
        }

        public Task CreatePresentationAsync(Project project, IProgress<IProgressInfo> progress)
        {
            return Task.Run(() =>
                {
                    progress.Report(new UndefinedProgress("Creating presentation"));

                    var factory = new PresentationFactory();
                    project.Presentation = factory.CreateFlameSetPresentation(project.TraceLog);

                    foreach (var provider in ProjectItemProviders)
                    {
                        provider.OnPresentationCreated(project);
                    }
                });
        }
    }
}
