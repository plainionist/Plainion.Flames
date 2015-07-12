using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace Plainion.Flames
{
    public class MemoryLeakUtils
    {
        public static void GenerateLeakStats()
        {
            // http://stackoverflow.com/questions/13026826/execute-command-after-view-is-loaded-wpf-mvvm
            // queue it in - we want to have the app idle - esp. all controls should be unloaded first
            Application.Current.Dispatcher.BeginInvoke( DispatcherPriority.ApplicationIdle, new Action( () =>
            {
                PrintReflectTypeDescriptionProviderContents();

                PrintDPCustomTypeDescriptorContents();
            } ) );
        }

        // http://code.logos.com/blog/2008/10/detecting_bindings_that_should_be_onetime.html
        // resolution: OneTime, INotifyPropertyChanged
        private static void PrintReflectTypeDescriptionProviderContents()
        {
            var type = typeof( PropertyDescriptor ).Module.GetType( "System.ComponentModel.ReflectTypeDescriptionProvider" );
            var propertyCache = ( Hashtable )type
                .GetField( "_propertyCache", BindingFlags.Static | BindingFlags.NonPublic )
                .GetValue( null );
            if( propertyCache == null )
            {
                return;
            }

            // try to make a copy of the hashtable as quickly as possible (this object can be accessed by other threads)
            var entries = new DictionaryEntry[ propertyCache.Count ];
            propertyCache.CopyTo( entries, 0 );

            var valueChangedHandlersFieldInfo = typeof( PropertyDescriptor )
                .GetField( "valueChangedHandlers", BindingFlags.Instance | BindingFlags.NonPublic );

            foreach( var entry in entries )
            {
                var propertyDescriptors = ( PropertyDescriptor[] )entry.Value;
                if( propertyDescriptors == null )
                {
                    continue;
                }

                foreach( var propertyDescriptor in propertyDescriptors )
                {
                    var valueChangedHandlers = ( Hashtable )valueChangedHandlersFieldInfo.GetValue( propertyDescriptor );
                    if( valueChangedHandlers == null || valueChangedHandlers.Count == 0 )
                    {
                        continue;
                    }

                    Debug.WriteLine( string.Format( "LEAK(non-observable property): ObservedType={0}, ObservedProperty={1}, HandlerCount={2}",
                        entry.Key,
                        propertyDescriptor.Name,
                        valueChangedHandlers.Count ) );
                }
            }
        }

        // resolution: RemoveValueChanged()
        private static void PrintDPCustomTypeDescriptorContents()
        {
            var type = typeof( DependencyObject ).Module.GetType( "MS.Internal.ComponentModel.DPCustomTypeDescriptor" );
            var propertyMap = ( IDictionary )type.GetField( "_propertyMap", BindingFlags.Static | BindingFlags.NonPublic )
                .GetValue( null );
            if( propertyMap == null )
            {
                return;
            }

            foreach( DictionaryEntry entry in propertyMap )
            {
                var dependencyObjectPropertyDescriptor = entry.Value;
                if( dependencyObjectPropertyDescriptor == null )
                {
                    continue;
                }

                var trackers = GetTrackersFieldFromDependencyObjectPropertyDescriptor( dependencyObjectPropertyDescriptor );
                if( trackers == null )
                {
                    continue;
                }

                foreach( DictionaryEntry trackerEntry in trackers )
                {
                    var tracker = trackerEntry.Value;

                    var changedHandler = ( EventHandler )tracker.GetType()
                        .GetField( "Changed", BindingFlags.Instance | BindingFlags.NonPublic )
                        .GetValue( tracker );
                    if( changedHandler == null )
                    {
                        continue;
                    }

                    Debug.WriteLine( string.Format( "LEAK(AddValueChanged): ObservedType={0}, ObservedProperty={1}, HandlerTarget={2}, HandlerName={3}",
                        tracker.GetType().GetField( "_object", BindingFlags.Instance | BindingFlags.NonPublic )
                            .GetValue( tracker ).GetType().FullName,
                        ( ( DependencyProperty )tracker.GetType().GetField( "_property", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( tracker ) ).Name,
                        changedHandler.Target.GetType().FullName,
                        changedHandler.Method.Name ) );
                }
            }
        }

        public static object GetPropertyChangeTracker( object component, DependencyPropertyDescriptor descriptor )
        {
            var dependencyObjectPropertyDescriptor = descriptor.GetType().GetProperty( "Property", BindingFlags.Instance | BindingFlags.NonPublic )
                .GetValue( descriptor );

            var trackers = GetTrackersFieldFromDependencyObjectPropertyDescriptor( dependencyObjectPropertyDescriptor );

            //var key = DependencyObjectPropertyDescriptor.FromObj( component );
            var fromObj = dependencyObjectPropertyDescriptor.GetType().GetMethod( "FromObj", BindingFlags.Static | BindingFlags.NonPublic );
            var key = fromObj.Invoke( null, new[] { component } );

            return trackers[ key ];
        }

        private static IDictionary GetTrackersFieldFromDependencyObjectPropertyDescriptor( object dependencyObjectPropertyDescriptor )
        {
            var trackersField = dependencyObjectPropertyDescriptor.GetType().GetField( "_trackers", BindingFlags.Instance | BindingFlags.NonPublic );
            return ( IDictionary )trackersField.GetValue( dependencyObjectPropertyDescriptor );
        }
    }
}
