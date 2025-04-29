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

using LobsterConnect.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LobsterConnect.VM
{
    /// <summary>
    /// A person that the app knows about.
    /// Note that its member variables have public set accessors and are bindable
    /// but UI code should not use those accessors to change their values, because doing so will
    /// bypass the journal mechanism (so changes won't be saved and won't be propagated to the
    /// cloud storage).
    /// You should create and modify instances of this class only on the UI thread; it is not thread-safe.
    /// </summary>
    public class Person : LobsterConnect.VM.BindableBase
    {
        /// <summary>
        /// The handle (user id) of this person.  Attempts to set it to a value containing a comma, a
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.
        /// </summary>
        public string Handle
        {
            get
            {
                return this._handle;
            }
            set
            {
                if (this._handle != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._handle == "")
                        dontNotify = true;
                    if (value == "" && this._handle == null)
                        dontNotify = true;

                    this._handle = value;
                    if (this._handle.Contains(','))
                    {
                        this._handle = this._handle.Replace(',','_') ;
                    }
                    if (this._handle.Contains('\\'))
                    {
                        this._handle = this._handle.Replace('\\', '_');
                    }
                    if (this._handle.Contains('|'))
                    {
                        this._handle = this._handle.Replace('|', '_');
                    }
                    if (this._handle.Contains('\n'))
                    {
                        this._handle = this._handle.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Handle");
                    }
                }
            }
        }
        private string _handle;

        /// <summary>
        /// The full name of this person. Attempts to set it to a value containing a 
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.
        /// </summary>
        public string FullName
        {
            get
            {
                return this._fullName;
            }
            set
            {
                if (this._fullName != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._fullName == "")
                        dontNotify = true;
                    if (value == "" && this._fullName == null)
                        dontNotify = true;

                    this._fullName = value;

                    if (this._fullName.Contains('\\'))
                    {
                        this._fullName = this._fullName.Replace('\\', '_');
                    }
                    if (this._fullName.Contains('|'))
                    {
                        this._fullName = this._fullName.Replace('|', '_');
                    }
                    if (this._fullName.Contains('\n'))
                    {
                        this._fullName = this._fullName.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("FullName");
                    }
                }
            }
        }
        private string _fullName;

        /// <summary>
        /// The phone number of this person.  Attempts to set it to a value containing a 
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.
        /// </summary>
        public string PhoneNumber
        {
            get
            {
                return this._phoneNumber;
            }
            set
            {
                if (this._phoneNumber != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._phoneNumber == "")
                        dontNotify = true;
                    if (value == "" && this._phoneNumber == null)
                        dontNotify = true;

                    this._phoneNumber = value;

                    if (this._phoneNumber.Contains('\\'))
                    {
                        this._phoneNumber = this._phoneNumber.Replace('\\', '_');
                    }
                    if (this._phoneNumber.Contains('|'))
                    {
                        this._phoneNumber = this._phoneNumber.Replace('|', '_');
                    }
                    if (this._phoneNumber.Contains('\n'))
                    {
                        this._phoneNumber = this._phoneNumber.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("PhoneNumber");
                    }
                }
            }
        }
        private string _phoneNumber;

        /// <summary>
        /// The email of this person.  Attempts to set it to a value containing a 
        /// backslash, a vertical bar or a newline will result in a value where the offending character
        /// is replaced by '_'.
        /// </summary>
        public string Email
        {
            get
            {
                return this._email;
            }
            set
            {
                if (this._email != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._email == "")
                        dontNotify = true;
                    if (value == "" && this._email == null)
                        dontNotify = true;

                    this._email = value;

                    if (this._email.Contains('\\'))
                    {
                        this._email = this._email.Replace('\\', '_');
                    }
                    if (this._email.Contains('|'))
                    {
                        this._email = this._email.Replace('|', '_');
                    }
                    if (this._email.Contains('\n'))
                    {
                        this._email = this._email.Replace('\n', '_');
                    }

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Email");
                    }
                }
            }
        }
        private string _email;

        /// <summary>
        /// The hashed password of this person
        /// </summary>
        public string Password
        {
            get
            {
                return this._password;
            }
            set
            {
                if (this._password != value)
                {
                    bool dontNotify = false;
                    if (value == null && this._password == "")
                        dontNotify = true;
                    if (value == "" && this._password == null)
                        dontNotify = true;

                    this._password = value;

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Password");
                    }
                }
            }
        }
        private string _password;
    

        /// <summary>
        /// True if this person is active (we don't ever delete a person, we just deactivate them, if we
        /// want them to no longer be capable of doing things)
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
        private bool _isActive;


        /// <summary>
        /// True if this person is grated admin rights, meaning the UI will let them change things belonging to other people
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                return this._isAdmin;
            }
            set
            {
                if (this._isAdmin != value)
                {
                    this._isAdmin = value;

                    this.OnPropertyChanged("IsAdmin");

                }
            }
        }
        private bool _isAdmin;
    }
}
