using LobsterConnect.Model;

namespace LobsterConnect;

/// <summary>
/// The LobsterConnect application.  The app is a thick client, running on Windows, iOS, MacOS and Android devices
/// which presents functionality for proposing gaming sessions at gaming events (evenings, conventions, etc.),
/// and for signing up to sessions organised by other players.
/// It relies on a cloud sync service, provided by an Azure Function application, LobsterConBackEnd.
/// 
/// The app's transient state is held in a singleton viewmodel, MainViewModel.Instance.  This maintains, and
/// exposes to UI binding:
///  - The current GamingEvent (a thing like LobsterCon XXVIII - an occasion and venue when games will be played),
///    and a list of alternative gaming events that can be selected instead (the app will only show one gaming
///    event at a time)
///  - The list of Sessions (occasions on which a certain game will be played at a certain time, organised by a 
///    certain person, at the current gaming event).  These when first created will be put in the OPEN state
///    (meaning that people are invited to sign up).  The organiser can later put a session into the FULL state
///    (no further sign-ups are invited, and the game will be played as planned) or the ABANDONED state (the
///    game won't be played after all).
///  - The list of Persons; persons are named individuals who can organise gaming sessions and sign up to them.
///    The id of a person is a 'user handle', which is a short string entered by the user and intended to identify
///    them to other participants at a gaming event (perhaps being written on a lanyard badge for instance).
///  - The list of Games; these are things that people can play.  The app is initially set up with a list
///    of 500 games, but if you want to organise a session of a game that is not on the list, you can create a new
///    one.
///    
/// The app's durable state is persisted into a journal object; to set the viewmodel up, you replay the journal.
/// Each journal entry describes a Create or Update operation on one of the entities mentioned above (GamingEvent,
/// Game, Session or Person).  There is an additional entity type, SignUp, that a journal entry can represent - 
/// and this supports operations Create and Delete.  In the viewmodel, creating and deleting signups consists
/// of modifying the 'SignUps' member of a Session object (which is a string containing a comma-separated
/// list of user handles).
/// Note that the Delete operation exists only for SignUps; other entity types cannot be deleted, though they
/// can be updated (and if you update an entity by setting its IsActive parameter to False, then it will be
/// largely hidden from the UI).  The design works this way, to make it harder for badly sequenced journal entries
/// to break referential integrity.  There is an exceptional case for Person entities; if a user exercises their
/// 'right to be forgotten' then all references to that user will be replaced by a special user, '#deleted'.
/// 
/// The app keeps a local copy of its journal in a file, and on startup it replays this file to set up the
/// viewmodel.  The cloud sync service, implemented in LobsterConBackEnd, takes care of keeping the journals
/// synchronised between all the devices running the LobsterConnect app.  Journal records include:
///  - An installation id, representing the device on which the UI was used to perform the action;
///  - A local sequence number, specific to the local device, and going up by one each time an action is journalled
///    on that local device;
///  - A cloud sequence number, which is allocated by the cloud sync service.  Every journal entry gets one of these
///    numbers, and the right way to populate a viewmodel is by replaying them all in ascending order of cloud
///    sequence number.  Exception: if an entry has a 0 cloud sequence number, this means that the cloud sync
///    service hasn't yet been informed about it.
///    
/// The process of syncing a local device with the cloud sync service is essentially:
///  1) The local device constructs a list of the journal entries that have been produced by local UI actions but that
///     haven't yet been sent to the cloud sync service (as noted above they will all have cloud sequence number 0).
///  2) The local device figures out what is the latest (highest cloud sequence number) record that it has
///     received from the cloud sync service
///  3) The local device posts to the cloud sync service saying: "Here (1) is a list of all the new journal
///     entries that I need you to record; please record them and give me cloud sequence numbers for them.
///     Additionally please tell me all the new journal entries that other devices have told you about since
///     I last asked, which was sequence number (2)".
///  4) The response will be a list of journal entries, consisting first of all the journal entries from (1), with
///     cloud sequence numbers now assigned, then a list of all the new journal entries originating from other
///     devices.
///  5) To bring the local journal and viewmodel up to date, the local machine should:
///      i)  Update the local journal by applying the new cloud sequence numbers for the journal entries we sent,
///          and appending all the new journal entries coming from other machines
///      ii) Update the local viewmodel by replaying all the new journal entries coming from other machines
///      
/// There are plenty of pathological cases in which inconsistent updates from multiple devices might collide and
/// leave the local viewmodel (and indeed the global journal held on the cloud sync server) in a state that does
/// not make sense.  Because it's not actually permitted to delete any objects except sign-ups, the kinds of 
/// inconsistency that could happen are: lost updates to persons, gaming events or sessions; missing sign-ups;
/// duplicate sign-ups; sign-ups to sessions that have been declared full or abandoned; and sign-ups or session
/// proposals by users who are inactive (or have had their data scrubbed and replaced by '#deleted').
/// These cases will mainly happen if a user is logged and and doing things simultaneously on two devices, or
/// if someone makes changes on a device that isn't connected to the internet, and leaves it disconnected from
/// the internet for some time.  We discourage our users from doing that.  And if a device gets into a really funny
/// state, there is a 'Reset App' option in the support menu, that will force the app to go back and fetch all of its
/// state from the cloud sync service, starting over from scratch.
/// 
/// App startup and the initial loading of the viewmodel are managed in the contructor of the main
/// page - see MainPage.xaml.cs.
///      
/// </summary>
public partial class App : Application
{
	public App()
	{
		InitializeComponent();

        App.ApplicationSuspended = false; // allow logging to happen

        // Set up the dispatcher helper (tell it that this thread is the dispatcher thread)
        DispatcherHelper.Initialise(Environment.CurrentManagedThreadId, this.Dispatcher); // because I am pretty sure that Window.Current exists at this stage.

        if (!DispatcherHelper.UIDispatcherHasThreadAccess)
        {
            Logger.LogMessage(Logger.Level.ERROR,"App ctor","Horrible error; this code should only be run on the UI thread!");
        }

        Logger.LogMessage(Logger.Level.DEBUG, "App ctor", "about to create main page");

#pragma warning disable 0618 // this call provokes a deprecated warning, but the code was produced by the 'new project' wizard so I don't imagine it's really wrong
        MainPage = new AppShell();
#pragma warning restore 0618
    }

    public static bool ApplicationSuspended = false; // used to the logger to stop logging.

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

    /// <summary>
    /// The application data folder
    /// </summary>
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
    private static string _appFolder = null;

    /// <summary>
    /// A subfolder called 'lobsterconnect' with the app folder.  Put our files here.
    /// </summary>
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
    private static string _programFolder = null;

}
