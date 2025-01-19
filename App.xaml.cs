using LobsterConnect.Model;

namespace LobsterConnect;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

        App.ApplicationSuspended = false;


        DispatcherHelper.Initialise(Environment.CurrentManagedThreadId, this.Dispatcher); // because I am pretty sure that Window.Current exists at this stage.

        if (!DispatcherHelper.UIDispatcherHasThreadAccess)
        {
            Logger.LogMessage(Logger.Level.ERROR,"App ctor","Horrible error; this code should only be run on the UI thread!");
        }

        Logger.LogMessage(Logger.Level.DEBUG, "App ctor", "about to create main page");

        MainPage = new AppShell();
	}

    public static bool ApplicationSuspended = false; // used to tell our various timers to shut themselves down.

    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);

        window.Created += (s, e) =>
        {
            ApplicationSuspended = false;
        };

        window.Activated += (s, e) =>
        {
            //ApplicationSuspended = false;
        };

        window.Deactivated += (s, e) =>
        {
            //ApplicationSuspended = true;
        };

        window.Stopped += (s, e) =>
        {
            ApplicationSuspended = true;
        };

        window.Resumed += (s, e) =>
        {
            ApplicationSuspended = false;
        };


        return window;
    }

    private static string _appFolder = null;
    public static string AppFolder
    {
        get
        {
            if (_appFolder == null)
            {
#if __ANDROID__
                _appFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
#else
                _appFolder = Microsoft.Maui.Storage.FileSystem.AppDataDirectory;
#endif
                Logger.LogMessage(Logger.Level.INFO,"App.AppFolder get accessor", "path is " + _appFolder);
            }

            return _appFolder;
        }
    }

    private static string _programFolder = null;
    public static string ProgramFolder
    {
        get
        {
            if (_programFolder == null)
            {
                string folderName = System.IO.Path.Combine(AppFolder, "lobsterconnect");

                if (System.IO.Directory.Exists(folderName))
                {
                    _programFolder = folderName;
                }
                else
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(folderName);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "App.ProgramFolder get accessor", ex, "when trying to create folder " + folderName);
                    }
                    _programFolder = folderName;
                }

                Logger.LogMessage(Logger.Level.INFO, "App.ProgramFolder get accessor","path is " + _programFolder);

                return _programFolder;
            }
            else
                return _programFolder;
        }
    }

}
