using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Plainion.Flames
{
    public class MemoryLeakUtils
    {
        public static void PrintKnownLeaks()
        {
            PrintReflectTypeDescriptionProviderContents();

            PrintDPCustomTypeDescriptorContents();
        }

        // http://code.logos.com/blog/2008/10/detecting_bindings_that_should_be_onetime.html
        // resolution: OneTime, INotifyPropertyChanged
        private static void PrintReflectTypeDescriptionProviderContents()
        {
            // get the ReflectTypeDescriptionProvider._propertyCache field
            var typeRtdp = typeof( PropertyDescriptor ).Module.GetType( "System.ComponentModel.ReflectTypeDescriptionProvider" );
            var fieldInfo = typeRtdp.GetField( "_propertyCache", BindingFlags.Static | BindingFlags.NonPublic );
            var propertyCache = ( Hashtable )fieldInfo.GetValue( null );
            if( propertyCache == null )
            {
                return;
            }

            // try to make a copy of the hashtable as quickly as possible (this object can be accessed by other threads)
            var entries = new DictionaryEntry[ propertyCache.Count ];
            propertyCache.CopyTo( entries, 0 );

            var valueChangedHandlersFieldInfo = typeof( PropertyDescriptor ).GetField( "valueChangedHandlers",
                BindingFlags.Instance | BindingFlags.NonPublic );

            // count the "value changed" handlers for each type
            foreach( var entry in entries )
            {
                var pds = ( PropertyDescriptor[] )entry.Value;
                if( pds == null )
                {
                    continue;
                }

                foreach( var pd in pds )
                {
                    var valueChangedHandlers = ( Hashtable )valueChangedHandlersFieldInfo.GetValue( pd );

                    if( valueChangedHandlers != null && valueChangedHandlers.Count > 0 )
                    {
                        Debug.WriteLine( string.Format( "LEAK(non-observable property): ObservedType={0}, ObservedProperty={1}, HandlerCount={2}",
                            entry.Key,
                            pd.Name,
                            valueChangedHandlers.Count ) );
                    }
                }
            }
        }

        // resolution: RemoveValueChanged()
        private static void PrintDPCustomTypeDescriptorContents()
        {
            var type = typeof( DependencyObject ).Module.GetType( "MS.Internal.ComponentModel.DPCustomTypeDescriptor" );
            var fieldInfo = type.GetField( "_propertyMap", BindingFlags.Static | BindingFlags.NonPublic );
            var propertyMap = ( IDictionary )fieldInfo.GetValue( null );
            if( propertyMap == null )
            {
                return;
            }


            /*
Name
 Plainion.Flames!Plainion.Flames.Controls.FlameView
+ LIB <<mscorlib!Dictionary>>
|+ WindowsBase!MS.Internal.ComponentModel.DependencyObjectPropertyDescriptor
||+ LIB <<mscorlib!Dictionary>>
|||+ [static var MS.Internal.ComponentModel.DPCustomTypeDescriptor._propertyMap]
||||+ [static vars]
             */
            foreach( DictionaryEntry entry in propertyMap )
            {
                var key = entry.Key;
                if( entry.Value != null )
                {
                    var dependencyObjectPropertyDescriptor = entry.Value;
                    var trackersField = dependencyObjectPropertyDescriptor.GetType().GetField( "_trackers", BindingFlags.Instance | BindingFlags.NonPublic );
                    var trackers = ( IDictionary )trackersField.GetValue( dependencyObjectPropertyDescriptor );
                    if( trackers != null )
                    {
                        foreach( DictionaryEntry trackerEntry in trackers )
                        {
                            var tracker = trackerEntry.Value;

                            var changedHandler = ( EventHandler )tracker.GetType()
                                .GetField( "Changed", BindingFlags.Instance | BindingFlags.NonPublic )
                                .GetValue( tracker );
                            if( changedHandler != null )
                            {
                                Debug.WriteLine( string.Format( "LEAK(AddValueChanged): ObservedType={0}, ObservedProperty={1}, HandlerTarget={2}, HandlerName={3}",
                                    tracker.GetType().GetField( "_object", BindingFlags.Instance | BindingFlags.NonPublic )
                                        .GetValue( tracker ).GetType().FullName,
                                    ( ( DependencyProperty )tracker.GetType().GetField( "_property", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( tracker ) ).Name,
                                    changedHandler.Target.GetType().FullName,
                                    changedHandler.Method.Name ) );
                            }
                        }
                    }
                }
            }
        }

        public static object GetPropertyChangeTracker( object component, DependencyPropertyDescriptor descriptor )
        {
            var dependencyObjectPropertyDescriptor = descriptor.GetType().GetProperty( "Property", BindingFlags.Instance | BindingFlags.NonPublic )
                .GetValue( descriptor );

            var trackersField = dependencyObjectPropertyDescriptor.GetType().GetField( "_trackers", BindingFlags.Instance | BindingFlags.NonPublic );
            var trackers = ( IDictionary )trackersField.GetValue( dependencyObjectPropertyDescriptor );

            //var key = DependencyObjectPropertyDescriptor.FromObj( component );
            var fromObj = dependencyObjectPropertyDescriptor.GetType().GetMethod( "FromObj", BindingFlags.Static | BindingFlags.NonPublic );
            var key = fromObj.Invoke( null, new[] { component } );

            return trackers[ key ];
        }
    }
}
