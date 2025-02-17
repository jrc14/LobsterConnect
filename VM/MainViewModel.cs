using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using LobsterConnect.Model;

namespace LobsterConnect.VM
{
    public class MainViewModel : LobsterConnect.VM.BindableBase
    {
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

        public void CreateGame(string name, string bggLink=null)
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

        public void CreatePerson(string handle, string fullName = null, string phoneNumber = null, string email = null, string password = null)
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
                password = "*";
            }

            lock (_personsLock)
            {
                if (CheckPersonHandleExists(handle))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreatePerson", "person with that handle already exists:'" + handle + "'");
                    throw new ArgumentException("MainViewModel.CreatePerson: duplicate handle:'" + handle + "'");
                }
                _persons.Add(new Person() { Handle = handle, FullName = fullName, PhoneNumber = phoneNumber, Email = email, Password = password });
            }
        }

        public string CreateSession(string proposerHandle, string gameNameToPlay, SessionTime startAt, string notes=null, string whatsAppLink=null, int sitsMinimum=0, int sitsMaximum=0)
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

            string id = new Guid().ToString();

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

            return id;
        }

        public void SignUp(string personHandle, string sessionId)
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
        public string CurrentEvent
        {
            // This is just placeholder code.  Some day we will want to support more than one event.
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
        private string _currentEvent = "";

        public List<string> GetAvailableEvents()
        {
            // This is just placeholder code.  Some day we will want to support more than one event.
            return _AvailableEvents;
        }
        private static List<string> _AvailableEvents = new List<string> {"LoBsterCon XXVIII" };

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
        private List<Session> _sessions = new List<Session>();
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
        private List<Person> _persons = new List<Person>();
        private readonly LobsterLock _personsLock = new LobsterLock();

        public bool CheckGameNameExists(string name)
        {
            if (this.GetGame(name) == null)
                return false;
            else
                return true;
        }
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
        private List<Game> _games = new List<Game>();
        private readonly LobsterLock _gamesLock = new LobsterLock();

        private static MainViewModel _Instance = null;
        private static readonly LobsterLock _InstanceLock = new LobsterLock();

    }
}
