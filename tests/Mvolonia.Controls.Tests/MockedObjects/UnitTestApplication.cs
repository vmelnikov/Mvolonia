using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Avalonia.Styling;

namespace Mvolonia.Controls.Tests.MockedObjects
{
   public class UnitTestApplication : Application
    {
        private readonly TestServices _services;

        public UnitTestApplication() : this(null)
        {

        }

        public UnitTestApplication(TestServices services)
        {
            _services = services ?? new TestServices();
            RegisterServices();
        }

        public static new UnitTestApplication Current => (UnitTestApplication)Application.Current;

        public TestServices Services => _services;

        public static IDisposable Start(TestServices services = null)
        {
            var scope = AvaloniaLocator.EnterScope();
            var app = new UnitTestApplication(services);
            AvaloniaLocator.CurrentMutable.BindToSelf<Application>(app);
           // Dispatcher.UIThread.UpdateServices();
            return Disposable.Create(() =>
            {
                scope.Dispose();
                //Dispatcher.UIThread.UpdateServices();
            });
        }

        public override void RegisterServices()
        {
            AvaloniaLocator.CurrentMutable
                .Bind<IAssetLoader>().ToConstant(Services.AssetLoader)
                .Bind<IFocusManager>().ToConstant(Services.FocusManager)
                .Bind<IGlobalClock>().ToConstant(Services.GlobalClock)
                .BindToSelf<IGlobalStyles>(this)
                .Bind<IInputManager>().ToConstant(Services.InputManager)
                .Bind<IKeyboardDevice>().ToConstant(Services.KeyboardDevice?.Invoke())
                .Bind<IKeyboardNavigationHandler>().ToConstant(Services.KeyboardNavigation)
                .Bind<IMouseDevice>().ToConstant(Services.MouseDevice?.Invoke())
                .Bind<IRuntimePlatform>().ToConstant(Services.Platform)
                .Bind<IPlatformRenderInterface>().ToConstant(Services.RenderInterface)
                .Bind<IFontManagerImpl>().ToConstant(Services.FontManagerImpl)
                .Bind<ITextShaperImpl>().ToConstant(Services.TextShaperImpl)
                .Bind<IPlatformThreadingInterface>().ToConstant(Services.ThreadingInterface)
                .Bind<IScheduler>().ToConstant(Services.Scheduler)
                .Bind<IStyler>().ToConstant(Services.Styler)
                .Bind<IWindowingPlatform>().ToConstant(Services.WindowingPlatform)
                .Bind<PlatformHotkeyConfiguration>().ToSingleton<PlatformHotkeyConfiguration>();
            var styles = Services.Theme?.Invoke();

            if (styles != null)
            {
                Styles.AddRange(styles);
            }
        }
    }
}