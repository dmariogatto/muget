using System;
using AiForms.Renderers;
using AiForms.Renderers.Droid;
using Android.Content;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Support.V7.Widget;
using Android.Runtime;
using MuGet.Forms.Android.Renderers;

// Workaroud for https://github.com/muak/AiForms.SettingsView/issues/73

[assembly: ExportRenderer(typeof(SettingsView), typeof(SettingsViewCustomRenderer))]
namespace MuGet.Forms.Android.Renderers
{
    /// <summary>
    /// Settings view renderer.
    /// </summary>
    /// 
    [Preserve(AllMembers = true)]
    public class SettingsViewCustomRenderer : ViewRenderer<SettingsView, RecyclerView>
    {
        Page _parentPage;
        SettingsViewRecyclerAdapter _adapter;
        LinearLayoutManager _layoutManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AiForms.Renderers.Droid.SettingsViewRenderer"/> class.
        /// </summary>
        public SettingsViewCustomRenderer(Context context) : base(context)
        {
            AutoPackage = false;
        }

        /// <summary>
        /// Ons the element changed.
        /// </summary>
        /// <param name="e">E.</param>
        protected override void OnElementChanged(ElementChangedEventArgs<SettingsView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {

                var recyclerView = new RecyclerView(Context);
                _layoutManager = new LinearLayoutManager(Context);
                recyclerView.SetLayoutManager(_layoutManager);

                SetNativeControl(recyclerView);

                Control.Focusable = false;
                Control.DescendantFocusability = DescendantFocusability.AfterDescendants;

                UpdateBackgroundColor();
                UpdateRowHeight();

                _adapter = new SettingsViewRecyclerAdapter(Context, e.NewElement, recyclerView);
                Control.SetAdapter(_adapter);

                Element elm = Element;
                while (elm != null)
                {
                    elm = elm.Parent;
                    if (elm is Page)
                    {
                        break;
                    }
                }

                _parentPage = elm as Page;
                _parentPage.Appearing += ParentPageAppearing;
            }
        }

        void ParentPageAppearing(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => _adapter?.DeselectRow());
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);

            if (!changed) return;

            var startPos = _layoutManager.FindFirstCompletelyVisibleItemPosition();
            var endPos = _layoutManager.FindLastCompletelyVisibleItemPosition();

            int totalH = 0;
            for (var i = startPos; i <= endPos; i++)
            {
                var child = _layoutManager.GetChildAt(i);

                if (child == null) return;

                totalH += _layoutManager.GetChildAt(i).Height;
            }
            Element.VisibleContentHeight = Context.FromPixels(Math.Min(totalH, Control.Height));
        }

        /// <summary>
        /// Ons the element property changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == SettingsView.SeparatorColorProperty.PropertyName)
            {
                _adapter.NotifyDataSetChanged();
            }
            else if (e.PropertyName == SettingsView.BackgroundColorProperty.PropertyName)
            {
                UpdateBackgroundColor();
            }
            else if (e.PropertyName == TableView.RowHeightProperty.PropertyName)
            {
                UpdateRowHeight();
            }
            else if (e.PropertyName == SettingsView.UseDescriptionAsValueProperty.PropertyName)
            {
                _adapter.NotifyDataSetChanged();
            }
            else if (e.PropertyName == SettingsView.SelectedColorProperty.PropertyName)
            {
                //_adapter.NotifyDataSetChanged();
            }
            else if (e.PropertyName == SettingsView.ShowSectionTopBottomBorderProperty.PropertyName)
            {
                _adapter.NotifyDataSetChanged();
            }
            else if (e.PropertyName == TableView.HasUnevenRowsProperty.PropertyName)
            {
                _adapter.NotifyDataSetChanged();
            }
            else if (e.PropertyName == SettingsView.ScrollToTopProperty.PropertyName)
            {
                UpdateScrollToTop();
            }
            else if (e.PropertyName == SettingsView.ScrollToBottomProperty.PropertyName)
            {
                UpdateScrollToBottom();
            }
        }

        void UpdateRowHeight()
        {
            if (Element.RowHeight == -1)
            {
                Element.RowHeight = 60;
            }
            else
            {
                _adapter?.NotifyDataSetChanged();
            }
        }

        void UpdateScrollToTop()
        {
            if (Element.ScrollToTop)
            {
                _layoutManager.ScrollToPosition(0);
                Element.ScrollToTop = false;
            }
        }

        void UpdateScrollToBottom()
        {
            if (Element.ScrollToBottom)
            {
                if (_adapter != null)
                {
                    _layoutManager.ScrollToPosition(_adapter.ItemCount - 1);
                }
                Element.ScrollToBottom = false;
            }
        }

        new void UpdateBackgroundColor()
        {
            if (Element.BackgroundColor != Xamarin.Forms.Color.Default)
            {
                Control.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
            }
        }

        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <returns>The dispose.</returns>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _parentPage.Appearing -= ParentPageAppearing;
                _adapter?.Dispose();
                _adapter = null;
                _layoutManager?.Dispose();
                _layoutManager = null;
            }
            base.Dispose(disposing);
        }
    }
}
