using CommunityToolkit.Maui.Markup;
using LobsterConnect.Model;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.VM
{
    /// <summary>
    /// A filter that the UI can use to select a subset of sessions according to some criteria.  It implements INotifyPropertyChanged
    /// so you can bind it to a UI.  Warning: since it raises PropertyChanged events every time any criterion is changed, you
    /// probably want to be a bit careful about triggering a complete UI refresh in response to every such event, because you
    /// will end up doing that refresh operation a lot, if the filter is bound to XAML elements that the user is typing into.
    /// </summary>
    public class SessionFilter : LobsterConnect.VM.BindableBase
    {
        public SessionFilter()
        {
            this._proposer = null;
            this._toPlay = null;
            this._signUpsInclude = null;
            this._state = null;
        }

        public SessionFilter(SessionFilter that)
        {
            this._proposer = that._proposer;
            this._toPlay = that._toPlay;
            this._signUpsInclude = that._signUpsInclude;
            this._state = that._state;
        }
        /// <summary>
        /// True if this filter applies no criteria (i.e. will return true for any call to Matches(Session).  False if
        /// it contains some filter criteria
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Proposer))
                    return false;
                if (!string.IsNullOrEmpty(this.ToPlay))
                    return false;
                if (!string.IsNullOrEmpty(this.SignUpsInclude))
                    return false;
                if (!string.IsNullOrEmpty(this.State))
                    return false;

                return true;
            }
        }

        public bool Matches(Session s)
        {
            if (!string.IsNullOrEmpty(this.Proposer))
            {
                if (s.Proposer != this.Proposer)
                    return false;
            }
            if (!string.IsNullOrEmpty(this.ToPlay))
            {
                if(!s.ToPlay.Contains(this.ToPlay, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }
            if (!string.IsNullOrEmpty(this.SignUpsInclude))
            {
                if (!s.IsSignedUp(this.SignUpsInclude))
                    return false;
            }
            if (!string.IsNullOrEmpty(this.State))
            {
                if (s.State != this.State)
                    return false;
            }
            return true;
        }

        public string Description
        {
            get
            {
                if (this.IsEmpty)
                    return null;
                else
                {
                    string d = "";

                    if (!string.IsNullOrEmpty(this.Proposer))
                        d += this.Proposer;

                    if (!string.IsNullOrEmpty(this.ToPlay))
                    {
                        if (string.IsNullOrEmpty(d))
                            d = "'" + this.ToPlay + "'";
                        else
                            d+=", '" + this.ToPlay + "'";
                    }


                    if (!string.IsNullOrEmpty(this.SignUpsInclude))
                    {
                        if (string.IsNullOrEmpty(d))
                            d = this.SignUpsInclude;
                        else
                            d += ", " + this.SignUpsInclude;
                    }

                    if (!string.IsNullOrEmpty(this.State))
                    {
                        if (string.IsNullOrEmpty(d))
                            d = this.State;
                        else
                            d += ", " + this.State;
                    }

                    return "{" + d + "}";
                }
            }
        }
        /// <summary>
        /// Person handle of the person proposing this gaming session.  The set accessor does not check whether it is a valid, active person.
        /// </summary>
        public string Proposer
        {
            get
            {
                return this._proposer;
            }
            set
            {
                if (this._proposer != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._proposer == "")
                        dontNotify = true;
                    if (value == "" && this._proposer == null)
                        dontNotify = true;

                    this._proposer = value;

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Proposer");
                        this.OnPropertyChanged("Description");
                    }
                }
            }
        }
        private string _proposer="";

        /// <summary>
        /// A string to be compared (using case-insensitive Contains()) with the full name of the game that is to be played.
        /// </summary>
        public string ToPlay
        {
            get
            {
                return this._toPlay;
            }
            set
            {
                if (this._toPlay != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._toPlay == "")
                        dontNotify = true;
                    if (value == "" && this._toPlay == null)
                        dontNotify = true;

                    this._toPlay = value;

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("ToPlay");
                        this.OnPropertyChanged("Description");
                    }
                }
            }
        }
        private string _toPlay="";

        /// <summary>
        /// A person handle to be searched for in the list of sign-ups for the session
        /// /// </summary>
        public string SignUpsInclude
        {
            get
            {
                return this._signUpsInclude;
            }
            set
            {
                if (this._signUpsInclude != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._signUpsInclude == "")
                        dontNotify = true;
                    if (value == "" && this._signUpsInclude == null)
                        dontNotify = true;

                    this._signUpsInclude = value;

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("SignUpsInclude");
                        this.OnPropertyChanged("Description");
                    }
                }
            }
        }
        private string _signUpsInclude = "";


        /// <summary>
        /// A string representing the state of this session:
        /// OPEN = players are permitted to sign up
        /// FULL = no more slots are available (the game is full); sign up attempts will fail
        /// ABANDONED = the session has been abandoned; sign up attempts will fail
        /// </summary>
        public string State
        {
            get
            {
                return this._state;
            }
            set
            {
                if (value == "OPEN" || value == "FULL" || value == "ABANDONED" || string.IsNullOrEmpty(value))
                {
                    this._state = value;
                    this.OnPropertyChanged("State");
                    this.OnPropertyChanged("Description");
                }
                else
                {
                    Logger.LogMessage(Logger.Level.ERROR, "Session.State set accessor", "invalid state:'" + value + "'");
                    throw new ArgumentException("SessionFilter.State set accessor: invalid state:'" + value + "'");
                }
            }
        }
        private string _state = "";

    }
}
