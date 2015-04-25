using System;
using Plainion;

namespace Plainion.Flames.Model
{
    public class Method : IEquatable<Method>, IComparable<Method>, IComparable
    {
        internal Method( string module, string callNamespace, string callClass, string name )
        {
            Contract.RequiresNotNullNotEmpty( name, "name" );

            Module = module;
            Namespace = callNamespace;
            Class = callClass;
            Name = name;

            Contract.Requires( module != null || callNamespace != null || callClass != null, "At least module, namespace or class must be set" );
        }

        public string Module { get; private set; }

        public string Namespace { get; private set; }

        public string Class { get; private set; }

        public string Name { get; private set; }

        public bool Equals( Method other )
        {
            if( object.ReferenceEquals( this, other ) )
            {
                return true;
            }

            if( Module != other.Module )
            {
                return false;
            }

            if( Namespace != other.Namespace )
            {
                return false;
            }

            if( Class != other.Class )
            {
                return false;
            }

            return Name == other.Name;
        }

        public override bool Equals( object obj )
        {
            var other = obj as Method;
            if( other == null )
            {
                return false;
            }

            return Equals( other );
        }

        // http://stackoverflow.com/questions/70303/how-do-you-implement-gethashcode-for-structure-with-two-string-when-both-string
        public override int GetHashCode()
        {
            return GetHashCode( Module, Namespace, Class, Name );
        }

        internal static int GetHashCode( string module, string callNamespace, string callClass, string methodName )
        {
            unchecked
            {
                return ( module == null ? 0 : module.GetHashCode() )
                    ^ ( callNamespace == null ? 0 : callNamespace.GetHashCode() )
                    ^ ( callClass == null ? 0 : callClass.GetHashCode() )
                    ^ methodName.GetHashCode();
            }
        }

        // From MSDN: 
        //     A value that indicates the relative order of the objects being compared.
        //     The return value has the following meanings: Value Meaning Less than zero
        //     This object is less than the other parameter.Zero This object is equal to
        //     other. Greater than zero This object is greater than other.
        public int CompareTo( Method other )
        {
            if( Module == null )
            {
                if( other.Module != null )
                {
                    return -1;
                }
            }
            else
            {
                var cmp = Module.CompareTo( other.Module );
                if( cmp != 0 )
                {
                    return cmp;
                }
            }

            if( Namespace == null )
            {
                if( other.Namespace != null )
                {
                    return -1;
                }
            }
            else
            {
                var cmp = Namespace.CompareTo( other.Namespace );
                if( cmp != 0 )
                {
                    return cmp;
                }
            }

            if( Class == null )
            {
                if( other.Class != null )
                {
                    return -1;
                }
            }
            else
            {
                var cmp = Class.CompareTo( other.Class );
                if( cmp != 0 )
                {
                    return cmp;
                }
            }

            return Name.CompareTo( other.Name );
        }

        public int CompareTo( object obj )
        {
            var other = obj as Method;
            if( other == null )
            {
                return -1;
            }

            return CompareTo( other );
        }
    }
}
