#if IOS

using ObjCRuntime;
using UIKit;

namespace AcademiaDoZe.Presentation.AppMaui;

public class Program
{
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}

#endif