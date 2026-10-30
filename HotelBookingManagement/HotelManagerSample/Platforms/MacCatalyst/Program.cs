using ObjCRuntime;
using UIKit;

namespace HotelManagerSample;

/// <summary>
/// Native entry point for the Mac Catalyst build. UIKit calls
/// <see cref="Main"/> first; everything else is delegated to MAUI.
/// </summary>
public class Program
{
    /// <summary>UIKit entry point. Pass-through to UIApplication.Main,
    /// using <see cref="AppDelegate"/> as the host application class.
    /// </summary>
    /// <param name="args">Native command-line arguments forwarded by
    /// UIKit. Unused by MAUI itself.</param>
    static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
