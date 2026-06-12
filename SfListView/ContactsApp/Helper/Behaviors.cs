using Syncfusion.Maui.ListView;
using Syncfusion.Maui.Popup;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContactsApp
{
    public class IndexTapBehavior : Behavior<SfListView>
    {
        public SfListView TargetListView { get; set; }

        protected override void OnAttachedTo(SfListView bindable)
        {
            bindable.ItemTapped += OnIndexTapped;
            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(SfListView bindable)
        {
            bindable.ItemTapped -= OnIndexTapped;
            base.OnDetachingFrom(bindable);
        }

        private void OnIndexTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            if (TargetListView == null) return;

            if (e.DataItem is string letter)
            {
                var groups = TargetListView.DataSource.Groups;

                foreach (var group in groups)
                {
                    if (group.Key.ToString() == letter)
                    {
                        TargetListView.ScrollTo(group, ScrollToPosition.Start, true);
                        break;
                    }
                }
            }
        }
    }

    public class ItemTapBehavior : Behavior<SfListView>
    {
        public SfPopup Popup { get; set; }

        protected override void OnAttachedTo(SfListView bindable)
        {
            bindable.ItemTapped += OnItemTapped;
            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(SfListView bindable)
        {
            bindable.ItemTapped -= OnItemTapped;
            base.OnDetachingFrom(bindable);
        }

        private void OnItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            if (Popup == null) return;

            if (e.DataItem is Contact contact)
            {
                Popup.BindingContext = contact;
                Popup.IsOpen = true;
            }
        }
    }

    public class SearchBarBehavior : Behavior<SearchBar>
    {
        public SfListView ListView { get; set; }

        protected override void OnAttachedTo(SearchBar bindable)
        {
            bindable.TextChanged += OnTextChanged;
            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(SearchBar bindable)
        {
            bindable.TextChanged -= OnTextChanged;
            base.OnDetachingFrom(bindable);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ListView == null) return;

            ListView.DataSource.Filter = (obj) =>
            {
                var contact = obj as Contact;

                return string.IsNullOrEmpty(e.NewTextValue) ||
                       contact?.Name?.ToLower().Contains(e.NewTextValue.ToLower()) == true;
            };

            ListView.DataSource.RefreshFilter();
        }
    }

    public class ClosePopupBehavior : Behavior<View>
    {
        public static readonly BindableProperty PopupProperty =
            BindableProperty.Create(nameof(Popup), typeof(SfPopup), typeof(ClosePopupBehavior));

        public SfPopup Popup
        {
            get => (SfPopup)GetValue(PopupProperty);
            set => SetValue(PopupProperty, value);
        }

        private TapGestureRecognizer tapGesture;

        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);

            tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnTapped;

            bindable.GestureRecognizers.Add(tapGesture);
        }

        protected override void OnDetachingFrom(View bindable)
        {
            base.OnDetachingFrom(bindable);

            if (tapGesture != null)
            {
                tapGesture.Tapped -= OnTapped;
                bindable.GestureRecognizers.Remove(tapGesture);
            }
        }

        private void OnTapped(object sender, EventArgs e)
        {
            if (Popup != null)
            {
                Popup.IsOpen = false;
            }
        }
    }
}
