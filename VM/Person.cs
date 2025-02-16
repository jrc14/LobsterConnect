using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.VM
{
    public class Person : LobsterConnect.VM.BindableBase
    {

        public static bool CheckHandle(string h)
        {
            return MainViewModel.Instance.CheckPersonHandleExists(h);
        }

        /// <summary>
        /// The handle (user id) of this person
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

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("Handle");
                    }
                }
            }
        }
        private string _handle;

        /// <summary>
        /// The full name of this person
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

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("FullName");
                    }
                }
            }
        }
        private string _fullName;

        /// <summary>
        /// The phone number of this person
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

                    if (!dontNotify)
                    {
                        this.OnPropertyChanged("PhoneNumber");
                    }
                }
            }
        }
        private string _phoneNumber;

        /// <summary>
        /// The email of this person
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
    }
}
