using System.Collections;
using System.Collections.Generic;
using System.IO;
using Plainion;

namespace Plainion.Flames.Model
{
    public class SymbolPool
    {
        private Dictionary<int, string> myValues;
        private bool myIsFrozen;

        public SymbolPool()
        {
            myValues = new Dictionary<int, string>();
            myIsFrozen = false;
        }

        public string Intern( string value )
        {
            Contract.Invariant( !myIsFrozen, "SymbolPool is frozen. Modifications not allowed" );

            if( value == null )
            {
                return null;
            }

            string pooledString;
            if( myValues.TryGetValue( value.GetHashCode(), out pooledString ) )
            {
                return pooledString;
            }
            else
            {
                myValues.Add( value.GetHashCode(), value );
                return value;
            }
        }

        public void Clear()
        {
            myValues.Clear();
        }

        public IEnumerable<string> Values { get { return myValues.Values; } }

        public int Count { get { return myValues.Count; } }

        public string Get( int key )
        {
            if( key == 0 )
            {
                return null;
            }

            return myValues[ key ];
        }

        internal void Freeze()
        {
            myIsFrozen = true;
        }
        
        public void Serialize( BinaryWriter writer )
        {
            writer.Write( myValues.Count );

            foreach( var entry in myValues )
            {
                writer.Write( entry.Key );
                writer.Write( entry.Value );
            }
        }

        public void Deserialize( BinaryReader reader )
        {
            Contract.Invariant( myValues.Count == 0, "Pool already in use. Deserialization not allowed" );

            var count = reader.ReadInt32();

            for( int i = 0; i < count; ++i )
            {
                var key = reader.ReadInt32();
                var value = reader.ReadString();

                myValues.Add( key, value );
            }
        }
    }
}
