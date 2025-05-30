﻿/*
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
    /// A gaming session that the app knows about, representing the fact that a certain group of people have
    /// signed up to play some game at an agreed time, at a gaming event.
    /// Note that its member variables have public set accessors and are bindable
    /// but UI code should not use those accessors to change their values, because doing so will
    /// bypass the journal mechanism (so changes won't be saved and won't be propagated to the
    /// cloud storage).
    /// You should create and modify instances of this class only on the UI thread; it is not thread-safe
    /// </summary>
    public class Session : LobsterConnect.VM.BindableBase, IComparable
    {
        public int CompareTo(object that)
        {
            if (that == null)
                throw new ArgumentException("Session.CompareTo: can't compare with null");

            if (that as Session == null)
                throw new ArgumentException("Session.CompareTo: can't compare with a different type");

            int c1 = this.StartAt.CompareTo(((Session)that).StartAt);
            if (c1 != 0) return c1;

            int c2 = this.ToPlay.CompareTo(((Session)that).ToPlay);
            if (c2!= 0) return c2;

            int c3 = this.Proposer.CompareTo(((Session)that).Proposer);
            if (c3 != 0) return c3;

            int c4 = this.Id.CompareTo(((Session)that).Id);
            if (c4 != 0) return c4;

            return 0;
        }

        /// <summary>
        /// Opaque Id (it will be set to a GUID when the ViewModel creates a session)
        /// </summary>
        public string Id
        {
            get
            {
                return this._id;
            }
            set
            {
                if (this._id != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._id == "")
                        dontNotify = true;
                    if (value == "" && this._id == null)
                        dontNotify = true;

                    this._id = value;

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Id");
                    }
                }
            }
        }
        private string _id;

        /// <summary>
        /// Person handle of the person proposing this gaming session.  The set accessor does not check whether it is a valid, active person.
        /// Attempts to set it to a value containing a 
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.  Such values are, in any case, not valid person handles.
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

                    if (this._proposer.Contains('\\'))
                    {
                        this._proposer = this._proposer.Replace('\\', '_');
                    }
                    if (this._proposer.Contains('|'))
                    {
                        this._proposer = this._proposer.Replace('|', '_');
                    }
                    if (this._proposer.Contains('\n'))
                    {
                        this._proposer = this._proposer.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Proposer");
                    }
                }
            }
        }
        private string _proposer;

        /// <summary>
        /// The full name of the game that is to be played.  The set accessor does not check that it is a valid game name.
        /// Attempts to set it to a value containing a 
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.  Such values are, in any case, not valid game names.
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

                    if (this._toPlay.Contains('\\'))
                    {
                        this._toPlay = this._toPlay.Replace('\\', '_');
                    }
                    if (this._toPlay.Contains('|'))
                    {
                        this._toPlay = this._toPlay.Replace('|', '_');
                    }
                    if (this._toPlay.Contains('\n'))
                    {
                        this._toPlay = this._toPlay.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("ToPlay");
                    }
                }
            }
        }
        private string _toPlay;


        /// <summary>
        /// The name of the event at which this session will happen.  The set accessor does not check that it is a valid game name.
        /// Attempts to set it to a value containing a 
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.  Such values are, in any case, not valid gaming event names.
        /// </summary>
        public string EventName
        {
            get
            {
                return this._eventName;
            }
            set
            {
                if (this._eventName != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._eventName == "")
                        dontNotify = true;
                    if (value == "" && this._eventName == null)
                        dontNotify = true;

                    this._eventName = value;

                    if (this._eventName.Contains('\\'))
                    {
                        this._eventName = this._eventName.Replace('\\', '_');
                    }
                    if (this._eventName.Contains('|'))
                    {
                        this._eventName = this._eventName.Replace('|', '_');
                    }
                    if (this._eventName.Contains('\n'))
                    {
                        this._eventName = this._eventName.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("EventName");
                    }
                }
            }
        }
        private string _eventName;

        /// <summary>
        /// When the session starts
        /// </summary>
        public SessionTime StartAt // session time slot for the start of the game
        {
            get
            {
                return this._startAt;
            }
            set
            {
                if (this._startAt != value)
                {
                    bool dontNotify = false;

                    this._startAt = value;

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("StartAt");
                    }
                }
            }
        }
        private SessionTime _startAt;

        /// <summary>
        /// Human-readable notes about this session. Attempts to set it to a value containing a 
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

        /// <summary>
        /// Link to WhatsApp chat for this session.  Attempts to set it to a value containing a 
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.
        /// </summary>
        public string WhatsAppLink
        {
            get
            {
                return this._whatsAppLink;
            }
            set
            {
                if (this._whatsAppLink != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._whatsAppLink == "")
                        dontNotify = true;
                    if (value == "" && this._whatsAppLink == null)
                        dontNotify = true;

                    this._whatsAppLink = value;

                    if (this._whatsAppLink.Contains('\\'))
                    {
                        this._whatsAppLink = this._whatsAppLink.Replace('\\', '_');
                    }
                    if (this._whatsAppLink.Contains('|'))
                    {
                        this._whatsAppLink = this._whatsAppLink.Replace('|', '_');
                    }
                    if (this._whatsAppLink.Contains('\n'))
                    {
                        this._whatsAppLink = this._whatsAppLink.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("WhatsAppLink");
                    }
                }
            }
        }
        private string _whatsAppLink;

        /// <summary>
        /// Link to BGG entry for the game played in this session.  Attempts to set it to a value containing a 
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
        /// Comma-separated list of person handles, being the people signed up for this session.  The set accessor does not check whether
        /// the handles are valid (but none of them should be null (i.e. in a comma-separated list, we should never see ",,").
        /// The accessor will create a comma-separated list with spaces after each comma, to make it look prettier in the UI - but
        /// person handles ought not to have spaces at their start of end.
        /// /// </summary>
        public string SignUps
        {
            get
            {
                return this._signUps;
            }
            set
            {
                if (this._signUps != value)
                {
                    if (string.IsNullOrEmpty(value)) // no person handles
                    {
                        this._signUps = "";
                        this._numSignUps = 0;
                        this._signUps = value;
                    }
                    else if (!value.Contains(',')) // one person handle
                    {
                        string handle = value.Trim();

                        this._signUps = handle;
                        this._numSignUps = 1;
                    }
                    else // the list contains more than one person handle
                    {
                        int hh = 0;
                        string handles = "";
                        foreach (string h in value.Split(','))
                        {
                            string handle = h.Trim();
                            if (string.IsNullOrEmpty(handle))
                            {
                                Logger.LogMessage(Logger.Level.ERROR, "Session.SignUps set accessor", "list contains NULL person");
                                throw new ArgumentException("Session.SignUps set accessor: list contains NULL person'");
                            }

                            if (handles == "")
                                handles = handle;
                            else
                                handles += ", " + handle;

                            hh++;
                        }
                        this._signUps = handles;
                        this._numSignUps = hh;
                    }

                    this.OnPropertyChanged("SignUps");
                    this.OnPropertyChanged("NumSignUps");
                }
            }
        }
        private string _signUps="";

        /// <summary>
        /// Adds a signup to self.  The method doesn't check that the person handle is a valid, active person, but it does
        /// ignore attempts to add a duplicate sign up, unless they are for the '#deleted' user.  It also rejects
        /// person handles containing a comma, because of the trouble this can cause.
        /// Warning: though this method is public you probably don't want to call it.  Rather, you should add sign-ups by
        /// calling MainViewModel.Instance.ignUp because that method will take care of updating the UI and writing to
        /// the journal, so that changes get saved properly.
        /// </summary>
        /// <param name="personHandle"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddSignUp(string personHandle)
        {
            if (string.IsNullOrEmpty(personHandle))
            {
                Logger.LogMessage(Logger.Level.ERROR, "Session.AddSignUp", "person handle must not be null");
                throw new ArgumentException("Session.AddSignUp: null person");
            }
            if (personHandle.Contains(','))
            {
                Logger.LogMessage(Logger.Level.ERROR, "Session.AddSignUp", "person handle invalid (it contains a comma): '" + personHandle + "'");
                throw new ArgumentException("Session.AddSignUp: person handle is invalid");
            }

            personHandle = personHandle.Trim();

            if (string.IsNullOrEmpty(this._signUps)) // no person handles existing
            {
                this._signUps = personHandle;
                this._numSignUps = 1;
            }
            else if (!_signUps.Contains(',')) // one person handle existing
            {
                string existing = this._signUps.Trim();

                if (existing == personHandle && personHandle!="#deleted")
                {
                    Logger.LogMessage(Logger.Level.INFO, "Session.AddSignUp", "person handle is already signed up: '" + personHandle + "'");
                    this._numSignUps = 1;
                }
                else
                {
                    this._signUps += ", " + personHandle;
                    this._numSignUps = 2;
                }
            }
            else // the existing list contains more than one person handle
            {
                int hh = 0;

                bool duplicated = false;
                if (personHandle != "#deleted")
                {
                    foreach (string h in this._signUps.Split(','))
                    {
                        string existing = h.Trim();
                        if (personHandle == existing)
                        {
                            duplicated = true;
                            Logger.LogMessage(Logger.Level.INFO, "Session.AddSignUp", "person is already signed up: '" + personHandle + "'");
                        }

                        hh++;
                    }
                }
                //else
                //{
                //    Logger.LogMessage(Logger.Level.DEBUG, "Session.AddSignUp", "signing up #deleted person to "+this.Id);
                //}
                if (!duplicated)
                {
                    this._signUps += ", " + personHandle;
                    this._numSignUps = hh + 1;
                }
            }

            this.OnPropertyChanged("SignUps");
            this.OnPropertyChanged("NumSignUps");
        }

        /// <summary>
        /// Removes a signup from self.  The method doesn't check that the person handle is a valid, active person, but it does
        /// reject person handles containing a comma, because of the trouble this can cause.  It's possible that when the
        /// method is called, there are multiple sign-ups for the '#deleted' user; in this case, the method will remove one of
        /// them.
        /// It will throw an exception if the person isn't signed up.
        /// Warning: though this method is public you probably don't want to call it.  Rather, you should remove sign-ups by
        /// calling MainViewModel.Instance.CancelSignUp because that method will take care of updating the UI and writing to
        /// the journal, so that changes get saved properly.
        /// </summary>
        /// <param name="personHandle"></param>
        /// <exception cref="ArgumentException"></exception>
        public void RemoveSignUp(string personHandle)
        {
            if (string.IsNullOrEmpty(personHandle))
            {
                Logger.LogMessage(Logger.Level.ERROR, "Session.RemoveSignUp", "person handle must not be null");
                throw new ArgumentException("Session.RemoveSignUp: null person");
            }
            if (personHandle.Contains(','))
            {
                Logger.LogMessage(Logger.Level.ERROR, "Session.RemoveSignUp", "person handle invalid (it contains a comma): '" + personHandle + "'");
                throw new ArgumentException("Session.RemoveSignUp: person handle is invalid");
            }

            personHandle = personHandle.Trim();

            if (string.IsNullOrEmpty(this._signUps)) // no person handles existing
            {
                Logger.LogMessage(Logger.Level.INFO, "Session.RemoveSignUp", "session has no sign ups");
            }
            else if (!_signUps.Contains(',')) // one person handle existing
            {
                string existing = this._signUps.Trim();

                if (existing == personHandle)
                {
                    this._signUps = "";
                    this._numSignUps = 0;
                }
                else
                {
                    Logger.LogMessage(Logger.Level.INFO, "Session.RemoveSignUp", "person was not signed up: '" + personHandle + "'");
                }
            }
            else // the existing list contains more than one person handle
            {
                int toRemove = -1;
                List<string> existing = new List<string>(this._signUps.Split(','));
                for (int hh = 0; hh < existing.Count; hh++)
                {
                    existing[hh] = existing[hh].Trim();
                    if (existing[hh] == personHandle)
                    {
                        if (toRemove != -1 && personHandle!="#deleted") // a duplicate '#deleted' user is OK; we just remove the last one we find
                        {
                            Logger.LogMessage(Logger.Level.ERROR, "Session.RemoveSignUp", "person was signed up twice: '" + personHandle + "'");
                            throw new ArgumentException("Session.RemoveSignUp: person was signed up twice");
                        }
                        toRemove = hh;
                    }
                }
                if (toRemove == -1)
                {
                    Logger.LogMessage(Logger.Level.INFO, "Session.RemoveSignUp", "person was not signed up: '" + personHandle + "'");
                }
                else
                {
                    existing.RemoveAt(toRemove);

                    this._signUps = string.Join(", ", existing);
                    this._numSignUps = existing.Count;
                }
            }

            this.OnPropertyChanged("SignUps");
            this.OnPropertyChanged("NumSignUps");
        }

        /// <summary>
        /// Returns true if the indicated person is signed up to self (i.e. the SignUps member includes
        /// that person).
        /// </summary>
        /// <param name="userHandle"></param>
        /// <returns></returns>
        public bool IsSignedUp(string userHandle)
        {
            /* consider - should I use this test instead?
            if (string.IsNullOrEmpty(userHandle)) // no signups - the criterion can't be met
                return false;
            if (!SignUps.Contains(',')) // just one signup - check whether it matches the crierion
            {
                if (SignUps != userHandle)
                    return false;

            }
            else // session has multiple signups, in a comma separated list; the check is more intricate
            {
                bool matchedLast = SignUps.EndsWith(", " + userHandle);
                bool matchedFirst = SignUps.StartsWith(userHandle + ", ");
                bool matchedOther = SignUps.Contains(", " + userHandle + ", ");
                if (!matchedFirst && !matchedLast && !matchedOther)
                    return false;
            }
            return true;
            */

            if (string.IsNullOrEmpty(this._signUps))
                return false;
            else if (!this.SignUps.Contains(','))
            {
                return this.SignUps.Trim() == userHandle;
            }
            else
            {
                List<string> existing = new List<string>(this._signUps.Split(','));
                foreach(string e in existing)
                {
                    if (e.Trim() == userHandle)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Minimum number of players sought for this session.  The ViewModel doesn't do any domain/range checking based
        /// on this value - it is informational only.
        /// </summary>
        public int SitsMinimum
        {
            get
            {
                return this._sitsMinimum;
            }
            set
            {
                this._sitsMinimum = value;
                this.OnPropertyChanged("SitsMinimum");
            }
        }
        private int _sitsMinimum;

        /// <summary>
        /// Maximum number of players sought for this session.  The ViewModel doesn't do any domain/range checking based
        /// on this value - it is informational only.
        /// </summary>
        public int SitsMaximum
        {
            get
            {
                return this._sitsMaximum;
            }
            set
            {
                this._sitsMaximum = value;
                this.OnPropertyChanged("SitsMaximum");
            }
        }
        private int _sitsMaximum;

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
                if (value == "OPEN" || value == "FULL" || value == "ABANDONED")
                {
                    this._state = value;
                    this.OnPropertyChanged("State");
                }
                else
                {
                    Logger.LogMessage(Logger.Level.ERROR, "Session.State set accessor", "invalid state:'" + value + "'");
                    throw new ArgumentException("Session.State set accessor: invalid state:'" + value + "'");
                }
            }
        }
        private string _state="OPEN"; // games start in the OPEN state

        /// <summary>
        /// Number of entries in the SignUps list (the only places this is set, and the only places that
        /// calls its OnPropertyChanged, are the set accessor for SignUps and the
        /// AddSignUp and RemoveSignUp methods on this class)
        /// </summary>
        public int NumSignUps
        {
            get
            {
                return this._numSignUps;
            }
        }
        private int _numSignUps = 0;
    }
}
