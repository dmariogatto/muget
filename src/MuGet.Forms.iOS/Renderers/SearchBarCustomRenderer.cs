using Foundation;
using MuGet.Forms.iOS.Renderers;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SearchBar), typeof(SearchBarCustomRenderer))]
namespace MuGet.Forms.iOS.Renderers
{
    [Preserve(AllMembers = true)]
    public class SearchBarCustomRenderer : SearchBarRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.AutocorrectionType = UITextAutocorrectionType.No;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == SearchBar.TextProperty.PropertyName ||
                e.PropertyName == SearchBar.IsFocusedProperty.PropertyName)
            {
                Control.ShowsCancelButton = string.IsNullOrEmpty(Control.Text) && Element.IsFocused;
            }
        }
    }
}