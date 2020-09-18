using MuGet.ViewModels;
using System;
using Xamarin.Forms;

namespace MuGet.Forms.UI.Views
{
    public class BasePage<T> : ContentPage where T : BaseViewModel
    {
        public T ViewModel => BindingContext as T;

        public BasePage() : base()
        {
            var vm = IoC.ResolveViewModel<T>();
            BindingContext = vm ?? throw new ArgumentNullException(typeof(T).Name);

            ViewModel.OnCreate();
        }

        ~BasePage()
        {
            ViewModel.OnDestory();
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
