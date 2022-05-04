using Android.App;
using Android.Runtime;
using MuGet.Forms.Android.Services;
using MuGet.Forms.Services;
using MuGet.Forms.UI;
using MuGet.Forms.UI.Services;
using MuGet.Services;
using System;

namespace MuGet.Forms.Android
{
#if DEBUG
    [Application(Debuggable = true)]
#else
[Application(Debuggable = false)]
#endif
    public class MainApplication : Shiny.ShinyAndroidApplication<ShinyStartup>
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transer)
            : base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            IoC.RegisterSingleton<ILocalise, LocaliseService_Droid>();
            IoC.RegisterSingleton<IEnvironmentService, EnvironmentService_Droid>();
            IoC.RegisterSingleton<IRendererResolver, RendererResolver_Droid>();
            IoC.RegisterSingleton<IHttpHandlerService, HttpHandlerService_Droid>();
            IoC.RegisterSingleton<IThemeService, ThemeService>();
        }
    }
}