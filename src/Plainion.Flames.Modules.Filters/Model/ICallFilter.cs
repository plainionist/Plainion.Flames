using Plainion.Flames.Presentation;

namespace Plainion.Flames.Modules.Filters.Model
{
    interface ICallFilter
    {
        string Label { get; }

        /// <summary>
        /// Indicates whether the filter defines the calls to show or to hide
        /// Default: true
        /// </summary>
        bool IsShowFilter { get; }

        /// <summary>
        /// Defines whether filter is considered when rendering the scene or not.
        /// this approach avoid having two models (all filters, rendered filters) in sync.
        /// Default: true
        /// </summary>
        bool IsApplied { get; }

        /// <summary>
        /// Returns true if call is visible according to this filter.
        /// Returns false if call is not visible according to this filter.
        /// Returns null if this filter does not match this call.
        /// </summary>
        bool? IsVisible( Activity call );
    }
}
