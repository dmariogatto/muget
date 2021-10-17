using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace MuGet.Forms.UI.Controls
{
    public class TopTabBar : Grid
    {
        public static readonly BindableProperty ItemsSourceProperty =
          BindableProperty.Create(
              propertyName: nameof(ItemsSource),
              returnType: typeof(IList),
              declaringType: typeof(TopTabBar),
              defaultValue: null,
              propertyChanged: OnItemsSourceChanged);

        public static readonly BindableProperty TabTemplateProperty =
          BindableProperty.Create(
              propertyName: nameof(TabTemplate),
              returnType: typeof(DataTemplate),
              declaringType: typeof(TopTabBar),
              defaultValue: null,
              propertyChanged: null);

        public static readonly BindableProperty SelectedIndexProperty =
          BindableProperty.Create(
              propertyName: nameof(SelectedIndex),
              returnType: typeof(int),
              declaringType: typeof(TopTabBar),
              defaultValue: -1,
              propertyChanged: OnSelectedIndexChanged);

        public static readonly BindableProperty SelectedItemProperty =
          BindableProperty.Create(
              propertyName: nameof(SelectedItem),
              returnType: typeof(object),
              declaringType: typeof(TopTabBar),
              defaultValue: default,
              propertyChanged: OnSelectedItemChanged);

        public static readonly BindableProperty PrimaryColorProperty =
          BindableProperty.Create(
              propertyName: nameof(PrimaryColor),
              returnType: typeof(Color),
              declaringType: typeof(TopTabBar),
              defaultValue: Color.Default,
              propertyChanged: null);

        private List<ContentView> _currentViews = new List<ContentView>();
        private BoxView _bottomBorder;
        private BoxView _selectedUnderline;

        public TopTabBar()
        {
            const int bottomUnderlineHeight = 5;

            HeightRequest = 48;

            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.Start;

            RowSpacing = 0;
            ColumnSpacing = 1;

            _bottomBorder = new BoxView()
            {
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center
            };

            _selectedUnderline = new BoxView()
            {
                HeightRequest = bottomUnderlineHeight,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false
            };

            _bottomBorder.SetBinding(BoxView.BackgroundColorProperty, new Binding(nameof(PrimaryColor), source: this));
            _selectedUnderline.SetBinding(BoxView.BackgroundColorProperty, new Binding(nameof(PrimaryColor), source: this));

            RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
            RowDefinitions.Add(new RowDefinition() { Height = bottomUnderlineHeight });

            Children.Add(_bottomBorder, 0, 1);
            Children.Add(_selectedUnderline, 0, 1);

            _selectedUnderline.SizeChanged += (sender, args) => UpdateSelectedUnderlinePosition();

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IsVisible) && IsVisible)
                {
                    InvalidateLayout();
                }
                else if (args.PropertyName == nameof(Width) || args.PropertyName == nameof(SelectedIndex))
                {
                    UpdateSelectedUnderlinePosition();
                }
            };
        }

        public event EventHandler<IndexChangedArgs> IndexChanged;
        public event EventHandler<ItemChangedArgs> ItemChanged;

        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public DataTemplate TabTemplate
        {
            get => (DataTemplate)GetValue(TabTemplateProperty);
            set => SetValue(TabTemplateProperty, value);
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public object SelectedItem
        {
            get => (object)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public Color PrimaryColor
        {
            get => (Color)GetValue(PrimaryColorProperty);
            set => SetValue(PrimaryColorProperty, value);
        }

        private void Redraw()
        {
            foreach (var c in _currentViews)
            {
                if (c.Content?.BindingContext is INotifyPropertyChanged item)
                    item.PropertyChanged -= ViewPropertyChanged;
                Children.Remove(c);
            }

            _currentViews.Clear();
            ColumnDefinitions.Clear();

            var count = ItemsSource?.Count ?? -1;
            for (var i = 0; i < count; i++)
            {
                var idx = i;
                var item = ItemsSource[idx];

                var contentView = new ContentView()
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                };

                var view = TabTemplate is not DataTemplateSelector tabItemDataTemplate
                    ? (View)(TabTemplate?.CreateContent() ?? throw new NullReferenceException())
                    : (View)tabItemDataTemplate.SelectTemplate(item, this).CreateContent();

                view.BindingContext = item;
                contentView.Content = view;

                var tg = new TapGestureRecognizer();
                tg.Tapped += (sender, args) => { SelectedIndex = idx; };
                contentView.GestureRecognizers.Add(tg);

                contentView.Content.PropertyChanged += ViewPropertyChanged;

                SetColumn(contentView, idx);
                SetRowSpan(contentView, 2);

                _currentViews.Add(contentView);
                ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            }

            foreach (var c in _currentViews)
            {
                Children.Add(c);
            }

            SelectedIndex = _currentViews.Any() ? 0 : -1;

            SetColumnSpan(_bottomBorder, _currentViews.Count);
            SetColumnSpan(_selectedUnderline, _currentViews.Count);
        }

        private void ViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is View view && e.PropertyName == nameof(View.HorizontalOptions))
            {
                var idx = _currentViews.Select(i => i.Content).ToList().IndexOf(view);
                if (idx == SelectedIndex)
                    UpdateSelectedUnderlinePosition();
            }
        }

        private void UpdateSelectedUnderlinePosition()
        {
            var item = SelectedIndex >= 0 && SelectedIndex < _currentViews.Count
                ? _currentViews[SelectedIndex].Content
                : null;

            if (item != null && item.Width > 0)
            {
                var columnWidth = Width / _currentViews.Count;
                var tranX = columnWidth * SelectedIndex;

                switch (item.HorizontalOptions.Alignment)
                {
                    case LayoutAlignment.End:
                        tranX += columnWidth - item.Width;
                        break;
                    case LayoutAlignment.Center:
                    case LayoutAlignment.Fill:
                        tranX += (columnWidth / 2d) - (item.Width / 2d);
                        break;
                }

                _selectedUnderline.CancelAnimations();
                _ = _selectedUnderline.TranslateTo(tranX, 0);
            }
        }

        private void SelectedIndexChanged(int oldIdx, int newIdx)
        {
            object getItem(int idx) => idx >= 0 && idx < ItemsSource.Count ? ItemsSource[idx] : null;

            var oldSelected = getItem(oldIdx);
            var newSelected = getItem(newIdx);

            SelectedItem = newSelected;

            if (newSelected != null)
            {
                var item = _currentViews[newIdx].Content;

                _selectedUnderline.SetBinding(BoxView.WidthRequestProperty, new Binding(nameof(Width), source: item));
                _selectedUnderline.IsVisible = true;
            }
            else
            {
                _selectedUnderline.RemoveBinding(BoxView.WidthRequestProperty);
                _selectedUnderline.TranslationX = 0;
                _selectedUnderline.IsVisible = false;
            }

            IndexChanged?.Invoke(this, new IndexChangedArgs(oldIdx, newIdx));
            ItemChanged?.Invoke(this, new ItemChangedArgs(oldSelected, newSelected));
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => Redraw();

        private static void OnItemsSourceChanged(BindableObject sender, object oldValue, object newValue)
        {
            var tabBar = (TopTabBar)sender;

            if (oldValue is INotifyCollectionChanged oldCollection)
                oldCollection.CollectionChanged -= tabBar.CollectionChanged;

            if (newValue is INotifyCollectionChanged newCollection)
                newCollection.CollectionChanged += tabBar.CollectionChanged;

            tabBar.Redraw();
        }

        private static void OnSelectedIndexChanged(BindableObject sender, object oldValue, object newValue)
        {
            var tabBar = (TopTabBar)sender;

            var oldIdx = (int)oldValue;
            var newIdx = (int)newValue;

            tabBar.SelectedIndexChanged(oldIdx, newIdx);
        }

        private static void OnSelectedItemChanged(BindableObject sender, object oldValue, object newValue)
        {
            var tabBar = (TopTabBar)sender;

            var newIdx = tabBar.ItemsSource?.IndexOf(newValue) ?? -1;

            if (newIdx != tabBar.SelectedIndex)
                tabBar.SelectedIndex = newIdx;
        }
    }
}
