using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using MuGet.Forms.UI;
using MuGet.Forms.UI.Views;
using Shiny;
using System;
using Xamarin.Forms;

namespace MuGet.Forms.Android
{
    [Activity(
        Label = "MuGet",
        Icon = "@mipmap/icon",
        RoundIcon = "@mipmap/icon_round",
        Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { Intent.ActionView },
                  Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
                  DataScheme = "muget")]
    [IntentFilter(new[] { Intent.ActionView },
                  Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
                  DataScheme = "muget",
                  DataHost = "package")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Acr.UserDialogs.UserDialogs.Init(this);
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);
            AiForms.Renderers.Droid.SettingsViewInit.Init();

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            Sharpnado.Presentation.Forms.Droid.SharpnadoInitializer.Initialize();

            LoadApplication(new App());
            
            this.ShinyOnCreate();

            ProcessIntent(Intent);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            ProcessIntent(intent);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            this.ShinyRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void ProcessIntent(Intent intent)
        {
            if (intent?.Action != null &&
                intent?.Data != null)
            {
                var uri = intent.Data;
                var host = uri.Host;
                var pathSegs = uri.PathSegments;
                if (string.Equals(host, "package", StringComparison.OrdinalIgnoreCase))
                {
                    var packageId = pathSegs.Count > 0 ? pathSegs[0] : string.Empty;
                    var version = pathSegs.Count > 1 ? pathSegs[1] : string.Empty;

                    if (!string.IsNullOrEmpty(packageId) &&
                        Xamarin.Forms.Application.Current.MainPage is NavigationPage navPage)
                    {
                        var packagePage = new PackagePage();
                        packagePage.PackageId = packageId;

                        if (!string.IsNullOrEmpty(version))
                            packagePage.Version = version;
                        
                        Device.InvokeOnMainThreadAsync(() => navPage.PushAsync(packagePage));
                    }
                }
            }
        }
    }
}