using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LobsterConnect.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        AppInstance keyInstance = AppInstance.FindOrRegisterForKey("LobsterConnect_AppInstance"); // the key string can be any old thing.

        if (keyInstance.IsCurrent) // This is the only LobsterConnect_AppInstance running
        {
            keyInstance.Activated += (object sender, AppActivationArguments e) =>
            {
                try
                {
                    ShowWindow();

                    if (e.Kind == ExtendedActivationKind.Protocol)
                    {
                        ProtocolActivatedEventArgs protocol = e.Data as ProtocolActivatedEventArgs;
                        if (protocol != null && protocol.Uri != null)
                        {
                            string u = protocol.Uri.ToString();
                            Model.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                            {

#pragma warning disable 4014 // the method below will run asynchronously, but I am fine to let the method exit in the meantime
                                VM.MainViewModel.Instance.OpenSessionFromUrl(u);
#pragma warning restore 4014
                            });
                        }
                    }
                }
                catch (Exception )
                {
                    
                }
            };
        }
        else
        {
            try
            {
                // There's already an instance running.  The current (new) instance has to stop, before it
                // causes problems by messing with shared resources (the store etc.).
                _InstanceAlreadyRunning = true;

                // It will try to activate the instance that's already running.
                AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
                keyInstance.RedirectActivationToAsync(args).AsTask().Wait();

                // When InitializeComponent is called below, it will eventually result in a call to OnLaunched, which will
                // check _InstanceAlreadyRunning, and close the app.
            }
            catch (Exception )
            {
            }
        }

        this.InitializeComponent();
    }

    static bool _InstanceAlreadyRunning = false; // set this to true, and OnLaunched will try to stop the current app launch

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        if (_InstanceAlreadyRunning)
        {
            Microsoft.Maui.Controls.Application.Current?.CloseWindow(Microsoft.Maui.Controls.Application.Current.MainPage.Window);
            return;
        }

        try
        {
            var bugFreeArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (bugFreeArgs.Kind == ExtendedActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs protocol = bugFreeArgs.Data as ProtocolActivatedEventArgs;
                if(protocol!=null && protocol.Uri!=null)
                {
                    string u = protocol.Uri.ToString();
                    Model.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {

#pragma warning disable 4014 // the method below will run asynchronously, but I am fine to let the method exit in the meantime
                        VM.MainViewModel.Instance.OpenSessionFromUrl(u);
#pragma warning restore 4014
                    });
                }    
            }
        }
        catch (Exception )
        {
        }
    }
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();


    private void ShowWindow() // for reasons I do not yet understand, this method doesn't seem to reliably bring the app window to the front.
    {
        Model.DispatcherHelper.RunAsyncOnUI(() =>
        {
            Microsoft.Maui.MauiWinUIWindow window = Microsoft.Maui.Controls.Application.Current.Windows.Last().Handler.PlatformView as Microsoft.Maui.MauiWinUIWindow;
            window.Activate();

            nint hWnd = window.WindowHandle;
            Microsoft.UI.WindowId winId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(winId);

            appWindow.Show();
        });
    }
}

