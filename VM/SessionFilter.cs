/*
    Copyright (C) 2025 Turnipsoft Ltd, Jim Chapman

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
    /// A filter that the UI can use to select a subset of sessions according to some criteria.  It
    /// implements INotifyPropertyChanged so you can bind it to a UI.  Warning: since it raises
    /// PropertyChanged events every time any criterion is changed, you  probably want to be a bit
    /// careful about triggering a complete UI refresh in response to every such event, because you
    /// will end up doing that refresh operation a lot, if the filter is bound to XAML elements that
    /// the user is typing into.
    /// At present, the code doesn't pay any attention to these events at all, and handles changing
    /// the current filter by always just re-assigning MainViewModel.Instance.CurrentFilter to a whole
    /// new filter object, and handling the OnPropertyChanged event for MainViewModel.CurrentFilter
    /// to kick off the UI refresh. 
    /// </summary>
    public class SessionFilter : LobsterConnect.VM.BindableBase
    {
        public SessionFilter()
        {
            this._proposer = null;
            this._toPlay = null;
            this._signUpsInclude = null;
            this._state = null;
            this._onWishList = false;
        }

        public SessionFilter(SessionFilter that)
        {
            this._proposer = that._proposer;
            this._toPlay = that._toPlay;
            this._signUpsInclude = that._signUpsInclude;
            this._state = that._state;
            this._onWishList = that._onWishList;
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
                if (this.OnWishList)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Predicate that is true if the filter matches the given session.  Null or empty values in the
        /// attributes are treated as wildcards that match any session.  A 'false' value in the OnWishList
        /// attribute will match any session (whereas a true value will match only session's whose
        /// game is on the wish-list of the currently logged on user).
        /// </summary>
        /// <param name="s">the session to test for a match</param>
        /// <returns></returns>
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

            if(MainViewModel.Instance.LoggedOnUser!=null && this.OnWishList) // if there's a logged on user and the filter specifies we return only items on that person's wish list
            {
                List<WishListItem> wishList = MainViewModel.Instance.GetWishListItemsForPerson(MainViewModel.Instance.LoggedOnUser.Handle);

                if (!wishList.Any(w => w.Game == s.ToPlay)) // if no wish-list item has Game equal to s.ToPlay
                    return false;
            }

            return true;
        }

        /// <summary>
        /// A text string that loosely represents the contents of the filter
        /// </summary>
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

                    if (this.OnWishList)
                    {
                        if (string.IsNullOrEmpty(d))
                            d = "*";
                        else
                            d += ", *";
                    }

                    return "{" + d + "}";
                }
            }
        }

        /// <summary>
        /// Person handle of the person proposing this gaming session.  The set accessor does not
        /// check whether it is a valid, active person.
        /// A null or empty value will match any proposer.
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
        /// A string to be compared (using case-insensitive Contains()) with the full name of
        /// the game that is to be played.
        /// A null or empty value will match any game
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
        /// A null or empty value will match any signup list
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
        /// Null or Empty = all states will match the filter
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

        /// <summary>
        /// A boolean which, if true, means that the filter won't match a session unless its game
        /// is on the wish-list of the currently logged on user.
        /// /// </summary>
        public bool OnWishList
        {
            get
            {
                return this._onWishList;
            }
            set
            {
                if (this._onWishList != value)
                {
                    this._onWishList = value;

                    this.OnPropertyChanged("OnWishList");
                    this.OnPropertyChanged("Description");

                }
            }
        }
        private bool _onWishList = false;

    }
}
