using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Plainion.Windows.Diagnostics
{
    public class WpfStatics
    {
        static WpfStatics()
        {
            Findings = new ObservableCollection<DiagnosticFinding>();
        }

        public static ObservableCollection<DiagnosticFinding> Findings { get; private set; }

        public static bool WriteToDebugConsole { get; private set; }

        public static void CollectStatisticsOnIdle()
        {
            // http://stackoverflow.com/questions/13026826/execute-command-after-view-is-loaded-wpf-mvvm
            // queue it in - we want to have the app idle - esp. all controls should be unloaded first
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                CollectStatistics();
            }));
        }

        public static void CollectStatistics()
        {
            Findings.Clear();

            InspectReflectTypeDescriptionProvider();

            InspectDPCustomTypeDescriptor();

            InspectViewManager();

            if (WriteToDebugConsole)
            {
                using (var writer = new DebugTextWriter())
                {
                    foreach (var finding in Findings)
                    {
                        finding.WriteTo(writer);
                        writer.WriteLine();
                    }
                }
            }
        }

        // http://code.logos.com/blog/2008/10/detecting_bindings_that_should_be_onetime.html
        // resolution: OneTime, INotifyPropertyChanged
        private static void InspectReflectTypeDescriptionProvider()
        {
            var finding = new DiagnosticFinding(
                "Data binding to non-observable property causes memory leak",
                "Choose one of the following options:" + Environment.NewLine +
                "  a) convert to DependencyProperty" + Environment.NewLine +
                "  b) implement INotifyPropertyChanged in owning type" + Environment.NewLine +
                "  c) bind with BindingMode=OneTime");

            var type = typeof(PropertyDescriptor).Module.GetType("System.ComponentModel.ReflectTypeDescriptionProvider");
            var propertyCache = (Hashtable)type
                .GetField("_propertyCache", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null);
            if (propertyCache == null)
            {
                return;
            }

            // try to make a copy of the hashtable as quickly as possible (this object can be accessed by other threads)
            var entries = new DictionaryEntry[propertyCache.Count];
            propertyCache.CopyTo(entries, 0);

            var valueChangedHandlersFieldInfo = typeof(PropertyDescriptor)
                .GetField("valueChangedHandlers", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var entry in entries)
            {
                var propertyDescriptors = (PropertyDescriptor[])entry.Value;
                if (propertyDescriptors == null)
                {
                    continue;
                }

                foreach (var propertyDescriptor in propertyDescriptors)
                {
                    var valueChangedHandlers = (Hashtable)valueChangedHandlersFieldInfo.GetValue(propertyDescriptor);
                    if (valueChangedHandlers == null || valueChangedHandlers.Count == 0)
                    {
                        continue;
                    }

                    finding.AddLocation("ObservedType={0}, ObservedProperty={1}, HandlerCount={2}",
                        entry.Key, propertyDescriptor.Name, valueChangedHandlers.Count);
                }
            }

            if (finding.Locations.Any())
            {
                Findings.Add(finding);
            }
        }

        // resolution: RemoveValueChanged()
        private static void InspectDPCustomTypeDescriptor()
        {
            var finding = new DiagnosticFinding(
                "Observation of DependencyProperty using DependencyPropertyDescriptor.AddValueChanged() causes memory leak",
                "Call DependencyPropertyDescriptor.RemoveValueChanged() to remove the event handler.");

            var type = typeof(DependencyObject).Module.GetType("MS.Internal.ComponentModel.DPCustomTypeDescriptor");
            var propertyMap = (IDictionary)type.GetField("_propertyMap", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null);
            if (propertyMap == null)
            {
                return;
            }

            foreach (DictionaryEntry entry in propertyMap)
            {
                var dependencyObjectPropertyDescriptor = entry.Value;
                if (dependencyObjectPropertyDescriptor == null)
                {
                    continue;
                }

                var trackers = GetTrackersFieldFromDependencyObjectPropertyDescriptor(dependencyObjectPropertyDescriptor);
                if (trackers == null)
                {
                    continue;
                }

                foreach (DictionaryEntry trackerEntry in trackers)
                {
                    var tracker = trackerEntry.Value;

                    var changedHandler = (EventHandler)tracker.GetType()
                        .GetField("Changed", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(tracker);
                    if (changedHandler == null)
                    {
                        continue;
                    }

                    finding.AddLocation("ObservedType={0}, ObservedProperty={1}, HandlerTarget={2}, HandlerName={3}",
                        tracker.GetType().GetField("_object", BindingFlags.Instance | BindingFlags.NonPublic)
                            .GetValue(tracker).GetType().FullName,
                        ((DependencyProperty)tracker.GetType().GetField("_property", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(tracker)).Name,
                        changedHandler.Target.GetType().FullName,
                        changedHandler.Method.Name);
                }
            }

            if (finding.Locations.Any())
            {
                Findings.Add(finding);
            }
        }

        public static object GetPropertyChangeTracker(object component, DependencyPropertyDescriptor descriptor)
        {
            var dependencyObjectPropertyDescriptor = descriptor.GetType().GetProperty("Property", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(descriptor);

            var trackers = GetTrackersFieldFromDependencyObjectPropertyDescriptor(dependencyObjectPropertyDescriptor);

            //var key = DependencyObjectPropertyDescriptor.FromObj( component );
            var fromObj = dependencyObjectPropertyDescriptor.GetType().GetMethod("FromObj", BindingFlags.Static | BindingFlags.NonPublic);
            var key = fromObj.Invoke(null, new[] { component });

            return trackers[key];
        }

        private static IDictionary GetTrackersFieldFromDependencyObjectPropertyDescriptor(object dependencyObjectPropertyDescriptor)
        {
            var trackersField = dependencyObjectPropertyDescriptor.GetType().GetField("_trackers", BindingFlags.Instance | BindingFlags.NonPublic);
            return (IDictionary)trackersField.GetValue(dependencyObjectPropertyDescriptor);
        }

        private static void InspectViewManager()
        {
            var finding = new DiagnosticFinding(
                "Data binding to non-observable collection OR uage of non-observable collection together with CollectionViewSource causes higher memory footpint."+
                "This is no real memory leak - the memory will be released after some 'Purge cylces' of the ViewManager. See http://referencesource.microsoft.com/PresentationFramework/Framework/MS/Internal/Data/ViewManager.cs.html",
                "If you need to free the memory as soon as it is no longer needed by your application consider converting this collection into one which implements INotifyCollectionChanged (e.g. ObservableCollection<>) - even if the collection is immutable.");

            var type = typeof(Binding).Module.GetType("MS.Internal.Data.ViewManager");
            var viewManager = (IDictionary)type.GetProperty("Current", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null);
            if (viewManager == null)
            {
                return;
            }

            var inactiveViewTables = (IDictionary)type.GetField("_inactiveViewTables", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(viewManager);
            if (inactiveViewTables == null)
            {
                return;
            }

            if (inactiveViewTables.Count == 0)
            {
                return;
            }

            var entries = inactiveViewTables.OfType<DictionaryEntry>().ToList();

            foreach (var entry in entries)
            {
                var viewTable = (IEnumerable)entry.Key;
                var entryWithViewRecord = (DictionaryEntry)viewTable.OfType<object>().First();
                var collectionView = (ICollectionView)entryWithViewRecord.Value.GetType()
                    .GetProperty("View", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(entryWithViewRecord.Value);
                var sourceCollection = collectionView.SourceCollection;

                if (collectionView.IsEmpty)
                {
                    continue;
                }

                finding.AddLocation("CollectionType={0}, Count={1}",
                    collectionView.SourceCollection.GetType().FullName,
                    collectionView.SourceCollection.OfType<object>().Count());
            }

            if (finding.Locations.Any())
            {
                Findings.Add(finding);
            }
        }
    }
}
