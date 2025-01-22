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

        public List<Session> sessions = new List<Session>();

        private static MainViewModel _Instance = null;
        private static readonly LobsterLock _InstanceLock = new LobsterLock();

    }
}
