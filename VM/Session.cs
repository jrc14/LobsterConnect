using LobsterConnect.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.VM
{
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

            return 0;
        }

        /// <summary>
        /// Opaque Id, in case I need one
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
        /// Person handle of the person proposing this gaming session
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

                    if (!Person.CheckHandle(value))
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "Session.Proposer set accessor", "invalid person:'" + value + "'");
                        throw new ArgumentException("Session.Proposer set accessor: invalid person:'" + value + "'");
                    }

                    this._proposer = value;

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Proposer");
                    }
                }
            }
        }
        private string _proposer;

        /// <summary>
        /// The full name of the game that is to be played
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

                    if (!Game.CheckName(value))
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "Session.ToPlay set accessor", "invalid game:'" + value + "'");
                        throw new ArgumentException("Session.ToPlay set accessor: invalid game:'" + value + "'");
                    }

                    this._toPlay = value;

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("ToPlay");
                    }
                }
            }
        }
        private string _toPlay;

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
        /// Human-readable notes about this session
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

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Notes");
                    }
                }
            }
        }
        private string _notes;

        /// <summary>
        /// Link to WhatsApp chat for this session
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

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("WhatsAppLink");
                    }
                }
            }
        }
        private string _whatsAppLink;

        /// <summary>
        /// Link to BGG entry for the game played in this session
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

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("BggLink");
                    }
                }
            }
        }
        private string _bggLink;

        /// <summary>
        /// Comma-separated list of person handles, being the people signed up for this session
        /// </summary>
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
                    if(string.IsNullOrEmpty(value)) // no person handles
                    {
                        this._signUps = "";
                        this._numSignUps = 0;
                        this._signUps = value;
                    }
                    else if( !value.Contains(',')) // one person handle
                    {
                        string handle = value.Trim();
                        if(!Person.CheckHandle(handle))
                        {
                            Logger.LogMessage(Logger.Level.ERROR, "Session.SignUps set accessor", "invalid person:'" + value + "'");
                            throw new ArgumentException("Session.SignUps set accessor: invalid person:'" + value + "'");
                        }
                        this._signUps = handle;
                        this._numSignUps = 1;
                    }
                    else // the list contains more than one person handle
                    {
                        int hh = 0;
                        string handles = "";
                        foreach(string h in value.Split(','))
                        {
                            string handle = h.Trim();
                            if(string.IsNullOrEmpty(handle))
                            {
                                Logger.LogMessage(Logger.Level.ERROR, "Session.SignUps set accessor", "list contains NULL person");
                                throw new ArgumentException("Session.SignUps set accessor: list contains NULL person'");
                            }
                            if (!Person.CheckHandle(handle))
                            {
                                Logger.LogMessage(Logger.Level.ERROR, "Session.SignUps set accessor", "list contains invalid person:'" + handle + "'");
                                throw new ArgumentException("Session.SignUps set accessor: list contains invalid person:'" + handle + "'");
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

                    this._signUps = value;

                    this.OnPropertyChanged("SignUps");
                    this.OnPropertyChanged("NumSignUps");
                }
            }
        }
        private string _signUps="";

        /// <summary>
        /// Number of entries in the SignUps list (the only place this is set, and the only place that
        /// calls its OnPropertyChanged, is the set access for SignUps
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
