using ObjCRuntime;
using UIKit;

namespace SmartMovieBooking;

public class Program
{
    // Mac Catalyst entry point. The MAUI workload does not auto-generate a
    // Main() for Mac Catalyst when other TFMs (iOS, Android, Windows) are
    // also targeted, so we provide one explicitly. This mirrors the iOS
    // Program.cs and simply delegates to UIKit's application bootstrap,
    // which in turn instantiates the SmartMovieBooking AppDelegate.
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}