using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LobsterConnect.Model;

namespace LobsterConnect.VM
{
    /// <summary>
    /// A game that the app knows about; it will be available for creating game sessions via the UI.
    /// Note that its member variables have public set accessors and are bindable
    /// but UI code should not use those accessors to change their values, because doing so will
    /// bypass the journal mechanism (so changes won't be saved and won't be propagated to the
    /// cloud storage).  Instead, use the appropriate CreateGame/UpdateGame methods on the main viewmodel.
    /// Note that, though it's possible to change BggLink this way, it is not a good idea to do so, because
    /// when sessions are created to play a certain game, the BggLink is copied from the game into the new
    /// session, so if the game's BggLink is subsequently amended, those sessions will now have
    /// inconsistent data in them.
    /// You should create and modify instances of this class only on the UI thread; it is not thread-safe
    /// 
    /// </summary>
    public class Game : LobsterConnect.VM.BindableBase
    {
        /// <summary>
        /// The name of this game.  Attempts to set it to a value containing a comma, a
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
                    if (this._name.Contains(','))
                    {
                        this._name = this._name.Replace(',', '_');
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
        /// Link to BGG entry for this game.  Attempts to set it to a value containing a 
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.
        /// </summary>
        public string BggLink
        {
            get
            {
                return this._bggLink;
            }
            set
            {
                if (this._bggLink != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._bggLink == "")
                        dontNotify = true;
                    if (value == "" && this._bggLink == null)
                        dontNotify = true;

                    this._bggLink = value;

                    if (this._bggLink.Contains('\\'))
                    {
                        this._bggLink = this._bggLink.Replace('\\', '_');
                    }
                    if (this._bggLink.Contains('|'))
                    {
                        this._bggLink = this._bggLink.Replace('|', '_');
                    }
                    if (this._bggLink.Contains('\n'))
                    {
                        this._bggLink = this._bggLink.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("BggLink");
                    }
                }
            }
        }
        private string _bggLink;

        /// <summary>
        /// True if this game is active (we don't ever delete a game, we just deactivate it, if we
        /// want the game to be no longer available for sign-up)
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
        private bool _isActive=true;
    }
}
