using System.Collections.Generic;
using Plainion;

namespace Plainion.Flames.Presentation
{
    class CachingRingBuffer<TKey, TValue>
    {
        private IReadOnlyList<TValue> myValues;
        private IDictionary<TKey, TValue> myCache;
        private int myPos;

        public CachingRingBuffer( IReadOnlyList<TValue> values )
        {
            Contract.RequiresNotNullNotEmpty( values, "values" );

            myValues = values;

            myCache = new Dictionary<TKey, TValue>();

            myPos = 0;
        }

        public TValue Get( TKey key )
        {
            TValue value;
            if( !myCache.TryGetValue( key, out value ) )
            {
                if( myPos >= myValues.Count )
                {
                    myPos = 0;
                }

                value = myValues[ myPos ];
                myCache.Add( key, value );

                myPos++;
            }

            return value;
        }
    }
}
