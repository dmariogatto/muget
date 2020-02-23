using MuGet.Forms.Services;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.StateSquid;

namespace MuGet.Forms.ViewModels
{
    public class BaseViewModel : MvvmHelpers.BaseViewModel
    {
        protected readonly INuGetService NuGetService;
        protected readonly ILogger Logger;

        private NavigationPage _navPage => Application.Current.MainPage as NavigationPage;

        public BaseViewModel(INuGetService nuGetService, ILogger logger) : base()
        {
            NuGetService = nuGetService;
            Logger = logger;
        }

        protected Task DisplayAlert(string title, string message, string cancel) =>
            _navPage.DisplayAlert(title, message, cancel);

        protected Task<bool> DisplayAlert(string title, string message, string accept, string cancel) =>
            _navPage.DisplayAlert(title, message, accept, cancel);

        protected Task<string> DisplayAction(string title, string cancel, string destruction, params string[] buttons) =>
            _navPage.DisplayActionSheet(title, cancel, destruction, buttons);

        public virtual void OnCreate()
        {
        }

        public virtual void OnAppearing()
        {
        }

        public virtual void OnDisappearing()
        {
        }

        public virtual void OnDestory()
        {
        }

        private State _currentState;
        public State CurrentState
        {
            get => _currentState;
            set => SetProperty(ref _currentState, value);
        }

#if DEBUG
        private bool _isDevelopment = true;
#else
        private bool _isDevelopment = false;
#endif
        public bool IsDevelopment
        {
            get => _isDevelopment;
            set => OnPropertyChanged(nameof(IsDevelopment));
        }
    }
}
