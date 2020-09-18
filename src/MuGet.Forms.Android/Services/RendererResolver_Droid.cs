using MuGet.Forms.Android.Services;
using MuGet.Forms.UI.Services;
using Xamarin.Forms;

namespace MuGet.Forms.Android.Services
{
    public class RendererResolver_Droid : IRendererResolver
    {
        public object GetRenderer(VisualElement element)
        {
            return Xamarin.Forms.Platform.Android.Platform.GetRenderer(element);
        }

        public bool HasRenderer(VisualElement element)
        {
            return GetRenderer(element) != null;
        }
    }
}