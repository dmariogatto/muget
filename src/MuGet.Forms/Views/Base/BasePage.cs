using MuGet.Forms.ViewModels;
using Xamarin.Forms;

namespace MuGet.Forms.Views
{
    public abstract class BasePage<T> : ContentPage where T : BaseViewModel
    {
        public T ViewModel => BindingContext as T;

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (Parent != null)
            {
                ViewModel.OnCreate();
            }
            else
            {
                ViewModel.OnDestory();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ViewModel.OnDisappearing();
        }
    }
}
