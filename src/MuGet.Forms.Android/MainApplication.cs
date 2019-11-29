using System;
using Android.App;
using Android.Runtime;
using Plugin.CurrentActivity;

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
            CrossCurrentActivity.Current.Init(this);
            //Shiny.AndroidShinyHost.Init(this, new ShinyStartup());
        }
    }
}