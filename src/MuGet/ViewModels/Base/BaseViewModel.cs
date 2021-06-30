using Acr.UserDialogs;
using MuGet.Services;

namespace MuGet.ViewModels
{
    public abstract class BaseViewModel : MvvmHelpers.BaseViewModel, IViewModel
    {
        protected readonly INuGetService NuGetService;
        protected readonly ILogger Logger;

        protected readonly IUserDialogs Dialogs;

        public BaseViewModel(IBvmConstructor bvmConstructor) : base()
        {
            NuGetService = bvmConstructor.NuGetService;
            Logger = bvmConstructor.Logger;
            Dialogs = bvmConstructor.UserDialogs;
        }

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
        public State State
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