
using Android.Content;
using Android.Runtime;
using MuGet.Forms.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Button), typeof(ButtonCustomRenderer))]
namespace MuGet.Forms.Renderers
{
    [Preserve(AllMembers = true)]
    public class ButtonCustomRenderer : ButtonRenderer
    {        
        public ButtonCustomRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control is global::Android.Widget.Button btn)
            {
                // Bug in XForms 4.8, style does not work
                btn.SetAllCaps(false);
            }
        }
    }
}