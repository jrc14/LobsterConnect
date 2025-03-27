using LobsterConnect.Model;

namespace LobsterConnect.VM
{
    /// <summary>
    /// An event that that the app knows about; if it is active, it will be available for creating game sessions via the UI.
    /// Note that its member variables have public set accessors and are bindable
    /// but UI code should not use those accessors to change their values, because doing so will
    /// bypass the journal mechanism (so changes won't be saved and won't be propagated to the
    /// cloud storage).  Instead you should use the UpdateGamingEvent method on the viewmodel.
    /// Note that the EventType member can't be changed this way; this is because changing
    /// the event type is a bad idea (because the range of valid time slot indexes on a session is
    /// different depending on event type, changes to a game event's type could invalidate existing
    /// sessions in a way that would be painful to manage)
    /// You should create and modify instances of this class only on the UI thread; it is not thread-safe
    /// </summary>
    public class GamingEvent : LobsterConnect.VM.BindableBase
    {
        /// <summary>
        /// The name of this gaming event. Attempts to set it to a value containing a 
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (this._name != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._name == "")
                        dontNotify = true;
                    if (value == "" && this._name == null)
                        dontNotify = true;

                    this._name = value;

                    if (this._name.Contains('\\'))
                    {
                        this._name = this._name.Replace('\\', '_');
                    }
                    if (this._name.Contains('|'))
                    {
                        this._name = this._name.Replace('|', '_');
                    }
                    if (this._name.Contains('\n'))
                    {
                        this._name = this._name.Replace('\n', '_');
                    }


                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Name");
                    }
                }
            }
        }
        private string _name;


        /// <summary>
        /// A string representing the type of this gaming event:
        /// EVENING = sign-up slots will be available for 18h00, 19h00, and 20h00
        /// DAY = sign-up slots will be available from 09h00 to 20h00
        /// CONVENTION = sign-up slots will be available for Fri afternoon, all day Sat and Sun, and Mon morning
        /// </summary>
        public string EventType
        {
            get
            {
                return this._eventType;
            }
            set
            {
                if (value == "EVENING" || value == "DAY" || value == "CONVENTION")
                {
                    this._eventType = value;
                    this.OnPropertyChanged("EventType");
                }
                else
                {
                    Logger.LogMessage(Logger.Level.ERROR, "GamingEvent.EventType set accessor", "invalid type:'" + value + "'");
                    throw new ArgumentException("GamingEvent.EventType set accessor: invalid state:'" + value + "'");
                }
            }
        }
        private string _eventType = "DAY"; // initialise it as 'DAY' because it has to have some valid value

        /// <summary>
        /// True if this gaming event is active (we don't ever delete an event, we just deactivate it, if we
        /// want it to be no longer available for gaming sessions)
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this._isActive;
            }
            set
            {
                if (this._isActive != value)
                {
                    this._isActive = value;

                    this.OnPropertyChanged("IsActive");

                }
            }
        }
        private bool _isActive = true;
    }
}
