using Xamarin.Forms;

namespace MuGet.Forms.Services
{
    public interface IRendererResolver
    {
        object GetRenderer(VisualElement element);
        bool HasRenderer(VisualElement element);
    }
}
