using System;

namespace MuGet.ViewModels
{
    public interface IViewModel
    {
        void OnCreate();
        void OnAppearing();
        void OnDisappearing();
        void OnDestory();
    }
}
