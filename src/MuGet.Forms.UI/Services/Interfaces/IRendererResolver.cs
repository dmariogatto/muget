using Xamarin.Forms;

namespace MuGet.Forms.UI.Services
{
    public interface IRendererResolver
    {
        object GetRenderer(VisualElement element);
        bool HasRenderer(VisualElement element);
    }
}