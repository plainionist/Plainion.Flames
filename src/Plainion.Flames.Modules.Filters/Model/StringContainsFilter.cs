﻿using System;
using System.Runtime.Serialization;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Modules.Filters.Model
{
    // TODO: ignores case for simplicitly - later own we should let the user chose
    [DataContract(Name = "StringContainsFilter", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/Filters")]
    class StringContainsFilter : NameFilterBase
    {
        public StringContainsFilter( FilterTarget target, string filter )
            : base( target, target + " contains '" + filter + "'" )
        {
            Contract.RequiresNotNullNotEmpty( filter, "filter" );

            Filter = filter;
        }

        [DataMember(Name = "Filter")]
        public string Filter { get; private set; }

        public override bool? IsVisible( Activity activity )
        {
            if( !IsApplied )
            {
                return null;
            }

            if( !Matches( activity.Model.Method ) )
            {
                return null;
            }

            return IsShowFilter;
        }

        private bool Matches( Method call )
        {
            switch( Target )
            {
                case FilterTarget.Module: return call.Module != null && call.Module.Contains( Filter, StringComparison.OrdinalIgnoreCase );
                case FilterTarget.Namespace: return call.Namespace != null && call.Namespace.Contains( Filter, StringComparison.OrdinalIgnoreCase );
                case FilterTarget.Class: return call.Class != null && call.Class.Contains( Filter, StringComparison.OrdinalIgnoreCase );
                case FilterTarget.Method: return call.Name.Contains( Filter, StringComparison.OrdinalIgnoreCase );
                default: throw new NotSupportedException( Target.ToString() );
            }
        }

        public override bool? IsVisible( FilterTarget target, string value )
        {
            if( !IsApplied || Target != target )
            {
                return null;
            }

            if( !value.Contains( Filter, StringComparison.OrdinalIgnoreCase ) )
            {
                return null;
            }

            return IsShowFilter;
        }
    }
}
