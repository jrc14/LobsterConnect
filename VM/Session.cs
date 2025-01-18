using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.VM
{
    internal class Session : LobsterConnect.VM.BindableBase
    {
        private string _id;
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
    }
}
