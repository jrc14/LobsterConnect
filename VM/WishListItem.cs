using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.VM
{
    /// <summary>
    /// A wish-list item, representing the fact that a certain person has an interest in playing
    /// a certain game at a certain gaming event.
    /// Note that its member variables have public set accessors and are bindable
    /// but UI code should not use those accessors to change their values, because doing so will
    /// bypass the journal mechanism (so changes won't be saved and won't be propagated to the
    /// cloud storage).
    /// You should create and modify instances of this class only on the UI thread; it is not thread-safe.
    /// </summary>
    public class WishListItem : LobsterConnect.VM.BindableBase
    {
        /// <summary>
        /// The handle (user id) of the person.  Attempts to set it to a value containing a comma, a
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.
        /// </summary>
        public string Person
        {
            get
            {
                return this._person;
            }
            set
            {
                if (this._person != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._person == "")
                        dontNotify = true;
                    if (value == "" && this._person == null)
                        dontNotify = true;

                    this._person = value;
                    if (this._person.Contains(','))
                    {
                        this._person = this._person.Replace(',', '_');
                    }
                    if (this._person.Contains('\\'))
                    {
                        this._person = this._person.Replace('\\', '_');
                    }
                    if (this._person.Contains('|'))
                    {
                        this._person = this._person.Replace('|', '_');
                    }
                    if (this._person.Contains('\n'))
                    {
                        this._person = this._person.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Person");
                    }
                }
            }
        }
        private string _person;

        /// <summary>
        /// The name (id) of the game.  Attempts to set it to a value containing a comma, a
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.
        /// </summary>
        public string Game
        {
            get
            {
                return this._game;
            }
            set
            {
                if (this._game != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._game == "")
                        dontNotify = true;
                    if (value == "" && this._game == null)
                        dontNotify = true;

                    this._game = value;
                    if (this._game.Contains(','))
                    {
                        this._game = this._game.Replace(',', '_');
                    }
                    if (this._game.Contains('\\'))
                    {
                        this._game = this._game.Replace('\\', '_');
                    }
                    if (this._game.Contains('|'))
                    {
                        this._game = this._game.Replace('|', '_');
                    }
                    if (this._game.Contains('\n'))
                    {
                        this._game = this._game.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Game");
                    }
                }
            }
        }
        private string _game;


        /// <summary>
        /// The name (id) of the gaming event.  Attempts to set it to a value containing a comma, a
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.
        /// </summary>
        public string GamingEvent
        {
            get
            {
                return this._gamingEvent;
            }
            set
            {
                if (this._gamingEvent != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._gamingEvent == "")
                        dontNotify = true;
                    if (value == "" && this._gamingEvent == null)
                        dontNotify = true;

                    this._gamingEvent = value;
                    if (this._gamingEvent.Contains(','))
                    {
                        this._gamingEvent = this._gamingEvent.Replace(',', '_');
                    }
                    if (this._gamingEvent.Contains('\\'))
                    {
                        this._gamingEvent = this._gamingEvent.Replace('\\', '_');
                    }
                    if (this._gamingEvent.Contains('|'))
                    {
                        this._gamingEvent = this._gamingEvent.Replace('|', '_');
                    }
                    if (this._gamingEvent.Contains('\n'))
                    {
                        this._gamingEvent = this._gamingEvent.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("GamingEvent");
                    }
                }
            }
        }
        private string _gamingEvent;


        /// <summary>
        /// The notes that the person has created.  Attempts to set it to a value containing a
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.
        /// </summary>
        public string Notes
        {
            get
            {
                return this._notes;
            }
            set
            {
                if (this._notes != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._notes == "")
                        dontNotify = true;
                    if (value == "" && this._notes == null)
                        dontNotify = true;

                    this._notes = value;

                    if (this._notes.Contains('\\'))
                    {
                        this._notes = this._notes.Replace('\\', '_');
                    }
                    if (this._notes.Contains('|'))
                    {
                        this._notes = this._notes.Replace('|', '_');
                    }
                    if (this._notes.Contains('\n'))
                    {
                        this._notes = this._notes.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Notes");
                    }
                }
            }
        }
        private string _notes;
    }
}
