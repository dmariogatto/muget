using MuGet.Forms.ViewModels;
using Shiny.Settings;
using System;
using Xamarin.Forms;

namespace MuGet.Forms.Views
{
    public class BasePage<T> : ContentPage where T : BaseViewModel
    {
        public T ViewModel => BindingContext as T;

        public BasePage() : base()
        {
            var vmType = typeof(T);

            var vm = Shiny.ShinyHost.Container.GetService(vmType) as T;
            var settings = Shiny.ShinyHost.Container.GetService(typeof(ISettings)) as ISettings;

            if (vm == null)
                throw new ArgumentNullException(nameof(vmType.Name));
            if (settings == null)
                throw new ArgumentNullException(nameof(ISettings));

            settings.UnBind(vm);
            BindingContext = vm;
        }

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
