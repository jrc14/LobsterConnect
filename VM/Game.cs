using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.VM
{
    public class Game : LobsterConnect.VM.BindableBase
    {
        /// <summary>
        /// The name of this game
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

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Name");
                    }
                }
            }
        }
        private string _name;

        /// <summary>
        /// Link to BGG entry for this game
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
    }
}
