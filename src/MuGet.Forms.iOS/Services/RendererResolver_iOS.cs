using MuGet.Forms.iOS.Services;
using MuGet.Forms.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(RendererResolver_iOS))]
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