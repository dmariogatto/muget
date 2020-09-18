using MuGet.Forms.UI.Services;
using Xamarin.Forms;

namespace MuGet.Forms.iOS.Services
{
    public class RendererResolver_iOS : IRendererResolver
    {
        public object GetRenderer(VisualElement element)
        {
            return Xamarin.Forms.Platform.iOS.Platform.GetRenderer(element);
        }

        public bool HasRenderer(VisualElement element)
        {
            return GetRenderer(element) != null;
        }
    }
}