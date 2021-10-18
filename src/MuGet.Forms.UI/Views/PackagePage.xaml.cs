using MuGet.Forms.UI.Controls;
using MuGet.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xamarin.Forms;

namespace MuGet.Forms.UI.Views
{
    public partial class PackagePage : BasePage<PackageViewModel>
    {
        private View _packageDetailsView, _dependanciesView, _versionsView;

        public PackagePage() : base()
        {
            InitializeComponent();

            TopTabs.ItemsSource = new List<string>()
            {
                Localisation.Resources.Details,
                Localisation.Resources.Dependencies,
                Localisation.Resources.Versions
            };
        }

        public string PackageId
        {
            get => ViewModel.PackageId;
            set
            {
                ViewModel.PackageId = WebUtility.UrlDecode(value);
            }
        }

        public string Version
        {
            get => ViewModel.Version;
            set
            {
                ViewModel.Version = WebUtility.UrlDecode(value);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Navigation.NavigationStack.LastOrDefault() == this &&
                Navigation.NavigationStack.Count > 1 &&
                Navigation.NavigationStack[Navigation.NavigationStack.Count - 2] is ContentPage previous)
            {
                BackButton.Text = previous.Title;
                CloseButton.IsVisible = Navigation.NavigationStack.Count > 2;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private void SelectedTabIndexChanged(object sender, IndexChangedArgs e)
        {
            var view = e.NewIndex switch
            {
                0 => _packageDetailsView ??= CreatePackageDetailsView(),
                1 => _dependanciesView ??= CreateDependanciesView(),
                2 => _versionsView ??= CreateVersionsView(),
                _ => throw new InvalidOperationException()
            };

            view.IsVisible = true;

            if (!TabGridContent.Children.Contains(view))
                TabGridContent.Children.Add(view);

            foreach (var v in TabGridContent.Children.Where(i => i != view))
                v.IsVisible = false;
        }

        private void BackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void CloseClicked(object sender, EventArgs e)
        {
            Navigation.PopToRootAsync();
        }

        private View CreatePackageDetailsView() =>
            new Xamarin.Forms.ScrollView()
            {
                Padding = new Thickness(0, 3, 0, 3),
                Content = new PackageDetailsView()
                {
                    BindingContext = this.BindingContext
                }
            };

        private View CreateDependanciesView() =>
            new DependanciesView() { BindingContext = this.BindingContext };

        private View CreateVersionsView() =>
            new VersionsView() { BindingContext = this.BindingContext };
    }
}