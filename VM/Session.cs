using System;
using System.Collections.Generic;
using System.Linq;
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

        public string Proposer // a player handle
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
                    }
                }
            }
        }
        private string _proposer;

        public string ToPlay // a game full name
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
                    }
                }
            }
        }
        private string _toPlay;

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
    }
}
