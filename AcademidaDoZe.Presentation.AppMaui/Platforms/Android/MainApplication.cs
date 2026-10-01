using Android.App;
using Android.Runtime;
using AcademiaDoZe.Presentation.AppMaui;

namespace AcademidaDoZe.Presentation.AppMaui
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
