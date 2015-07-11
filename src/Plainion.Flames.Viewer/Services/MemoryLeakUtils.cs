using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace Plainion.Flames.Viewer.Services
{
    class MemoryLeakUtils
    {
        // http://code.logos.com/blog/2008/10/detecting_bindings_that_should_be_onetime.html
        public static void PrintReflectTypeDescriptionProviderContents()
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

                    if( valueChangedHandlers != null && valueChangedHandlers.Count != 0 )
                    {
                        Debug.WriteLine( string.Format( "TypeName: {0}, PropertyName: {1}, HandlerCount: {2}", entry.Key, pd.Name, valueChangedHandlers.Count ) );
                    }
                }
            }
        }
    }
}
