using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
