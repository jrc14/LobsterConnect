using Microsoft.Windows.AppLifecycle;
using System.Data.SqlTypes;
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
        // The intention is to use the key to keep track of whether there is another instance running, because if there 
        // is, we want to shut down without doing anything more than passing URL arguments over to the running
        // instance, and then exiting.  Or if there is not another instance running, then we want to make sure that this
        // instance will, when activated, look for a URL argument (to open a session in 'manage session' popup).

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
                            Model.Logger.LogMessage(Model.Logger.Level.INFO, "AppInstance.Activated Handler", "found URI: " + u);

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

        if (_InstanceAlreadyRunning) // The app has detected an instance that is already running, so it will now try to close, to implement 'single instance' behaviour
        {
            Microsoft.Maui.Controls.Application.Current?.CloseWindow(Microsoft.Maui.Controls.Application.Current.MainPage.Window);
            return;
        }

        try
        {
            // In the ideal world, MAUI would make it so 'args' contained the right data,  But it does not, so there is
            // this stupid work-around.
            var bugFreeArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (bugFreeArgs.Kind == ExtendedActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs protocol = bugFreeArgs.Data as ProtocolActivatedEventArgs;
                if(protocol!=null && protocol.Uri!=null)
                {
                    string u = protocol.Uri.ToString();

                    Model.Logger.LogMessage(Model.Logger.Level.INFO, "App.OnLaunched", "found URI: " + u);

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


    /// <summary>
    /// Attempt to make the app window visible
    /// </summary>
    private void ShowWindow()
    {
        Model.DispatcherHelper.RunAsyncOnUI(() =>
        {
            Microsoft.Maui.MauiWinUIWindow window = Microsoft.Maui.Controls.Application.Current.Windows.Last().Handler.PlatformView as Microsoft.Maui.MauiWinUIWindow;
            window.Activate();
           

            nint hWnd = window.WindowHandle;
            Microsoft.UI.WindowId winId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(winId);

            appWindow.Show();

            // Try to make the window appear as the top window, briefly.
            (appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter).IsAlwaysOnTop = true;
            (appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter).IsAlwaysOnTop = false;

        });
    }
}

