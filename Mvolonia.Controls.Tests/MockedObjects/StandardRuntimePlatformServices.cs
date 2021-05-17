using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Platform.Interop;
using Avalonia.Shared.PlatformSupport;

namespace Mvolonia.Controls.Tests.MockedObjects
{
    static class StandardRuntimePlatformServices
    {
        public static void Register(Assembly assembly = null)
        {
            var standardPlatform = new MockStandardRuntimePlatform();
            AssetLoader.RegisterResUriParsers();
            AvaloniaLocator.CurrentMutable
                .Bind<IRuntimePlatform>().ToConstant(standardPlatform)
                .Bind<IAssetLoader>().ToConstant(new AssetLoader(assembly));
//                 .Bind<IDynamicLibraryLoader>().ToConstant(
// #if __IOS__
//                     new IOSLoader()
// #else
//                     RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
//                         ? (IDynamicLibraryLoader)new Win32Loader()
//                         : new UnixLoader()
// #endif
//                 );
        }
    }
}