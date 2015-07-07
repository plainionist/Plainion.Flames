using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Plainion.Flames.Modules.Filters.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Modules.Filters
{
    class CallFilterModule
    {
        private const uint FilterMaskBit = 1;
        private const uint DurationMaskBit = 2;

        private INameFilter myAllCallsFilter;
        private FlameSetPresentation myPresentation;

        private CallFilterModule()
        {
        }

        public static CallFilterModule CreateEmpty()
        {
            var module = new CallFilterModule();
            module.DurationFilter = new DurationFilter();
            module.NameFilters = new ObservableCollection<INameFilter>();

            module.Initialize();

            return module;
        }

        public static CallFilterModule CreateFromDocument(FiltersDocument document)
        {
            var module = new CallFilterModule();
            module.DurationFilter = document.DurationFilter;
            module.NameFilters = new ObservableCollection<INameFilter>(document.NameFilters);

            module.Initialize();

            return module;
        }

        private void Initialize()
        {
            myAllCallsFilter = new AllCallsFilter { IsApplied = false, IsShowFilter = false };
            myAllCallsFilter.PropertyChanged += OnNameFilterPropertyChanged;
            NameFilters.Add(myAllCallsFilter);

            NameFilters.CollectionChanged += OnNameFilterCollectionChanged;

            DurationFilter.PropertyChanged += OnDurationFilterPropertyChanged;
        }

        public FlameSetPresentation Presentation
        {
            get { return myPresentation; }
            internal set
            {
                if (myPresentation == value)
                {
                    return;
                }

                myPresentation = value;

                if (DurationFilter.IsApplied)
                {
                    ApplyDurationFilter();
                }

                if (NameFilters.Count > 1)
                {
                    ApplyNameFilters();
                }
            }
        }

        public DurationFilter DurationFilter { get; private set; }

        public event EventHandler NameFilterApplianceChanged;

        private void OnDurationFilterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Threshold")
            {
                ApplyDurationFilter();
            }
        }

        private void OnNameFilterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsShowFilter" || e.PropertyName == "IsApplied")
            {
                if (NameFilterApplianceChanged != null)
                {
                    NameFilterApplianceChanged(this, EventArgs.Empty);
                }

                ApplyNameFilters();
            }
        }

        private void OnNameFilterCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ApplyNameFilters();
        }

        public ObservableCollection<INameFilter> NameFilters { get; private set; }

        public void Push(INameFilter filter)
        {
            Insert(0, filter);
        }

        public void Insert(int pos, INameFilter filter)
        {
            filter.PropertyChanged += OnNameFilterPropertyChanged;

            NameFilters.Insert(pos, filter);
        }

        public bool Remove(INameFilter filter)
        {
            if (filter == myAllCallsFilter)
            {
                return false;
            }

            filter.PropertyChanged -= OnNameFilterPropertyChanged;

            return NameFilters.Remove(filter);
        }

        public void MoveUp(INameFilter filter)
        {
            var idx = NameFilters.IndexOf(filter);
            if (idx == 0)
            {
                return;
            }

            NameFilters.Move(idx, idx - 1);
        }

        public void MoveDown(INameFilter filter)
        {
            var idx = NameFilters.IndexOf(filter);
            if (idx == NameFilters.Count - 1)
            {
                return;
            }

            NameFilters.Move(idx, idx + 1);
        }

        private void ApplyDurationFilter()
        {
            if (Presentation == null)
            {
                return;
            }

            foreach (var flame in Presentation.Flames)
            {
                bool isEmpty = true;

                using (var modifier = flame.CreateModifier())
                {
                    FlamesEventSource.Log.CallVisibilityModifying(flame.GetHashCode());

                    foreach (var call in flame.Activities)
                    {
                        var ret = DurationFilter.IsVisible(call);
                        if (ret == null)
                        {
                            // this filter is no longer relevant
                            modifier.SetVisibilityMask(call, call.VisiblityMask & (~DurationMaskBit));
                            continue;
                        }

                        if (ret == true)
                        {
                            modifier.SetVisibilityMask(call, call.VisiblityMask & (~DurationMaskBit));
                        }
                        else
                        {
                            modifier.SetVisibilityMask(call, call.VisiblityMask | DurationMaskBit);
                        }
                    }

                    if (!modifier.HasVisibilityMaskChanged)
                    {
                        continue;
                    }

                    foreach (var call in flame.Activities)
                    {
                        if (call.VisiblityMask == 0)
                        {
                            isEmpty = false;
                            break;
                        }
                    }
                    FlamesEventSource.Log.CallVisibilityModified(flame.GetHashCode());
                }

                if (Presentation.HideEmptyFlames && isEmpty)
                {
                    flame.Visibility = ContentVisibility.Hidden;
                }
                else if (flame.Visibility == ContentVisibility.Hidden && !isEmpty)
                {
                    flame.Unhide();
                }
            }
        }

        private void ApplyNameFilters()
        {
            if (Presentation == null)
            {
                return;
            }

            if (NameFilters.Where(m => m != myAllCallsFilter && m.IsApplied).Any(m => m.IsShowFilter))
            {
                myAllCallsFilter.PropertyChanged -= OnNameFilterPropertyChanged;
                myAllCallsFilter.IsApplied = true;
                myAllCallsFilter.IsShowFilter = false;
                myAllCallsFilter.PropertyChanged += OnNameFilterPropertyChanged;
            }
            else if (NameFilters.Where(m => m != myAllCallsFilter && m.IsApplied).All(m => !m.IsShowFilter))
            {
                myAllCallsFilter.PropertyChanged -= OnNameFilterPropertyChanged;
                myAllCallsFilter.IsApplied = false;
                myAllCallsFilter.IsShowFilter = true;
                myAllCallsFilter.PropertyChanged += OnNameFilterPropertyChanged;
            }

            foreach (var flame in Presentation.Flames)
            {
                bool isEmpty = true;

                using (var modifier = flame.CreateModifier())
                {
                    FlamesEventSource.Log.CallVisibilityModifying(flame.GetHashCode());

                    foreach (var call in flame.Activities)
                    {
                        foreach (var filter in NameFilters)
                        {
                            var ret = filter.IsVisible(call);
                            if (ret == null)
                            {
                                // this mask is no longer relevant
                                modifier.SetVisibilityMask(call, call.VisiblityMask & (~FilterMaskBit));
                                continue;
                            }

                            if (ret == true)
                            {
                                modifier.SetVisibilityMask(call, call.VisiblityMask & (~FilterMaskBit));
                            }
                            else
                            {
                                modifier.SetVisibilityMask(call, call.VisiblityMask | FilterMaskBit);
                            }

                            break;
                        }
                    }

                    if (!modifier.HasVisibilityMaskChanged)
                    {
                        continue;
                    }

                    foreach (var call in flame.Activities)
                    {
                        if (call.VisiblityMask == 0)
                        {
                            isEmpty = false;
                            break;
                        }
                    }
                    FlamesEventSource.Log.CallVisibilityModified(flame.GetHashCode());
                }

                if (Presentation.HideEmptyFlames && isEmpty)
                {
                    flame.Visibility = ContentVisibility.Hidden;
                }
                else if (flame.Visibility == ContentVisibility.Hidden && !isEmpty)
                {
                    flame.Unhide();
                }
            }
        }
    }
}
