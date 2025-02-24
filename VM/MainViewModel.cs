using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LobsterConnect.Model;

namespace LobsterConnect.VM
{
    public class MainViewModel : LobsterConnect.VM.BindableBase
    {
        /// <summary>
        /// The single instance of the MainViewModel class.
        /// </summary>
        public static MainViewModel Instance
        {
            get
            {
                lock (_InstanceLock)
                {
                    if (_Instance == null)
                    {
                        MainViewModel i = new MainViewModel();
                        _Instance = i;
                    }
                    return _Instance;
                }
            }
        }

        /// <summary>
        /// Initialise the main view model by loading Persons, Games and Sessions (in due course this will come from
        /// the local jourbal stire and the cloud; initially I'm just creating some test data.
        /// </summary>
        public void Load()
        {
            CreateGame(false, "Ludo", "https://www.whatever.com/ludo");
            CreateGame(false, "Chess", "https://www.whatever.com/chess");
            CreateGame(false, "Diplomacy", "https://www.whatever.com/diplomacy");
            CreateGame(false, "Bridge", "https://www.whatever.com/bridge");
            CreateGame(false, "Whist", "https://www.whatever.com/whist");
            CreateGame(false, "Dominoes", "https://www.whatever.com/dominoes");

            CreatePerson(false, "bobby", password: Model.Utilities.PasswordHash("password"));
            CreatePerson(false, "susan", password: Model.Utilities.PasswordHash("password"));
            CreatePerson(false, "jrc14", password: Model.Utilities.PasswordHash("password"));
            CreatePerson(false, "steve", password: Model.Utilities.PasswordHash("password"));
            CreatePerson(false, "mike", password: Model.Utilities.PasswordHash("password"));

            string s1 = CreateSession(false, "bobby", "Ludo", this.CurrentEvent, new SessionTime(0));
            SignUp(false, "bobby", s1);
            SignUp(false, "jrc14", s1);

            string s2 = CreateSession(false, "jrc14", "Diplomacy", this.CurrentEvent, new SessionTime(2));
            SignUp(false, "bobby", s2);
            SignUp(false, "jrc14", s2);
            SignUp(false, "steve", s2);
            SignUp(false, "mike", s2);
            
            string s3 = CreateSession(false, "steve", "Chess", this.CurrentEvent, new SessionTime(2));
            SignUp(false, "jrc14", s3);
            SignUp(false, "steve", s3);
            SignUp(false, "mike", s3);

            string s4 = CreateSession(false, "steve", "Bridge", this.CurrentEvent, new SessionTime(2));

            string s5 = CreateSession(false, "steve", "Whist", this.CurrentEvent, new SessionTime(2));

            string s6 = CreateSession(false, "steve", "Dominoes", this.CurrentEvent, new SessionTime(2));
            

        }
        /// <summary>
        /// Create a game.  The only information needed is game name.  There also an optional BoardGameGeek link.
        /// An exception is thrown if the game name is null, or a game of that name is already in the games collection.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="name"></param>
        /// <param name="bggLink"></param>
        /// <exception cref="ArgumentException"></exception>
        public void CreateGame(bool informJournal, string name, string bggLink=null)
        {
            if(name==null)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateGame", "game name cannot be null'");
                throw new ArgumentException("MainViewModel.CreateGame: null name");
            }
            if(bggLink==null)
            {
                bggLink = "NO LINK";
            }

            lock(_gamesLock)
            {
                if(CheckGameNameExists(name))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateGame", "game with that name already exists:'" + name + "'");
                    throw new ArgumentException("MainViewModel.CreateGame: duplicate name:'" + name + "'");
                }
                _games.Add(new Game() { Name = name, BggLink = bggLink });
            }
        }

        /// <summary>
        /// Update attributes of a game, informing the journal if necessary.  Any attributes you don't want to update should be
        /// set to null.
        /// This method is the correct way to amend a game object in response to UI actions, because it will inform the journal, meaning
        /// that updates will be saved locally and passed on to the cloud store.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="game"></param>
        /// <param name="bggLink"></param>
        public void UpdateGame(bool informJournal, Game game, string bggLink)
        {
            if (bggLink != null)
                game.BggLink = bggLink;
        }

        /// <summary>
        /// Create a person.  The only information needed is the person handle; all other attributes are optional.
        /// An exception is thrown if the handle is null, or if a person having that handle already exists in the
        /// persons collection.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="handle"></param>
        /// <param name="fullName"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <exception cref="ArgumentException"></exception>
        public void CreatePerson(bool informJournal, string handle, string fullName = null, string phoneNumber = null, string email = null, string password = null)
        {
            if (handle == null)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreatePerson", "person handle cannot be null'");
                throw new ArgumentException("MainViewModel.CreatePerson: null handle");
            }
            if (fullName == null)
            {
                fullName = "NO NAME";
            }
            if (phoneNumber == null)
            {
                phoneNumber = "NO PHONE NUMBER";
            }
            if (email == null)
            {
                email = "NO EMAIL";
            }
            if (password == null)
            {
                password = "";
            }

            lock (_personsLock)
            {
                if (CheckPersonHandleExists(handle))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreatePerson", "person with that handle already exists:'" + handle + "'");
                    throw new ArgumentException("MainViewModel.CreatePerson: duplicate handle:'" + handle + "'");
                }
                _persons.Add(new Person() { Handle = handle, FullName = fullName, PhoneNumber = phoneNumber, Email = email, Password = password, IsActive = true });
            }
        }

        /// <summary>
        /// Update attributes of a person, informing the journal if necessary.  Any attributes you don't want to update should be
        /// set to null.
        /// This method is the correct way to amend a person object in response to UI actions, because it will inform the journal, meaning
        /// that updates will be saved locally and passed on to the cloud store.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="person"></param>
        /// <param name="fullName"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="isActive"></param>
        public void UpdatePerson(bool informJournal, Person person, string fullName = null, string phoneNumber = null, string email = null, string password = null, bool? isActive=null)
        {
            if (fullName != null)
                person.FullName = fullName;
            if (phoneNumber != null)
                person.PhoneNumber = phoneNumber;
            if (email != null)
                person.Email = email;
            if (password != null)
                person.Password = password;
            if (isActive != null)
                person.IsActive = (bool)isActive;
        }

        /// <summary>
        /// Create a game session that people will be able to sign up for, at the event named in the eventName parameter.
        /// The person proposing the game is identified by the proposerHandle parameter, the game is identified by
        /// the gameNameToPlay parameter.  The starting time of the session is provided by the startAt parameter.
        /// Other parameters are optional.
        /// The method will throw an exception if the proposer handle isn't found in the persons collection or if it
        /// corresponds to an inactive person, or if the game name is not found in the games collection.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="proposerHandle"></param>
        /// <param name="gameNameToPlay"></param>
        /// <param name="eventName"></param>
        /// <param name="startAt"></param>
        /// <param name="notes"></param>
        /// <param name="whatsAppLink"></param>
        /// <param name="sitsMinimum"></param>
        /// <param name="sitsMaximum"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public string CreateSession(bool informJournal, string proposerHandle, string gameNameToPlay, string eventName, SessionTime startAt, string notes=null, string whatsAppLink=null, int sitsMinimum=0, int sitsMaximum=0)
        {
            Person proposer = null;
            Game game = null;

            if(notes==null)
            {
                notes = "";
            }

            if (whatsAppLink == null)
            {
                whatsAppLink = "NO WHATSAPP CHAT";
            }

            string id = Guid.NewGuid().ToString();

            proposer = GetPerson(proposerHandle);
            if (proposer == null)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateSession", "no such person:'" + proposerHandle + "'");
                throw new ArgumentException("MainViewModel.CreateSession: no such person:'" + proposerHandle + "'");
            }

            game = GetGame(gameNameToPlay);
            if (game == null)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateSession", "no such game:'" + gameNameToPlay + "'");
                throw new ArgumentException("MainViewModel.CreateSession: no such game:'" + gameNameToPlay + "'");
            }

            lock (proposer.instanceLock) // guard against the race condition 'proposer gets made inactive while session is being created'
            {
                if (!proposer.IsActive)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateSession", "proposer is not active:'" + proposerHandle + "'");
                    throw new ArgumentException("MainViewModel.CreateSession: proposer is not active:'" + proposerHandle + "'");
                }

                _sessions.Add(new Session()
                {
                    Id=id,
                    Proposer = proposerHandle,
                    ToPlay = gameNameToPlay,
                    EventName = eventName,
                    StartAt = startAt,
                    Notes = notes,
                    WhatsAppLink = whatsAppLink,
                    BggLink = game.BggLink,
                    SignUps="",// will by side-effect set NumSignUps to 0
                    SitsMinimum = sitsMinimum,
                    SitsMaximum = sitsMaximum,
                    State = "OPEN"
                });
            }

            Model.DispatcherHelper.CheckBeginInvokeOnUI(() => {
                this.SessionsMustBeRefreshed?.Invoke(this, new EventArgs());
            });

            return id;
        }

        /// <summary>
        /// Signs a person up to play in a session.  The method throws an exception if the person handle or session id is invalid
        /// or the person is inactive, or the session is not open (i.e. if its full or abandoned).  An exception is also thrown
        /// (from Session.AddSignUp) if the person was already signed up.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="personHandle"></param>
        /// <param name="sessionId"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SignUp(bool informJournal, string personHandle, string sessionId)
        {
            Person person = GetPerson(personHandle);
            Session session = GetSession(sessionId);

            if(person==null)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SignUp", "no such person:'" + personHandle + "'");
                throw new ArgumentException("MainViewModel.SignUp: no such person:'" + personHandle + "'");
            }

            if (session == null)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SignUp", "no such session:'" + sessionId + "'");
                throw new ArgumentException("MainViewModel.SignUp: no such session:'" + sessionId + "'");
            }

            lock(person.instanceLock)
            {
                if(!person.IsActive)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SignUp", "person is not active:'" + personHandle + "'");
                    throw new ArgumentException("MainViewModel.SignUp: person is not active:'" + personHandle + "'");
                }
                lock(session.instanceLock)
                {
                    if(session.State!="OPEN")
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SignUp", "session is not OPEN:'" + sessionId + "'");
                        throw new ArgumentException("MainViewModel.SignUp: session is not OPEN:'" + sessionId + "'");
                    }

                    session.AddSignUp(person.Handle);
                }
            }
        }

        /// <summary>
        /// Cancels an existing signup for a person to play in a session.  The method throws an exception if the person handle or session id is invalid.
        /// An exception is also thrown (from Session.RemoveSignUp) if the person was not signed up.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="personHandle"></param>
        /// <param name="sessionId"></param>
        /// <exception cref="ArgumentException"></exception>
        public void CancelSignUp(bool informJournal, string personHandle, string sessionId)
        {
            Person person = GetPerson(personHandle);
            Session session = GetSession(sessionId);

            if (person == null)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CancelSignUp", "no such person:'" + personHandle + "'");
                throw new ArgumentException("MainViewModel.CancelSignUp: no such person:'" + personHandle + "'");
            }

            if (session == null)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CancelSignUp", "no such session:'" + sessionId + "'");
                throw new ArgumentException("MainViewModel.CancelSignUp: no such session:'" + sessionId + "'");
            }

            lock (session.instanceLock)
            {
                session.RemoveSignUp(person.Handle);
            }
        }

        /// <summary>
        /// Update attributes of a session, informing the journal if necessary.  Any attributes you don't want to update should be
        /// set to null.  The state att4ribute should be one of OPEN, CLOSED or ABANDONED; if it is not then an exception will be thrown.
        /// This method is the correct way to amend a session object in response to UI actions, because it will inform the journal, meaning
        /// that updates will be saved locally and passed on to the cloud store.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="session"></param>
        /// <param name="notes"></param>
        /// <param name="whatsAppLink"></param>
        /// <param name="sitsMinimum"></param>
        /// <param name="sitsMaximum"></param>
        /// <param name="state"></param>
        public void UpdateSession(bool informJournal, Session session, string notes = null, string whatsAppLink = null, int? sitsMinimum = null, int? sitsMaximum = null, string state=null)
        {
            lock(session.instanceLock)
            {
                if (notes != null)
                    session.Notes = notes;
                if (whatsAppLink != null)
                    session.WhatsAppLink = whatsAppLink;
                if (sitsMinimum != null)
                    session.SitsMinimum = (int)sitsMinimum;
                if (sitsMaximum != null)
                    session.SitsMinimum = (int)sitsMaximum;
                if (state != null)
                {
                    if (state == "OPEN" || state == "CLOSED" || state == "ABANDONED")
                    {
                        session.State = state;
                    }
                    else
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.UpdateSession", "invalid state:'" + state + "'");
                        throw new ArgumentException("MainViewModel.UpdateSession: invalid state:'" + state + "'");
                    }
                }
            }
        }

        /// <summary>
        /// The gaming event that the app is currently managing sign-ups for. 
        /// </summary>
        public string CurrentEvent
        {
            get
            {
                return this._currentEvent;
            }
            set
            {
                if (this._currentEvent != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._currentEvent == "")
                        dontNotify = true;
                    if (value == "" && this._currentEvent == null)
                        dontNotify = true;

                    this._currentEvent = value;

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("CurrentEvent");
                    }
                }
            }
        }
        private string _currentEvent = "LoBsterCon XXVIII";


        /// <summary>
        /// The currently logged on person, or null if no person is logged on
        /// </summary>
        public Person LoggedOnUser
        {
            get
            {
                return this._loggedOnUser;
            }
            set
            {
                this._loggedOnUser = value;

                this.OnPropertyChanged("LoggedOnUser");
            }
        }
        private Person _loggedOnUser = null;

        /// <summary>
        /// Retrieve a list of events that we can manage signups for.  Right now, the functionality is stubbed
        /// out, and we only manage one possible event, LoBsterCon XXVIII.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableEvents()
        {
            // This is just placeholder code.  Some day we will want to support more than one event.
            return _AvailableEvents;
        }
        private static List<string> _AvailableEvents = new List<string> {"LoBsterCon XXVIII" };

        /// <summary>
        /// Constructs an array of all sessions for the given event.  The index into the array is session time (turned into
        /// an ordinal number) so the number of entries in the array will be equal to the number of possible time slots for
        /// that event.  Each entry in the array is a list of sessions, sorted (first by proposer name then by id).
        /// If there are no sessions for a specifed time slot, the corresponding array entry will be null.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public List<Session>[] GetAllSessions(string eventName)
        {
            int numSlots = GetNumberOfTimeSlots(eventName);

            List<Session>[] allSessions = new List<Session>[numSlots];

            lock(this._sessionsLock)
            {
                foreach(Session s in this._sessions)
                {
                    if (s.EventName != eventName)
                        continue;

                    // The sessions we already found for the same time slot as s.  We need to insert s
                    // into the right spot in this list.
                    List<Session> existing = allSessions[s.StartAt.Ordinal];

                    if(existing==null)
                    {
                        // easy case: so far we found no sessions for this slot; just create a new
                        // list for this slot, and this session into the list.
                        allSessions[s.StartAt.Ordinal] = new List<Session>() { s };
                    }
                    else
                    {
                        // starting at 0, advance insertAt until we find an element that is greater than s, or we run
                        // out of elements in the existing list
                        int insertAt = 0;
                        while(insertAt < existing.Count && existing[insertAt].CompareTo(s) <= 0)
                        {
                            insertAt++;
                        }

                        if (insertAt < existing.Count) // There is an existing element that the new session should go before
                        {
                            existing.Insert(insertAt, s);
                        }
                        else
                        {
                            existing.Add(s); // The new session should go on the end of the list
                        }
                    }
                }
            }

            return allSessions;
        }

        /// <summary>
        /// Fire this event when you do something that would necessitate refreshing the table of sessions in the UI.
        /// That means a change the sessions collection (i.e. when you add or remove elements
        /// to the collection, having Session.EventName==MainViewModel.CurrentEvent), or changing the value of 
        /// MainViewModel.CurrentEvent.  Note that merely changing an attribute of a session doesn't require
        /// a refresh of the sessions table in the UI, because we expect the UI to bind to the relevant session
        /// attributes, so it will see such changes anyway.
        /// </summary>
        public event EventHandler SessionsMustBeRefreshed;

        /// <summary>
        /// Get the number of time slots that exist for the given event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public int GetNumberOfTimeSlots(string eventName)
        {
            // when we actually implement more than one possible event, this will work differently.
            return SessionTime.NumberOfTimeSlots;

        }

        /// <summary>
        /// Fetch a Session object from the game sessions collection, given an id.  If the id is not valid the method
        /// returns null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Session GetSession(string id)
        {
            lock(_sessionsLock)
            {
                foreach (Session s in _sessions)
                    if (s.Id == id)
                        return s;
            }
            return null;
        }

        /// <summary>
        /// The game sessions collection.  It's private; add sessions using the CreateSession method, retrieve them (by id) using
        /// GetSession.  Sessions are never deleted from the game sessions collection, but if Session.Status is set to
        /// "ABANDONED" the session won't be available for sign-ups and shouldn't be shown in the sessions list UI.
        /// </summary>
        private List<Session> _sessions = new List<Session>();

        /// <summary>
        /// Lock for the game sessions collection.  Lock this while adding or removing elements in the collection, and while
        /// iterating over it.
        /// </summary>
        private readonly LobsterLock _sessionsLock = new LobsterLock();

        public Person GetPerson(string handle)
        {
            lock(_personsLock)
            {
                foreach (Person p in _persons)
                    if (p.Handle == handle)
                        return p;
            }
            return null;
        }
        public bool CheckPersonHandleExists(string handle)
        {
            if (this.GetPerson(handle) == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// The persons collection.  It's private; add persons using the CreatePerson method, retrieve them (by handle) using
        /// GetPerson, and check the validity of a person handle by using CheckPersonHandleExists.  Persons are never deleted
        /// from the persons collection, but if Person.IsActive is set to false the person won't be able to sign up for game
        /// sessions.
        /// </summary>
        private List<Person> _persons = new List<Person>();

        /// <summary>
        /// Lock for the persons collection.  Lock this while adding or removing elements in the collection, and while
        /// iterating over it.
        /// </summary>
        private readonly LobsterLock _personsLock = new LobsterLock();

        /// <summary>
        /// Checks whether the games collection contains a game having the name provided.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckGameNameExists(string name)
        {
            if (this.GetGame(name) == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Retrieve a list of the games that we can manage signups for.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableGames()
        {
            List<string> gameNames = new List<string>();
            lock (_gamesLock)
            {
                foreach (Game g in _games)
                    gameNames.Add(g.Name);
            }

            return gameNames;
        }

        /// <summary>
        /// Returns the Game object for the game having the name provided, or null if there is no such game in the
        /// games collection.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Game GetGame(string name)
        {
            lock (_gamesLock)
            {
                foreach (Game g in _games)
                    if (g.Name == name)
                        return g;
            }
            return null;
        }

        public void LogUserMessage(Logger.Level level, string message)
        {
            string l = "";
            switch(level)
            {
                case Logger.Level.DEBUG:   l = "DEBUG:"; break;
                case Logger.Level.INFO:    l = "INFO:"; break;
                case Logger.Level.WARNING: l = "ALERT:";  break;
                case Logger.Level.ERROR:   l = "ERROR:"; break;
                default:break;
            }

            DateTime d = DateTime.Now;
            int hh = d.Hour;
            int mm = d.Minute;

            string t = string.Format("{0:D2}:{1:D2}: ", hh, mm);

            if (UserMessages == null)
                UserMessages = new System.Collections.ObjectModel.ObservableCollection<Tuple<string, string, string>>();

            UserMessages.Insert(0, new Tuple<string, string, string>(t,l,message));

            Logger.LogMessage(level, "MainViewModel.LogUserMessage", message);

        }

        public System.Collections.ObjectModel.ObservableCollection<Tuple<string, string, string>> UserMessages { get; set; }

        /// <summary>
        /// The games collection.  It's private; add games using the CreateGame method, retrieve them (by name) using
        /// GetGame, and check the validity of a game name by using CheckGameNameExists.  Games are never deleted
        /// from the games collection.
        /// </summary>
        private List<Game> _games = new List<Game>();

        /// <summary>
        /// Lock for the games collection.  Lock this while adding or removing elements in the collection, and while
        /// iterating over it.
        /// </summary>
        private readonly LobsterLock _gamesLock = new LobsterLock();

        /// <summary>
        /// Backing member variable for the single instance of the MainViewModel class, accessed via the public get
        /// accessor for Instance.
        /// </summary>
        private static MainViewModel _Instance = null;
        /// <summary>
        /// Lock to be held when creating the single instance for the MainViewModel class.
        /// </summary>
        private static readonly LobsterLock _InstanceLock = new LobsterLock();

    }
}
