using System.ComponentModel;
using LobsterConnect.Model;

namespace LobsterConnect.VM
{
    public class MainViewModel : LobsterConnect.VM.BindableBase
    {
        /// <summary>
        /// The single instance of the MainViewModel class.  Manipulate the state and contents of gaming events, games,
        /// persons and gaming sessions by calling methods on this class.
        /// </summary>
        public static MainViewModel Instance
        {
            get
            {
                lock (_InstanceLock)
                {
                    if (_Instance == null)
                    {
                        MainViewModel i = new MainViewModel();
                        _Instance = i;
                    }
                    return _Instance;
                }
            }
        }

        /// <summary>
        /// Initialise the main view model by loading Gaming Events, Persons, Games and Sessions (in due course this will come from
        /// the local journal store and the cloud; initially I'm just creating some test data).
        /// It will load all Gaming Events, Persons and Games, because these exist independent of any particular gaming event.
        /// It only loads the Sessions appropriate for one particular gaming event (chosen using logic that is still TBC).
        /// This method also sets up the event handler for PropertyChanged events, to detect situations when the UI
        /// needs to refresh its session list, and to fire SessionsMustBeRefreshed in those situations
        /// </summary>
        public void Load()
        {
            if(_isLoaded)
            {
                Logger.LogMessage(Logger.Level.WARNING, "MainViewModel.Load", "attempting to Load the viewmodel twice is an error");
                return;
            }
            this._isLoaded = true;

            LoadEventsGamesAndPersons();

            SetCurrentEvent("LoBsterCon XXVIII");

            LoadSessionsAndSignUps(MainViewModel.Instance.CurrentEvent.Name);

            // We set this handler after calling SetCurrentEvent above, to avoid a redundant call to
            // LoadSessionsAndSignUps and the firing of SessionsMustBeRefreshed
            this.PropertyChanged += MainViewModel_PropertyChanged;
        }
        private bool _isLoaded = false;

        private void LoadEventsGamesAndPersons()
        {
            // Load 500 top games into the local store (without writing them to the journal or the cloud store, which
            // would be pointless being as every time the app starts these games get loaded).
            CreateDefaultGames();

            CreateGamingEvent(false, "LoBsterCon XXVIII", "CONVENTION");
            CreateGamingEvent(false, "2025-03-02 Sun", "DAY");
            CreateGamingEvent(false, "2025-03-03 Mon", "EVENING");
            CreateGamingEvent(false, "2025-03-04 Tue", "EVENING");

            CreatePerson(false, "jrc14", password: Model.Utilities.PasswordHash("jrc14"));

            // Create 500 random pixies
            for (int i=0; i<500; i++)
            {
                switch (i%5)
                {
                    case 0:
                        CreatePerson(false, "bobby" + i.ToString(), "Bobby the Magic Pixie number " + i.ToString(),
                            "+44-20-555-" + i.ToString("D4"),
                            "bobby" + i.ToString() + "@pixienet.com",
                            Model.Utilities.PasswordHash("bobby" + i.ToString()));
                        break;
                    case 1:
                        CreatePerson(false, "susan" + i.ToString(), "Susan the Magic Pixie number " + i.ToString(),
                            "+44-20-7555-" + i.ToString("D4"),
                            "susan" + i.ToString() + "@pixienet.com",
                            Model.Utilities.PasswordHash("susan" + i.ToString()));
                        break;
                    case 2:
                        CreatePerson(false, "steve" + i.ToString(), "Steve the Magic Pixie number " + i.ToString(),
                            "+44-20-7555-" + i.ToString("D4"),
                            "steve" + i.ToString() + "@pixienet.com",
                            Model.Utilities.PasswordHash("steve" + i.ToString()));
                        break;
                    case 3:
                        CreatePerson(false, "jack" + i.ToString(), "Jack the Magic Pixie number " + i.ToString(),
                            "+44-20-7555-" + i.ToString("D4"),
                            "jack" + i.ToString() + "@pixienet.com",
                            Model.Utilities.PasswordHash("jack" + i.ToString()));
                        break;
                    default:
                        CreatePerson(false, "mick" + i.ToString(), "Mick the Magic Pixie number " + i.ToString(),
                            "+44-20-7555-" + i.ToString("D4"),
                            "mick" + i.ToString() + "@pixienet.com",
                            Model.Utilities.PasswordHash("mick" + i.ToString()));
                        break;
                }
            }    
        }

        private void LoadSessionsAndSignUps(string eventName)
        {
            this._sessions.Clear();

            int numSessions = 15;
            if (eventName == "LoBsterCon XXVIII")
                numSessions = 100;

            for(int s=0; s<numSessions; s++)
            {
                string proposer = this._persons[System.Random.Shared.Next(0, this._persons.Count)].Handle;
                string toPlay = this._games[System.Random.Shared.Next(0, this._games.Count)].Name;
                int sitsMinimum = System.Random.Shared.Next(2, 6);
                int sitsMaximum = sitsMinimum+ System.Random.Shared.Next(0, 4);
                int sessionTime = System.Random.Shared.Next(0, SessionTime.NumberOfTimeSlots);

                string sessionId = CreateSession(false, proposer, toPlay, eventName, new SessionTime(sessionTime), false, "Here are some notes for session number " + s.ToString(),
                    sitsMinimum: sitsMinimum, sitsMaximum: sitsMaximum);

                Session session = GetSession(sessionId);
                for(int p = 0; p<sitsMaximum;p++)
                {
                    if (System.Random.Shared.Next(0, 2) == 0)
                    {
                        string person = this._persons[System.Random.Shared.Next(0, this._persons.Count)].Handle;
                        if(!session.IsSignedUp(person))
                        {
                            SignUp(false, person, sessionId, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Special treatment for any properties of the viewmodel where we want application-specific things to happen
        /// in response to the property being changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If the user switches to a different gaming event, the sessions and signups collections have to be
            // emptied and reloaded with the content for the new event.
            if(e.PropertyName=="CurrentEvent")
            {
                if (this._currentEvent != null && !string.IsNullOrEmpty(this._currentEvent.Name))
                {
                    // Load sessions and sign-ups applicable to the new event
                    LoadSessionsAndSignUps(this._currentEvent.Name);

                    // Fire the event that will tell the UI to reload sessions
                    this.SessionsMustBeRefreshed?.Invoke(this, new EventArgs());
                }
                return;
            }

            // If the filter is changed, we fire the event that will tell the UI to reload sessions
            if (e.PropertyName=="CurrentFilter")
            {
                this.SessionsMustBeRefreshed?.Invoke(this, new EventArgs());
                return;
            }
        }

        /// <summary>
        /// Create a game.  The only information needed is game name.  There also an optional BoardGameGeek link, and an active flag
        /// An exception is thrown if the game name is null, or a game of that name is already in the games collection.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="name">manadatory and must be unique</param>
        /// <param name="bggLink">optional link text defaulted to "NO LINK"</param>
        /// <param name="isActive">active flag defaulted to true</param>
        /// <exception cref="ArgumentException"></exception>
        public void CreateGame(bool informJournal, string name, string bggLink = null, bool? isActive=null)
        {
            if (!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateGame", "Coding bug: Should be called on the UI thread");
                Model.DispatcherHelper.RunAsyncOnUI(() => CreateGame(informJournal, name, bggLink, isActive));
            }
            else
            {
                if (name == null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateGame", "game name cannot be null");
                    throw new ArgumentException("MainViewModel.CreateGame: null name");
                }
                if (bggLink == null)
                {
                    bggLink = "NO LINK";
                }
                if (isActive == null)
                {
                    isActive = true;
                }

                lock (_gamesLock)
                {
                    if (CheckGameNameExists(name))
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateGame", "game with that name already exists:'" + name + "'");
                        throw new ArgumentException("MainViewModel.CreateGame: duplicate name:'" + name + "'");
                    }
                    _games.Add(new Game() { Name = name, BggLink = bggLink, IsActive = (bool)isActive });
                }

                if (informJournal)
                {
                    Journal.AddJournalEntry(Journal.EntityType.Game, Journal.OperationType.Create, name,
                        "ISACTIVE", ((bool)isActive).ToString(),
                        "BGGLINK", bggLink);
                }
            }
        }

        /// <summary>
        /// Update attributes of a game, informing the journal if necessary.  Any attributes you don't want to update should be
        /// set to null.
        /// This method is the correct way to amend a game object in response to UI actions, because it will inform the journal, meaning
        /// that updates will be saved locally and passed on to the cloud store.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="game">mandatory, full name of an existing game to be updated</param>
        /// <param name="bggLink"></param>
        /// <param name="isActive"></param>
        public void UpdateGame(bool informJournal, Game game, string bggLink=null, bool? isActive=null)
        {
            if (!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.UpdateGame", "Coding bug: Should be called on the UI thread");
                Model.DispatcherHelper.RunAsyncOnUI(() => UpdateGame(informJournal, game, bggLink, isActive));
            }
            else
            {
                if (bggLink != null)
                {
                    game.BggLink = bggLink;
                }

                if (isActive != null)
                {
                    game.IsActive = (bool)isActive;
                }

                if (informJournal)
                {
                    List<string> journalParameters = new List<string>();

                    if (bggLink != null)
                    {
                        journalParameters.Add("BGGLINK"); journalParameters.Add(bggLink);
                    }
                    if (isActive != null)
                    {
                        journalParameters.Add("ISACTIVE"); journalParameters.Add(((bool)isActive).ToString());
                    }

                    if (journalParameters.Count > 0)
                    {
                        Journal.AddJournalEntry(Journal.EntityType.Game, Journal.OperationType.Update, game.Name, journalParameters);
                    }
                }
            }
        }

        /// <summary>
        /// Create a person.  The only information needed is the person handle; all other attributes are optional.
        /// An exception is thrown if the handle is null, or if a person having that handle already exists in the
        /// persons collection.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="handle">mandatory and must be unique</param>
        /// <param name="fullName">optional defaulted to "NO FULL NAME"</param>
        /// <param name="phoneNumber">optional defaulted to "NO PHONE NUMBER"</param>
        /// <param name="email">optional defaulted to "NO EMAIL"</param>
        /// <param name="password">password hash string, if there is a password, or "" if there isn't.  It's optional, and
        /// defaulted to ""</param>
        /// <param name="isActive">active flag defaulted to true</param>
        /// <exception cref="ArgumentException"></exception>
        public void CreatePerson(bool informJournal, string handle, string fullName = null, string phoneNumber = null, string email = null, string password = null, bool? isActive=null)
        {
            if (!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreatePerson", "Coding bug: Should be called on the UI thread");
                Model.DispatcherHelper.RunAsyncOnUI(() => CreatePerson(informJournal, handle, fullName, phoneNumber, email, password, isActive));
            }
            {
                if (handle == null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreatePerson", "person handle cannot be null'");
                    throw new ArgumentException("MainViewModel.CreatePerson: null handle");
                }
                else
                {
                    if (fullName == null)
                    {
                        fullName = "NO NAME";
                    }
                    if (phoneNumber == null)
                    {
                        phoneNumber = "NO PHONE NUMBER";
                    }
                    if (email == null)
                    {
                        email = "NO EMAIL";
                    }
                    if (password == null)
                    {
                        password = "";
                    }
                    if (isActive == null)
                    {
                        isActive = true;
                    }

                    lock (_personsLock)
                    {
                        if (CheckPersonHandleExists(handle))
                        {
                            Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreatePerson", "person with that handle already exists:'" + handle + "'");
                            throw new ArgumentException("MainViewModel.CreatePerson: duplicate handle:'" + handle + "'");
                        }
                        _persons.Add(new Person() { Handle = handle, FullName = fullName, PhoneNumber = phoneNumber, Email = email, Password = password, IsActive = (bool)isActive });
                    }

                    if (informJournal)
                    {
                        Journal.AddJournalEntry(Journal.EntityType.Person, Journal.OperationType.Create, handle,
                            "FULLNAME", fullName,
                            "PHONENUMBER", phoneNumber,
                            "EMAIL", email,
                            "PASSWORD", password,
                            "ISACTIVE", ((bool)isActive).ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Update attributes of a person, informing the journal if necessary.  Any attributes you don't want to update should be
        /// set to null.
        /// This method is the correct way to amend a person object in response to UI actions, because it will inform the journal, meaning
        /// that updates will be saved locally and passed on to the cloud store.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="person">mandatory, handle of an existing person to be updated</param>
        /// <param name="fullName"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="isActive"></param>
        public void UpdatePerson(bool informJournal, Person person, string fullName = null, string phoneNumber = null, string email = null, string password = null, bool? isActive=null)
        {
            if (!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.UpdatePerson", "Coding bug: Should be called on the UI thread");
                Model.DispatcherHelper.RunAsyncOnUI(() => UpdatePerson(informJournal, person, fullName, phoneNumber, email, password, isActive));
            }
            else
            {
                if (fullName != null)
                    person.FullName = fullName;
                if (phoneNumber != null)
                    person.PhoneNumber = phoneNumber;
                if (email != null)
                    person.Email = email;
                if (password != null)
                    person.Password = password;

                if(informJournal)
                {
                    List<string> journalParameters = new List<string>();

                    if (fullName != null)
                    {
                        journalParameters.Add("FULLNAME"); journalParameters.Add(fullName);
                    }
                    if (phoneNumber != null)
                    {
                        journalParameters.Add("PHONENUMBER"); journalParameters.Add(phoneNumber);
                    }
                    if (email != null)
                    {
                        journalParameters.Add("EMAIL"); journalParameters.Add(email);
                    }
                    if (password != null)
                    {
                        journalParameters.Add("PASSWORD"); journalParameters.Add(password);
                    }

                    if (isActive != null)
                    {
                        journalParameters.Add("ISACTIVE"); journalParameters.Add(((bool)isActive).ToString());
                    }

                    Journal.AddJournalEntry(Journal.EntityType.Person, Journal.OperationType.Update, person.Handle, journalParameters);
                }
            }
        }

        /// <summary>
        /// Create a game session that people will be able to sign up for, at the event named in the eventName parameter.
        /// The person proposing the game is identified by the proposerHandle parameter, the game is identified by
        /// the gameNameToPlay parameter.  The starting time of the session is provided by the startAt parameter.
        /// Other parameters are optional.
        /// The method will throw an exception if the proposer handle isn't found in the persons collection or if it
        /// corresponds to an inactive person, or if the game name is not found in the games collection, or if the gaming
        /// event is not recognised.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="proposerHandle">mandatory, handle of an existing person</param>
        /// <param name="gameNameToPlay">mandatory, name of an existing game</param>
        /// <param name="eventName">mandatory, name of an existing gaming event</param>
        /// <param name="startAt">mandatory, the time when the session starts</param>
        /// <param name="checkActive">mandatory, whether to throw an exception or just log a user warning in the case of an
        /// inactive game or inactive person. If calling this method from the UI, 'true' is appropriate; if you're replaying a
        /// journal from the cloud store, 'false' might be a better choice.</param>
        /// <param name="notes">optional, defaulted to "NO NOTES"</param>
        /// <param name="whatsAppLink">optional, defaulted to "NO WHATSAPP CHAT"</param>
        /// <param name="sitsMinimum">optional, defaulted to 0</param>
        /// <param name="sitsMaximum">optional, defaulted to 0</param>
        /// <param name="state">optional, defaulted to "OPEN"</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public string CreateSession(bool informJournal, string proposerHandle, string gameNameToPlay, string eventName, SessionTime startAt, bool checkActive, string notes=null, string whatsAppLink=null, int sitsMinimum=0, int sitsMaximum=0, string state=null)
        {
            if (!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateSession", "Coding bug: Should be called on the UI thread");
                throw new Exception("MainViewModel.CreateSession: Coding bug: Should be called on the UI thread");
            }
            else
            {
                Person proposer = null;
                Game game = null;

                if (notes == null)
                {
                    notes = "NO NOTES";
                }

                if (whatsAppLink == null)
                {
                    whatsAppLink = "NO WHATSAPP CHAT";
                }

                if(state==null)
                {
                    state = "OPEN";
                }

                string id = Guid.NewGuid().ToString();

                proposer = GetPerson(proposerHandle);
                if (proposer == null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateSession", "no such person:'" + proposerHandle + "'");
                    throw new ArgumentException("MainViewModel.CreateSession: no such person:'" + proposerHandle + "'");
                }

                game = GetGame(gameNameToPlay);
                if (game == null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateSession", "no such game:'" + gameNameToPlay + "'");
                    throw new ArgumentException("MainViewModel.CreateSession: no such game:'" + gameNameToPlay + "'");
                }

                if(!CheckEventName(eventName))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateSession", "no such gaming event:'" + eventName + "'");
                    throw new ArgumentException("MainViewModel.CreateSession: no such gaming event:'" + eventName + "'");
                }

                lock (game.instanceLock) // guard against the race condition 'game gets made inactive while session is being created'
                {
                    if (!game.IsActive)
                    {
                        if (checkActive)
                        {
                            Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateSession", "game is not active:'" + gameNameToPlay + "'");
                            throw new ArgumentException("MainViewModel.CreateSession: game is not active:'" + gameNameToPlay + "'");
                        }
                        else
                        {
                            LogUserMessage(Logger.Level.WARNING, "Create Session: creating a session for an inactive game: '" + gameNameToPlay + "'");
                        }
                    }

                    lock (proposer.instanceLock) // guard against the race condition 'proposer gets made inactive while session is being created'
                    {
                        if (!proposer.IsActive)
                        {
                            if (checkActive)
                            {
                                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateSession", "proposer is not active:'" + proposerHandle + "'");
                                throw new ArgumentException("MainViewModel.CreateSession: proposer is not active:'" + proposerHandle + "'");
                            }
                            else
                            {
                                LogUserMessage(Logger.Level.WARNING, "Create Session: creating a session proposed by an inactive person: '" + proposerHandle + "'");
                            }
                        }

                        _sessions.Add(new Session()
                        {
                            Id = id,
                            Proposer = proposerHandle,
                            ToPlay = gameNameToPlay,
                            EventName = eventName,
                            StartAt = startAt,
                            Notes = notes,
                            WhatsAppLink = whatsAppLink,
                            BggLink = game.BggLink,
                            SignUps = "",// will by side-effect set NumSignUps to 0
                            SitsMinimum = sitsMinimum,
                            SitsMaximum = sitsMaximum,
                            State = state
                        });
                    }
                }

                // Adding a new session always means that the UI display of sessions must be refreshed (but because we might
                // sometimes add a lot of sessions in quick succession, make sure we don't fire the 'refresh' event every
                // single time).
                Model.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    ThrottledFireSessionsRefreshEvent();
                });

                if(informJournal)
                {
                    Journal.AddJournalEntry(Journal.EntityType.Session, Journal.OperationType.Create, id,
                        "EVENTNAME", eventName,
                        "PROPOSER", proposerHandle,
                        "TOPLAY", gameNameToPlay,
                        "STARTAT", startAt.ToString()+":"+startAt.Ordinal.ToString(),
                        "NOTES", notes,
                        "WHATSAPPLINK", whatsAppLink,
                        "BGGLINK", game.BggLink,
                        "SITSMINIMUM", sitsMinimum.ToString(),
                        "SITSMAXIMUM", sitsMaximum.ToString(),
                        "STATE", state);
                }

                return id;
            }
        }

        /// <summary>
        /// Signs a person up to play in a session.  The method throws an exception if the person handle or session id is invalid.
        /// The checkActive parameter defines the method's behaviour when the person is not active or the session is not open
        /// An exception is also thrown (from Session.AddSignUp) if the person was already signed up.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="personHandle">mandatory, the person to sign up </param>
        /// <param name="sessionId">mandatory, the id of the session</param>
        /// <param name="checkActive">mandatory, whether to throw an exception or just log a user warning in the case of a
        /// non-OPEN game or inactive person. If calling this method from the UI, 'true' is appropriate; if you're replaying a
        /// journal from the cloud store, 'false' might be a better choice.</param>
        /// <param name="modifiedBy">the handle of the person who's adding the sign-up, if it isn't the person playing</param>
        /// <exception cref="ArgumentException"></exception>
        public void SignUp(bool informJournal, string personHandle, string sessionId, bool checkActive, string modifiedBy=null)
        {
            if (!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SignUp", "Coding bug: Should be called on the UI thread");
                Model.DispatcherHelper.RunAsyncOnUI(() => SignUp(informJournal, personHandle, sessionId, checkActive, modifiedBy));
            }
            else
            {
                Person person = GetPerson(personHandle);
                Session session = GetSession(sessionId);

                if (person == null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SignUp", "no such person:'" + personHandle + "'");
                    throw new ArgumentException("MainViewModel.SignUp: no such person:'" + personHandle + "'");
                }

                if (session == null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SignUp", "no such session:'" + sessionId + "'");
                    throw new ArgumentException("MainViewModel.SignUp: no such session:'" + sessionId + "'");
                }

                lock (person.instanceLock)
                {
                    if (!person.IsActive)
                    {
                        if (checkActive)
                        {
                            Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SignUp", "person is not active:'" + personHandle + "'");
                            throw new ArgumentException("MainViewModel.SignUp: person is not active:'" + personHandle + "'");
                        }
                        else
                        {
                            LogUserMessage(Logger.Level.WARNING, "Sign-up: an inactive person '" + personHandle + "' is being signed up to play '" + session.ToPlay + "'");
                        }
                    }
                    lock (session.instanceLock)
                    {
                        if (session.State != "OPEN")
                        {
                            if (checkActive)
                            {
                                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SignUp", "session is not OPEN:'" + sessionId + "'");
                                throw new ArgumentException("MainViewModel.SignUp: session is not OPEN:'" + sessionId + "'");
                            }
                            else
                            {
                                LogUserMessage(Logger.Level.WARNING, "Sign-up: '" + personHandle + "' is being signed up to play a non-OPEN session of '" + session.ToPlay + "'");
                            }
                        }

                        session.AddSignUp(person.Handle);
                    }
                }

                // If there is a filter active, then changing a session's sign-ups could affect whether or not
                // a certain session is visible, if the active filter criteria include sign-ups.
                if (CurrentFilter != null)
                {
                    if (!string.IsNullOrEmpty(CurrentFilter.SignUpsInclude))
                    {
                        Model.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            ThrottledFireSessionsRefreshEvent();
                        });
                    }
                }

                if (informJournal)
                {
                    if (modifiedBy == null)
                        modifiedBy = personHandle;

                    Journal.AddJournalEntry(Journal.EntityType.SignUp, Journal.OperationType.Create, personHandle + "," + sessionId,
                        "EVENTNAME", session.EventName,
                        "MODIFIEDBY", modifiedBy);
                }
            }
        }

        /// <summary>
        /// Cancels an existing signup for a person to play in a session.  The method throws an exception if the person handle or session id is invalid.
        /// An exception is also thrown (from Session.RemoveSignUp) if the person was not signed up.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="personHandle"></param>
        /// <param name="sessionId"></param>
        /// <param name="checkActive">mandatory, whether to throw an exception or just log a user warning in the case of a
        /// non-OPEN game or inactive person. If calling this method from the UI, 'true' may be appropriate; if you're replaying
        /// a journal from the cloud store, 'false' might be a better choice.</param>
        /// <param name="modifiedBy">the handle of the person who's removing the sign-up, if it isn't the person playing</param>
        /// <exception cref="ArgumentException"></exception>
        public void CancelSignUp(bool informJournal, string personHandle, string sessionId, bool checkActive, string modifiedBy = null)
        {
            if (!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CancelSignUp", "Coding bug: Should be called on the UI thread");
                Model.DispatcherHelper.RunAsyncOnUI(() => CancelSignUp(informJournal, personHandle, sessionId, checkActive, modifiedBy));
            }
            else
            {
                Person person = GetPerson(personHandle);
                Session session = GetSession(sessionId);

                if (person == null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CancelSignUp", "no such person:'" + personHandle + "'");
                    throw new ArgumentException("MainViewModel.CancelSignUp: no such person:'" + personHandle + "'");
                }

                if (session == null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CancelSignUp", "no such session:'" + sessionId + "'");
                    throw new ArgumentException("MainViewModel.CancelSignUp: no such session:'" + sessionId + "'");
                }


                lock (person.instanceLock)
                {
                    if (!person.IsActive)
                    {
                        if (checkActive)
                        {
                            Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CancelSignUp", "person is not active:'" + personHandle + "'");
                            throw new ArgumentException("MainViewModel.CancelSignUp: person is not active:'" + personHandle + "'");
                        }
                        else
                        {
                            LogUserMessage(Logger.Level.WARNING, "Cancel sign-up: an inactive person '" + personHandle + "' is being removed from '" + session.ToPlay + "'");
                        }
                    }
                    lock (session.instanceLock)
                    {
                        if (checkActive)
                        {
                            if (session.State != "OPEN")
                            {
                                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CancelSignUp", "session is not OPEN:'" + sessionId + "'");
                                throw new ArgumentException("MainViewModel.CancelSignUp: session is not OPEN:'" + sessionId + "'");
                            }
                            else
                            {
                                LogUserMessage(Logger.Level.WARNING, "Cancel sign-up: '" + personHandle + "' is being removed from a non-OPEN session of '" + session.ToPlay + "'");
                            }
                        }

                        session.RemoveSignUp(person.Handle);
                    }
                }

                // If there is a filter active, then changing a session's sign-ups could affect whether or not
                // a certain session is visible, if the active filter criteria include sign-ups.
                if (CurrentFilter != null)
                {
                    if (!string.IsNullOrEmpty(CurrentFilter.SignUpsInclude))
                    {
                        Model.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            ThrottledFireSessionsRefreshEvent();
                        });
                    }
                }

                if (informJournal)
                {
                    if (modifiedBy == null)
                        modifiedBy = personHandle;

                    Journal.AddJournalEntry(Journal.EntityType.SignUp, Journal.OperationType.Delete, personHandle + "," + sessionId,
                        "EVENTNAME", session.EventName,
                        "MODIFIEDBY", modifiedBy);
                }
            }
        }

        /// <summary>
        /// Update attributes of a session, informing the journal if necessary.  Any attributes you don't want to update should be
        /// set to null.  The state attribute should be one of OPEN, FULL or ABANDONED; if it is not then an exception will be thrown.
        /// This method is the correct way to amend a session object in response to UI actions, because it will inform the journal, meaning
        /// that updates will be saved locally and passed on to the cloud store.
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="session">mandatory, the id of an existing session</param>
        /// <param name="notes"></param>
        /// <param name="whatsAppLink"></param>
        /// <param name="sitsMinimum"></param>
        /// <param name="sitsMaximum"></param>
        /// <param name="state">must be OPEN, FULL or ABANDONED</param>
        public void UpdateSession(bool informJournal, Session session, string notes = null, string whatsAppLink = null, int? sitsMinimum = null, int? sitsMaximum = null, string state=null)
        {
            if (!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.UpdateSession", "Coding bug: Should be called on the UI thread");
                Model.DispatcherHelper.RunAsyncOnUI(() => UpdateSession(informJournal, session, notes, whatsAppLink, sitsMinimum, sitsMaximum, state));
            }
            else
            {
                lock (session.instanceLock)
                {
                    if (notes != null)
                        session.Notes = notes;
                    if (whatsAppLink != null)
                        session.WhatsAppLink = whatsAppLink;
                    if (sitsMinimum != null)
                        session.SitsMinimum = (int)sitsMinimum;
                    if (sitsMaximum != null)
                        session.SitsMinimum = (int)sitsMaximum;
                    if (state != null)
                    {
                        if (state == "OPEN" || state == "FULL" || state == "ABANDONED")
                        {
                            session.State = state;
                        }
                        else
                        {
                            Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.UpdateSession", "invalid state:'" + state + "'");
                            throw new ArgumentException("MainViewModel.UpdateSession: invalid state:'" + state + "'");
                        }
                    }
                }

                // If there is a filter active, then changing a session's state could affect whether or not
                // a certain session is visible, if the active filter criteria include state.
                if (state!=null && CurrentFilter != null)
                {
                    if (!string.IsNullOrEmpty(CurrentFilter.SignUpsInclude))
                    {
                        Model.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            ThrottledFireSessionsRefreshEvent();
                        });
                    }
                }

                if (informJournal)
                {
                    List<string> journalParameters = new List<string>();

                    journalParameters.Add("EVENTNAME");
                    journalParameters.Add(session.EventName);

                    if (notes != null)
                    {
                        journalParameters.Add("NOTES"); journalParameters.Add(notes);
                    }
                    if (whatsAppLink != null)
                    {
                        journalParameters.Add("WHATSAPPLINK"); journalParameters.Add(whatsAppLink);
                    }
                    if (sitsMinimum != null)
                    {
                        journalParameters.Add("SITSMINIMUM"); journalParameters.Add(((int)sitsMinimum).ToString());
                    }
                    if (sitsMinimum != null)
                    {
                        journalParameters.Add("SITSMAXIMUM"); journalParameters.Add(((int)sitsMaximum).ToString());
                    }
                    if (state != null)
                    {
                        journalParameters.Add("STATE"); journalParameters.Add(state);
                    }

                    Journal.AddJournalEntry(Journal.EntityType.Session, Journal.OperationType.Update, session.Id, journalParameters);
                }
            }
        }

        /// <summary>
        /// The filter that is currently applied to restrict the set of sessions that will be displayed in the UI.
        /// Special magic code in the OnPropertyChanged handler for 'CurrentFilter' will take care of making
        /// the UI refresh itself when the filter changes.
        /// </summary>
        public SessionFilter CurrentFilter
        {
            get
            {
                return this._currentFilter;
            }
            set
            {
                this._currentFilter = value;

                this.OnPropertyChanged("CurrentFilter");
            }
        }
        private SessionFilter _currentFilter = new SessionFilter();

        /// <summary>
        /// Sets the current event to a new event.  The name must correspond to an event in the list returned by GetAvailableEventNames()
        /// </summary>
        /// <param name="eventName"></param>
        public void SetCurrentEvent(string eventName)
        {
            if (!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SetCurrentEvent", "Coding bug: Should be called on the UI thread");
                Model.DispatcherHelper.RunAsyncOnUI(() => SetCurrentEvent(eventName));
            }
            else
            {
                if (string.IsNullOrEmpty(eventName))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SetCurrentEvent", "event name cannot be null");
                    throw new ArgumentException("MainViewModel.SetCurrentEvent: null name");
                }

                GamingEvent g = null;
                foreach(GamingEvent gg in _availableEvents)
                {
                    if(gg.Name==eventName)
                    {
                        g = gg;
                        break;
                    }
                }

                if (g==null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SetCurrentEvent", "there is no event having name '"+eventName+"'");
                    throw new ArgumentException("MainViewModel.SetCurrentEvent: invalid name");
                }

                LogUserMessage(Logger.Level.INFO, "Current gaming event has been set to '" + eventName + "'");

                SessionTime.SetEventType(g.EventType); // set the number of gaming slots and their labels, according to the type of gaming event
                this.CurrentEvent = g;
            }
        }

        /// <summary>
        /// Create a new gaming event, at which sessions can be set up
        /// </summary>
        /// <param name="informJournal">set to true if this update should be sent to the journal (i.e. if it resulted from local
        /// UI action); set to false if the journal doesn't need to be told about this update (i.e. if it resulted from
        /// replaying the journal.</param>
        /// <param name="name"></param>
        /// <param name="eventType">DAY, EVENING or CONVENTION - depending on this value, different sign-up slot times will be set up</param>
        public void CreateGamingEvent(bool informJournal, string name, string eventType)
        {
            if (!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateGamingEvent", "Coding bug: Should be called on the UI thread");
                Model.DispatcherHelper.RunAsyncOnUI(() => CreateGamingEvent(informJournal, name, eventType));
            }
            else
            {
                if (string.IsNullOrEmpty(name))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateGamingEvent", "event name cannot be null");
                    throw new ArgumentException("MainViewModel.CreateGamingEvent: null name");
                }

                if(GetAvailableEventNames().Contains(name))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateGamingEvent", "event name is the same as one already in the list");
                    throw new ArgumentException("MainViewModel.CreateGamingEvent: duplicate name");
                }

                if (eventType != "EVENING" && eventType != "DAY" && eventType != "CONVENTION")
                {
                    Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.CreateGamingEvent", "invalid type: '" + eventType + "'");
                    throw new ArgumentException("MainViewModel.CreateGamingEvent: invalid event type");
                }

                GamingEvent g = new GamingEvent() { Name = name, EventType = eventType, IsActive = true };
                this._availableEvents.Add(g);

                if (informJournal)
                {
                    Journal.AddJournalEntry(Journal.EntityType.GamingEvent, Journal.OperationType.Create, name,
                        "ISACTIVE", "True",
                        "EVENTTYPE", eventType);
                }
            }
        }

        /// <summary>
        /// Check that the parameter corresponds to a gaming event that we know about
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns>true if the gaming event is know, false if it is not</returns>
        public bool CheckEventName(string eventName)
        {
            foreach (GamingEvent e in _availableEvents)
            {
                if (eventName == e.Name)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieve a list of events that we can manage signups for.
        /// </summary>
        /// <returns>The names of the events</returns>
        public List<string> GetAvailableEventNames()
        {
            List<string> names = new List<string>();

            foreach(GamingEvent e in _availableEvents)
            {
                names.Add(e.Name);
            }
            return names;
        }
        private List<GamingEvent> _availableEvents = new List<GamingEvent>();

        /// <summary>
        /// The gaming event that the app is currently managing sign-ups for. It should be set by calling SetCurrentEvent.
        /// </summary>
        public GamingEvent CurrentEvent
        {
            get
            {
                return this._currentEvent;
            }
            private set
            {
                if (this._currentEvent != value)
                {
                    this._currentEvent = value;

                    this.OnPropertyChanged("CurrentEvent");
                }
            }
        }
        private GamingEvent _currentEvent = null;


        /// <summary>
        /// The currently logged on person, or null if no person is logged on.  It should be set by calling
        /// SetLoggedOnUser.
        /// </summary>
        public Person LoggedOnUser
        {
            get
            {
                return this._loggedOnUser;
            }
            private set
            {
                this._loggedOnUser = value;

                this.OnPropertyChanged("LoggedOnUser");
            }
        }
        private Person _loggedOnUser = null;

        /// <summary>
        /// If p is a person, the method logs in and - if parameter 'remember' is true, saves the person handle in an application
        /// setting so they will be logged in again next time the app runs.
        /// If p is null, the user is logged out.  In this case the application setting is always cleared, so that the next time
        /// the app runs no user will be automatically logged n.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="remember"></param>
        public void SetLoggedOnUser(Person p, bool remember=false)
        {
            if(!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.SetLoggedOnUser", "Coding bug: logged on user can only be changed on the UI thread");
                Model.DispatcherHelper.RunAsyncOnUI(() => SetLoggedOnUser(p, remember));
            }
            else
            {
                this.LoggedOnUser = p;

                if (p==null)
                {
                    Microsoft.Maui.Storage.Preferences.Set("UserHandle", "");
                    MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "User has been logged out");
                }
                else
                {
                    MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "User '" + p.Handle + "' has been logged in");
                    if (remember)
                    {
                        Microsoft.Maui.Storage.Preferences.Set("UserHandle", p.Handle);
                        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "User '" + p.Handle + "' has been remembered, and will be logged in next time too");
                    }
                }

            }

        }

        /// <summary>
        /// Constructs an array of all sessions for the given event, filtered by some criteria.
        /// The index into the array is session time (turned into an ordinal number) so the number of entries in th
        /// array will be equal to the number of possible time slots for
        /// that event.  Each entry in the array is a list of sessions, sorted (first by proposer name then by id).
        /// If there are no sessions for a specified time slot, the corresponding array entry will be null.
        /// </summary>
        /// <param name="eventName">only sessions for this event will be included</param>
        /// <param name="filter">only sessions matching this filter will be included</param>
        /// <returns></returns>
        public List<Session>[] GetAllSessions(string eventName, SessionFilter filter)
        {
            int numSlots = SessionTime.NumberOfTimeSlots;

            if(numSlots==0)
            {
                return new List<Session>[0];
            }

            List<Session>[] allSessions = new List<Session>[numSlots];

            lock(this._sessionsLock)
            {
                foreach(Session s in this._sessions)
                {
                    if (s.EventName != eventName)
                        continue;

                    if (!filter.Matches(s))
                        continue;

                    // The sessions we already found for the same time slot as s.  We need to insert s
                    // into the right spot in this list.
                    List<Session> existing = allSessions[s.StartAt.Ordinal];

                    if(existing==null)
                    {
                        // easy case: so far we found no sessions for this slot; just create a new
                        // list for this slot, and this session into the list.
                        allSessions[s.StartAt.Ordinal] = new List<Session>() { s };
                    }
                    else
                    {
                        // starting at 0, advance insertAt until we find an element that is greater than s, or we run
                        // out of elements in the existing list
                        int insertAt = 0;
                        while(insertAt < existing.Count && existing[insertAt].CompareTo(s) <= 0)
                        {
                            insertAt++;
                        }

                        if (insertAt < existing.Count) // There is an existing element that the new session should go before
                        {
                            existing.Insert(insertAt, s);
                        }
                        else
                        {
                            existing.Add(s); // The new session should go on the end of the list
                        }
                    }
                }
            }

            return allSessions;
        }

        /// <summary>
        /// Fire this event when you do something that would necessitate refreshing the table of sessions in the UI.
        /// That means a change the sessions collection (i.e. when you add or remove elements
        /// to the collection, having Session.EventName==MainViewModel.CurrentEvent), or calling  
        /// MainViewModel.SetCurrentEvent, or changing MainView.CurrentFilter.  Note that merely changing an attribute
        /// of a session doesn't necessarily require a refresh of the sessions table in the UI, because we expect
        /// the UI to bind to the relevant session attributes, so it will see such changes anyway.
        /// The MainViewModel.Loaded method includes logic to decide when to fire this event, in response to
        /// the firing of its PropertyChanged event
        /// </summary>
        public event EventHandler SessionsMustBeRefreshed;

        /// <summary>
        /// Fire the SessionsMustBeRefreshed event after a delay of 500ms.  But if, during those 500ms, there are further
        /// calls to ThrottledFireSessionsRefreshEvent, the event won't be fired then.  Instead, the app will wait until
        /// 500ms have passed without any further calls to ThrottledFireSessionsRefreshEvent, and will then fire the
        /// event.  The upshot: to avoid the possibility of firing the 'refresh' event many times in quick succession,
        /// throttle the number of firings to a manximum of 2 per second, by using this method.
        /// </summary>
        private void ThrottledFireSessionsRefreshEvent()
        {
            Model.DispatcherHelper.StartTimer(ref sessionsRefreshTimer, 500, () =>
            {
                Model.DispatcherHelper.RunAsyncOnUI(() =>
                {
                    this.SessionsMustBeRefreshed?.Invoke(this, new EventArgs());
                });
            });
        }
        System.Threading.Timer sessionsRefreshTimer = null;

        /// <summary>
        /// Fetch a Session object from the game sessions collection, given an id.  If the id is not valid the method
        /// returns null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Session GetSession(string id)
        {
            lock(_sessionsLock)
            {
                foreach (Session s in _sessions)
                    if (s.Id == id)
                        return s;
            }
            return null;
        }

        /// <summary>
        /// The game sessions collection.  It's private; add sessions using the CreateSession method, retrieve them (by id) using
        /// GetSession.  Sessions are never deleted from the game sessions collection, but if Session.Status is set to
        /// "ABANDONED" the session won't be available for sign-ups and shouldn't be shown in the sessions list UI.
        /// </summary>
        private List<Session> _sessions = new List<Session>();

        /// <summary>
        /// Lock for the game sessions collection.  Lock this while adding or removing elements in the collection, and while
        /// iterating over it.
        /// </summary>
        private readonly LobsterLock _sessionsLock = new LobsterLock();

        /// <summary>
        /// Fetch a Person object from the persons collection, given a person handle.  If the handleis not valid the method
        /// returns null.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public Person GetPerson(string handle)
        {
            lock(_personsLock)
            {
                foreach (Person p in _persons)
                    if (p.Handle == handle)
                        return p;
            }
            return null;
        }

        /// <summary>
        /// Check that a person handle corresponds to a person in the persons collection
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool CheckPersonHandleExists(string handle)
        {
            if (this.GetPerson(handle) == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Retrieve a list of persons that we can manage signups for.
        /// </summary>
        /// <param name="includeInactive">if true, then inactive persons will be included in the list returned</param>
        /// <returns></returns>
        public List<string> GetAvailablePersons(bool includeInactive=false)
        {
            List<string> personNames = new List<string>();
            lock (_personsLock)
            {
                foreach (Person p in _persons)
                {
                    if (p.IsActive || includeInactive)
                    {
                        personNames.Add(p.Handle);
                    }
                }
            }

            return personNames;
        }

        /// <summary>
        /// The persons collection.  It's private; add persons using the CreatePerson method, retrieve them (by handle) using
        /// GetPerson, and check the validity of a person handle by using CheckPersonHandleExists.  Persons are never deleted
        /// from the persons collection, but if Person.IsActive is set to false the person won't be able to sign up for game
        /// sessions.
        /// </summary>
        private List<Person> _persons = new List<Person>();

        /// <summary>
        /// Lock for the persons collection.  Lock this while adding or removing elements in the collection, and while
        /// iterating over it.
        /// </summary>
        private readonly LobsterLock _personsLock = new LobsterLock();

        /// <summary>
        /// Checks whether the games collection contains a game having the name provided.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckGameNameExists(string name)
        {
            if (this.GetGame(name) == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Retrieve a list of the games that we can manage signups for.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableGames()
        {
            List<string> gameNames = new List<string>();
            lock (_gamesLock)
            {
                foreach (Game g in _games)
                {
                    if (g.IsActive)
                    {
                        gameNames.Add(g.Name);
                    }
                }
            }

            return gameNames;
        }

        /// <summary>
        /// Returns the Game object for the game having the name provided, or null if there is no such game in the
        /// games collection.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Game GetGame(string name)
        {
            lock (_gamesLock)
            {
                foreach (Game g in _games)
                    if (g.Name == name)
                        return g;
            }
            return null;
        }

        /// <summary>
        /// Emit a message into a list that the UI will show to the user.  Expect them to be shown in a viewable list,
        /// but not to produce a popup that would disturb the user's activities.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public void LogUserMessage(Logger.Level level, string message)
        {
            string l = "";
            switch(level)
            {
                case Logger.Level.DEBUG:   l = "DEBUG:"; break;
                case Logger.Level.INFO:    l = "INFO:"; break;
                case Logger.Level.WARNING: l = "ALERT:";  break;
                case Logger.Level.ERROR:   l = "ERROR:"; break;
                default:break;
            }

            DateTime d = DateTime.Now;
            int hh = d.Hour;
            int mm = d.Minute;

            string t = string.Format("{0:D2}:{1:D2}: ", hh, mm);

            string toReport = t + " "+l+" " + message;

            if(Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                if (UserMessages == null)
                    UserMessages = new System.Collections.ObjectModel.ObservableCollection<string>();

                UserMessages.Insert(0, toReport);
            }
            else
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainViewModel.LogUserMessage", "should only be called on UI thread");
            }

            Logger.LogMessage(level, "MainViewModel.LogUserMessage", message);
        }

        /// <summary>
        /// The observable collection of strings that messages are written into by LogUserMessage.  The most recent
        /// message is at the start.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> UserMessages { get; set; }

        /// <summary>
        /// The games collection.  It's private; add games using the CreateGame method, retrieve them (by name) using
        /// GetGame, and check the validity of a game name by using CheckGameNameExists.  Games are never deleted
        /// from the games collection.
        /// </summary>
        private List<Game> _games = new List<Game>();

        /// <summary>
        /// Lock for the games collection.  Lock this while adding or removing elements in the collection, and while
        /// iterating over it.
        /// </summary>
        private readonly LobsterLock _gamesLock = new LobsterLock();

        /// <summary>
        /// Create an initial set of games, that will be held in the local games database. They'll never be amended and they
        /// will not be copied onto the cloud store, there being no need to do so as every installation of the app will have
        /// this same list inserted into its local store.
        /// The list was scraped from the BGG website on 2025-02-25, and it consists of the 500 games with the highest 
        /// 'geek ratings' at that time. 
        /// </summary>
        private void CreateDefaultGames()
        {
            lock(_gamesLock)
            {
                _games.Add(new Game() { Name = "Brass: Birmingham (2018)", BggLink = "https://boardgamegeek.com/boardgame/224517/brass-birmingham" });
                _games.Add(new Game() { Name = "Pandemic Legacy: Season 1 (2015)", BggLink = "https://boardgamegeek.com/boardgame/161936/pandemic-legacy-season-1" });
                _games.Add(new Game() { Name = "Ark Nova (2021)", BggLink = "https://boardgamegeek.com/boardgame/342942/ark-nova" });
                _games.Add(new Game() { Name = "Gloomhaven (2017)", BggLink = "https://boardgamegeek.com/boardgame/174430/gloomhaven" });
                _games.Add(new Game() { Name = "Twilight Imperium: Fourth Edition (2017)", BggLink = "https://boardgamegeek.com/boardgame/233078/twilight-imperium-fourth-edition" });
                _games.Add(new Game() { Name = "Dune: Imperium (2020)", BggLink = "https://boardgamegeek.com/boardgame/316554/dune-imperium" });
                _games.Add(new Game() { Name = "Terraforming Mars (2016)", BggLink = "https://boardgamegeek.com/boardgame/167791/terraforming-mars" });
                _games.Add(new Game() { Name = "War of the Ring: Second Edition (2011)", BggLink = "https://boardgamegeek.com/boardgame/115746/war-of-the-ring-second-edition" });
                _games.Add(new Game() { Name = "Star Wars: Rebellion (2016)", BggLink = "https://boardgamegeek.com/boardgame/187645/star-wars-rebellion" });
                _games.Add(new Game() { Name = "Spirit Island (2017)", BggLink = "https://boardgamegeek.com/boardgame/162886/spirit-island" });
                _games.Add(new Game() { Name = "Gloomhaven: Jaws of the Lion (2020)", BggLink = "https://boardgamegeek.com/boardgame/291457/gloomhaven-jaws-of-the-lion" });
                _games.Add(new Game() { Name = "Gaia Project (2017)", BggLink = "https://boardgamegeek.com/boardgame/220308/gaia-project" });
                _games.Add(new Game() { Name = "Twilight Struggle (2005)", BggLink = "https://boardgamegeek.com/boardgame/12333/twilight-struggle" });
                _games.Add(new Game() { Name = "Dune: Imperium – Uprising (2023)", BggLink = "https://boardgamegeek.com/boardgame/397598/dune-imperium-uprising" });
                _games.Add(new Game() { Name = "Through the Ages: A New Story of Civilization (2015)", BggLink = "https://boardgamegeek.com/boardgame/182028/through-the-ages-a-new-story-of-civilization" });
                _games.Add(new Game() { Name = "The Castles of Burgundy (2011)", BggLink = "https://boardgamegeek.com/boardgame/84876/the-castles-of-burgundy" });
                _games.Add(new Game() { Name = "Great Western Trail (2016)", BggLink = "https://boardgamegeek.com/boardgame/193738/great-western-trail" });
                _games.Add(new Game() { Name = "Eclipse: Second Dawn for the Galaxy (2020)", BggLink = "https://boardgamegeek.com/boardgame/246900/eclipse-second-dawn-for-the-galaxy" });
                _games.Add(new Game() { Name = "Scythe (2016)", BggLink = "https://boardgamegeek.com/boardgame/169786/scythe" });
                _games.Add(new Game() { Name = "7 Wonders Duel (2015)", BggLink = "https://boardgamegeek.com/boardgame/173346/7-wonders-duel" });
                _games.Add(new Game() { Name = "Brass: Lancashire (2007)", BggLink = "https://boardgamegeek.com/boardgame/28720/brass-lancashire" });
                _games.Add(new Game() { Name = "Nemesis (2018)", BggLink = "https://boardgamegeek.com/boardgame/167355/nemesis" });
                _games.Add(new Game() { Name = "Clank! Legacy: Acquisitions Incorporated (2019)", BggLink = "https://boardgamegeek.com/boardgame/266507/clank-legacy-acquisitions-incorporated" });
                _games.Add(new Game() { Name = "A Feast for Odin (2016)", BggLink = "https://boardgamegeek.com/boardgame/177736/a-feast-for-odin" });
                _games.Add(new Game() { Name = "Concordia (2013)", BggLink = "https://boardgamegeek.com/boardgame/124361/concordia" });
                _games.Add(new Game() { Name = "Frosthaven (2022)", BggLink = "https://boardgamegeek.com/boardgame/295770/frosthaven" });
                _games.Add(new Game() { Name = "Great Western Trail: Second Edition (2021)", BggLink = "https://boardgamegeek.com/boardgame/341169/great-western-trail-second-edition" });
                _games.Add(new Game() { Name = "Arkham Horror: The Card Game (2016)", BggLink = "https://boardgamegeek.com/boardgame/205637/arkham-horror-the-card-game" });
                _games.Add(new Game() { Name = "Lost Ruins of Arnak (2020)", BggLink = "https://boardgamegeek.com/boardgame/312484/lost-ruins-of-arnak" });
                _games.Add(new Game() { Name = "Root (2018)", BggLink = "https://boardgamegeek.com/boardgame/237182/root" });
                _games.Add(new Game() { Name = "Terra Mystica (2012)", BggLink = "https://boardgamegeek.com/boardgame/120677/terra-mystica" });
                _games.Add(new Game() { Name = "Wingspan (2019)", BggLink = "https://boardgamegeek.com/boardgame/266192/wingspan" });
                _games.Add(new Game() { Name = "Too Many Bones (2017)", BggLink = "https://boardgamegeek.com/boardgame/192135/too-many-bones" });
                _games.Add(new Game() { Name = "Orléans (2014)", BggLink = "https://boardgamegeek.com/boardgame/164928/orleans" });
                _games.Add(new Game() { Name = "Mage Knight Board Game (2011)", BggLink = "https://boardgamegeek.com/boardgame/96848/mage-knight-board-game" });
                _games.Add(new Game() { Name = "Barrage (2019)", BggLink = "https://boardgamegeek.com/boardgame/251247/barrage" });
                _games.Add(new Game() { Name = "The Crew: Mission Deep Sea (2021)", BggLink = "https://boardgamegeek.com/boardgame/324856/the-crew-mission-deep-sea" });
                _games.Add(new Game() { Name = "Everdell (2018)", BggLink = "https://boardgamegeek.com/boardgame/199792/everdell" });
                _games.Add(new Game() { Name = "Viticulture Essential Edition (2015)", BggLink = "https://boardgamegeek.com/boardgame/183394/viticulture-essential-edition" });
                _games.Add(new Game() { Name = "Sky Team (2023)", BggLink = "https://boardgamegeek.com/boardgame/373106/sky-team" });
                _games.Add(new Game() { Name = "Heat: Pedal to the Metal (2022)", BggLink = "https://boardgamegeek.com/boardgame/366013/heat-pedal-to-the-metal" });
                _games.Add(new Game() { Name = "Marvel Champions: The Card Game (2019)", BggLink = "https://boardgamegeek.com/boardgame/285774/marvel-champions-the-card-game" });
                _games.Add(new Game() { Name = "Food Chain Magnate (2015)", BggLink = "https://boardgamegeek.com/boardgame/175914/food-chain-magnate" });
                _games.Add(new Game() { Name = "Crokinole (1876)", BggLink = "https://boardgamegeek.com/boardgame/521/crokinole" });
                _games.Add(new Game() { Name = "Pax Pamir: Second Edition (2019)", BggLink = "https://boardgamegeek.com/boardgame/256960/pax-pamir-second-edition" });
                _games.Add(new Game() { Name = "Underwater Cities (2018)", BggLink = "https://boardgamegeek.com/boardgame/247763/underwater-cities" });
                _games.Add(new Game() { Name = "Kanban EV (2020)", BggLink = "https://boardgamegeek.com/boardgame/284378/kanban-ev" });
                _games.Add(new Game() { Name = "Puerto Rico (2002)", BggLink = "https://boardgamegeek.com/boardgame/3076/puerto-rico" });
                _games.Add(new Game() { Name = "Cascadia (2021)", BggLink = "https://boardgamegeek.com/boardgame/295947/cascadia" });
                _games.Add(new Game() { Name = "Hegemony: Lead Your Class to Victory (2023)", BggLink = "https://boardgamegeek.com/boardgame/321608/hegemony-lead-your-class-to-victory" });
                _games.Add(new Game() { Name = "Pandemic Legacy: Season 0 (2020)", BggLink = "https://boardgamegeek.com/boardgame/314040/pandemic-legacy-season-0" });
                _games.Add(new Game() { Name = "Caverna: The Cave Farmers (2013)", BggLink = "https://boardgamegeek.com/boardgame/102794/caverna-the-cave-farmers" });
                _games.Add(new Game() { Name = "Anachrony (2017)", BggLink = "https://boardgamegeek.com/boardgame/185343/anachrony" });
                _games.Add(new Game() { Name = "On Mars (2020)", BggLink = "https://boardgamegeek.com/boardgame/184267/on-mars" });
                _games.Add(new Game() { Name = "Cthulhu: Death May Die (2019)", BggLink = "https://boardgamegeek.com/boardgame/253344/cthulhu-death-may-die" });
                _games.Add(new Game() { Name = "Blood Rage (2015)", BggLink = "https://boardgamegeek.com/boardgame/170216/blood-rage" });
                _games.Add(new Game() { Name = "Agricola (2007)", BggLink = "https://boardgamegeek.com/boardgame/31260/agricola" });
                _games.Add(new Game() { Name = "Oathsworn: Into the Deepwood (2022)", BggLink = "https://boardgamegeek.com/boardgame/251661/oathsworn-into-the-deepwood" });
                _games.Add(new Game() { Name = "Sleeping Gods (2021)", BggLink = "https://boardgamegeek.com/boardgame/255984/sleeping-gods" });
                _games.Add(new Game() { Name = "The Lord of the Rings: Duel for Middle-earth (2024)", BggLink = "https://boardgamegeek.com/boardgame/421006/the-lord-of-the-rings-duel-for-middle-earth" });
                _games.Add(new Game() { Name = "Lisboa (2017)", BggLink = "https://boardgamegeek.com/boardgame/161533/lisboa" });
                _games.Add(new Game() { Name = "Pandemic Legacy: Season 2 (2017)", BggLink = "https://boardgamegeek.com/boardgame/221107/pandemic-legacy-season-2" });
                _games.Add(new Game() { Name = "Obsession (2018)", BggLink = "https://boardgamegeek.com/boardgame/231733/obsession" });
                _games.Add(new Game() { Name = "Grand Austria Hotel (2015)", BggLink = "https://boardgamegeek.com/boardgame/182874/grand-austria-hotel" });
                _games.Add(new Game() { Name = "Mansions of Madness: Second Edition (2016)", BggLink = "https://boardgamegeek.com/boardgame/205059/mansions-of-madness-second-edition" });
                _games.Add(new Game() { Name = "Tzolk'in: The Mayan Calendar (2012)", BggLink = "https://boardgamegeek.com/boardgame/126163/tzolkin-the-mayan-calendar" });
                _games.Add(new Game() { Name = "Power Grid (2004)", BggLink = "https://boardgamegeek.com/boardgame/2651/power-grid" });
                _games.Add(new Game() { Name = "Age of Innovation (2023)", BggLink = "https://boardgamegeek.com/boardgame/383179/age-of-innovation" });
                _games.Add(new Game() { Name = "Clans of Caledonia (2017)", BggLink = "https://boardgamegeek.com/boardgame/216132/clans-of-caledonia" });
                _games.Add(new Game() { Name = "The Quacks of Quedlinburg (2018)", BggLink = "https://boardgamegeek.com/boardgame/244521/the-quacks-of-quedlinburg" });
                _games.Add(new Game() { Name = "Maracaibo (2019)", BggLink = "https://boardgamegeek.com/boardgame/276025/maracaibo" });
                _games.Add(new Game() { Name = "Le Havre (2008)", BggLink = "https://boardgamegeek.com/boardgame/35677/le-havre" });
                _games.Add(new Game() { Name = "Paladins of the West Kingdom (2019)", BggLink = "https://boardgamegeek.com/boardgame/266810/paladins-of-the-west-kingdom" });
                _games.Add(new Game() { Name = "The Gallerist (2015)", BggLink = "https://boardgamegeek.com/boardgame/125153/the-gallerist" });
                _games.Add(new Game() { Name = "Star Wars: Imperial Assault (2014)", BggLink = "https://boardgamegeek.com/boardgame/164153/star-wars-imperial-assault" });
                _games.Add(new Game() { Name = "Android: Netrunner (2012)", BggLink = "https://boardgamegeek.com/boardgame/124742/android-netrunner" });
                _games.Add(new Game() { Name = "Slay the Spire: The Board Game (2024)", BggLink = "https://boardgamegeek.com/boardgame/338960/slay-the-spire-the-board-game" });
                _games.Add(new Game() { Name = "Mechs vs. Minions (2016)", BggLink = "https://boardgamegeek.com/boardgame/209010/mechs-vs-minions" });
                _games.Add(new Game() { Name = "The Crew: The Quest for Planet Nine (2019)", BggLink = "https://boardgamegeek.com/boardgame/284083/the-crew-the-quest-for-planet-nine" });
                _games.Add(new Game() { Name = "Clank!: Catacombs (2022)", BggLink = "https://boardgamegeek.com/boardgame/365717/clank-catacombs" });
                _games.Add(new Game() { Name = "Agricola (Revised Edition) (2016)", BggLink = "https://boardgamegeek.com/boardgame/200680/agricola-revised-edition" });
                _games.Add(new Game() { Name = "Kingdom Death: Monster (2015)", BggLink = "https://boardgamegeek.com/boardgame/55690/kingdom-death-monster" });
                _games.Add(new Game() { Name = "Race for the Galaxy (2007)", BggLink = "https://boardgamegeek.com/boardgame/28143/race-for-the-galaxy" });
                _games.Add(new Game() { Name = "Azul (2017)", BggLink = "https://boardgamegeek.com/boardgame/230802/azul" });
                _games.Add(new Game() { Name = "Five Tribes: The Djinns of Naqala (2014)", BggLink = "https://boardgamegeek.com/boardgame/157354/five-tribes-the-djinns-of-naqala" });
                _games.Add(new Game() { Name = "Clank!: A Deck-Building Adventure (2016)", BggLink = "https://boardgamegeek.com/boardgame/201808/clank-a-deck-building-adventure" });
                _games.Add(new Game() { Name = "Eclipse: New Dawn for the Galaxy (2011)", BggLink = "https://boardgamegeek.com/boardgame/72125/eclipse-new-dawn-for-the-galaxy" });
                _games.Add(new Game() { Name = "Fields of Arle (2014)", BggLink = "https://boardgamegeek.com/boardgame/159675/fields-of-arle" });
                _games.Add(new Game() { Name = "Aeon's End (2016)", BggLink = "https://boardgamegeek.com/boardgame/191189/aeons-end" });
                _games.Add(new Game() { Name = "Lords of Waterdeep (2012)", BggLink = "https://boardgamegeek.com/boardgame/110327/lords-of-waterdeep" });
                _games.Add(new Game() { Name = "El Grande (1995)", BggLink = "https://boardgamegeek.com/boardgame/93/el-grande" });
                _games.Add(new Game() { Name = "Teotihuacan: City of Gods (2018)", BggLink = "https://boardgamegeek.com/boardgame/229853/teotihuacan-city-of-gods" });
                _games.Add(new Game() { Name = "Revive (2022)", BggLink = "https://boardgamegeek.com/boardgame/332772/revive" });
                _games.Add(new Game() { Name = "Darwin's Journey (2023)", BggLink = "https://boardgamegeek.com/boardgame/322289/darwins-journey" });
                _games.Add(new Game() { Name = "Beyond the Sun (2020)", BggLink = "https://boardgamegeek.com/boardgame/317985/beyond-the-sun" });
                _games.Add(new Game() { Name = "Through the Ages: A Story of Civilization (2006)", BggLink = "https://boardgamegeek.com/boardgame/25613/through-the-ages-a-story-of-civilization" });
                _games.Add(new Game() { Name = "The White Castle (2023)", BggLink = "https://boardgamegeek.com/boardgame/371942/the-white-castle" });
                _games.Add(new Game() { Name = "SCOUT (2019)", BggLink = "https://boardgamegeek.com/boardgame/291453/scout" });
                _games.Add(new Game() { Name = "Wingspan Asia (2022)", BggLink = "https://boardgamegeek.com/boardgame/366161/wingspan-asia" });
                _games.Add(new Game() { Name = "Concordia Venus (2018)", BggLink = "https://boardgamegeek.com/boardgame/256916/concordia-venus" });
                _games.Add(new Game() { Name = "Title", BggLink = "https://boardgamegeek.com/browse/boardgame?sort=title" });
                _games.Add(new Game() { Name = "Dominant Species (2010)", BggLink = "https://boardgamegeek.com/boardgame/62219/dominant-species" });
                _games.Add(new Game() { Name = "Harmonies (2024)", BggLink = "https://boardgamegeek.com/boardgame/414317/harmonies" });
                _games.Add(new Game() { Name = "7 Wonders (2010)", BggLink = "https://boardgamegeek.com/boardgame/68448/7-wonders" });
                _games.Add(new Game() { Name = "Robinson Crusoe: Adventures on the Cursed Island (2012)", BggLink = "https://boardgamegeek.com/boardgame/121921/robinson-crusoe-adventures-on-the-cursed-island" });
                _games.Add(new Game() { Name = "The Search for Planet X (2020)", BggLink = "https://boardgamegeek.com/boardgame/279537/the-search-for-planet-x" });
                _games.Add(new Game() { Name = "Ticket to Ride Legacy: Legends of the West (2023)", BggLink = "https://boardgamegeek.com/boardgame/390092/ticket-to-ride-legacy-legends-of-the-west" });
                _games.Add(new Game() { Name = "The Voyages of Marco Polo (2015)", BggLink = "https://boardgamegeek.com/boardgame/171623/the-voyages-of-marco-polo" });
                _games.Add(new Game() { Name = "Decrypto (2018)", BggLink = "https://boardgamegeek.com/boardgame/225694/decrypto" });
                _games.Add(new Game() { Name = "Great Western Trail: New Zealand (2023)", BggLink = "https://boardgamegeek.com/boardgame/380607/great-western-trail-new-zealand" });
                _games.Add(new Game() { Name = "Final Girl (2021)", BggLink = "https://boardgamegeek.com/boardgame/277659/final-girl" });
                _games.Add(new Game() { Name = "Inis (2016)", BggLink = "https://boardgamegeek.com/boardgame/155821/inis" });
                _games.Add(new Game() { Name = "Battlestar Galactica: The Board Game (2008)", BggLink = "https://boardgamegeek.com/boardgame/37111/battlestar-galactica-the-board-game" });
                _games.Add(new Game() { Name = "Splendor Duel (2022)", BggLink = "https://boardgamegeek.com/boardgame/364073/splendor-duel" });
                _games.Add(new Game() { Name = "Trickerion: Legends of Illusion (2015)", BggLink = "https://boardgamegeek.com/boardgame/163068/trickerion-legends-of-illusion" });
                _games.Add(new Game() { Name = "Tainted Grail: The Fall of Avalon (2019)", BggLink = "https://boardgamegeek.com/boardgame/264220/tainted-grail-the-fall-of-avalon" });
                _games.Add(new Game() { Name = "Architects of the West Kingdom (2018)", BggLink = "https://boardgamegeek.com/boardgame/236457/architects-of-the-west-kingdom" });
                _games.Add(new Game() { Name = "Carnegie (2022)", BggLink = "https://boardgamegeek.com/boardgame/310873/carnegie" });
                _games.Add(new Game() { Name = "Blood on the Clocktower (2022)", BggLink = "https://boardgamegeek.com/boardgame/240980/blood-on-the-clocktower" });
                _games.Add(new Game() { Name = "Dwellings of Eldervale (2020)", BggLink = "https://boardgamegeek.com/boardgame/271055/dwellings-of-eldervale" });
                _games.Add(new Game() { Name = "Keyflower (2012)", BggLink = "https://boardgamegeek.com/boardgame/122515/keyflower" });
                _games.Add(new Game() { Name = "The Quest for El Dorado (2017)", BggLink = "https://boardgamegeek.com/boardgame/217372/the-quest-for-el-dorado" });
                _games.Add(new Game() { Name = "Raiders of the North Sea (2015)", BggLink = "https://boardgamegeek.com/boardgame/170042/raiders-of-the-north-sea" });
                _games.Add(new Game() { Name = "Tigris & Euphrates (1997)", BggLink = "https://boardgamegeek.com/boardgame/42/tigris-and-euphrates" });
                _games.Add(new Game() { Name = "Dominion: Intrigue (2009)", BggLink = "https://boardgamegeek.com/boardgame/40834/dominion-intrigue" });
                _games.Add(new Game() { Name = "Caylus (2005)", BggLink = "https://boardgamegeek.com/boardgame/18602/caylus" });
                _games.Add(new Game() { Name = "Troyes (2010)", BggLink = "https://boardgamegeek.com/boardgame/73439/troyes" });
                _games.Add(new Game() { Name = "Lorenzo il Magnifico (2016)", BggLink = "https://boardgamegeek.com/boardgame/203993/lorenzo-il-magnifico" });
                _games.Add(new Game() { Name = "Eldritch Horror (2013)", BggLink = "https://boardgamegeek.com/boardgame/146021/eldritch-horror" });
                _games.Add(new Game() { Name = "Voidfall (2023)", BggLink = "https://boardgamegeek.com/boardgame/337627/voidfall" });
                _games.Add(new Game() { Name = "Mombasa (2015)", BggLink = "https://boardgamegeek.com/boardgame/172386/mombasa" });
                _games.Add(new Game() { Name = "Ra (1999)", BggLink = "https://boardgamegeek.com/boardgame/12/ra" });
                _games.Add(new Game() { Name = "The Lord of the Rings: Journeys in Middle-Earth (2019)", BggLink = "https://boardgamegeek.com/boardgame/269385/the-lord-of-the-rings-journeys-in-middle-earth" });
                _games.Add(new Game() { Name = "Arcs (2024)", BggLink = "https://boardgamegeek.com/boardgame/359871/arcs" });
                _games.Add(new Game() { Name = "Nemesis: Lockdown (2022)", BggLink = "https://boardgamegeek.com/boardgame/310100/nemesis-lockdown" });
                _games.Add(new Game() { Name = "Twilight Imperium: Third Edition (2005)", BggLink = "https://boardgamegeek.com/boardgame/12493/twilight-imperium-third-edition" });
                _games.Add(new Game() { Name = "Patchwork (2014)", BggLink = "https://boardgamegeek.com/boardgame/163412/patchwork" });
                _games.Add(new Game() { Name = "Trajan (2011)", BggLink = "https://boardgamegeek.com/boardgame/102680/trajan" });
                _games.Add(new Game() { Name = "Dominion (2008)", BggLink = "https://boardgamegeek.com/boardgame/36218/dominion" });
                _games.Add(new Game() { Name = "Age of Steam (2002)", BggLink = "https://boardgamegeek.com/boardgame/4098/age-of-steam" });
                _games.Add(new Game() { Name = "Russian Railroads (2013)", BggLink = "https://boardgamegeek.com/boardgame/144733/russian-railroads" });
                _games.Add(new Game() { Name = "Aeon's End: War Eternal (2017)", BggLink = "https://boardgamegeek.com/boardgame/218417/aeons-end-war-eternal" });
                _games.Add(new Game() { Name = "The 7th Continent (2017)", BggLink = "https://boardgamegeek.com/boardgame/180263/the-7th-continent" });
                _games.Add(new Game() { Name = "Hansa Teutonica (2009)", BggLink = "https://boardgamegeek.com/boardgame/43015/hansa-teutonica" });
                _games.Add(new Game() { Name = "Rising Sun (2018)", BggLink = "https://boardgamegeek.com/boardgame/205896/rising-sun" });
                _games.Add(new Game() { Name = "Wonderland's War (2022)", BggLink = "https://boardgamegeek.com/boardgame/227935/wonderlands-war" });
                _games.Add(new Game() { Name = "Vinhos: Deluxe Edition (2016)", BggLink = "https://boardgamegeek.com/boardgame/175640/vinhos-deluxe-edition" });
                _games.Add(new Game() { Name = "Yokohama (2016)", BggLink = "https://boardgamegeek.com/boardgame/196340/yokohama" });
                _games.Add(new Game() { Name = "Clank! In! Space!: A Deck-Building Adventure (2017)", BggLink = "https://boardgamegeek.com/boardgame/233371/clank-in-space-a-deck-building-adventure" });
                _games.Add(new Game() { Name = "Iberia (2016)", BggLink = "https://boardgamegeek.com/boardgame/198928/iberia" });
                _games.Add(new Game() { Name = "Forbidden Stars (2015)", BggLink = "https://boardgamegeek.com/boardgame/175155/forbidden-stars" });
                _games.Add(new Game() { Name = "Codenames (2015)", BggLink = "https://boardgamegeek.com/boardgame/178900/codenames" });
                _games.Add(new Game() { Name = "Just One (2018)", BggLink = "https://boardgamegeek.com/boardgame/254640/just-one" });
                _games.Add(new Game() { Name = "Wyrmspan (2024)", BggLink = "https://boardgamegeek.com/boardgame/410201/wyrmspan" });
                _games.Add(new Game() { Name = "SETI: Search for Extraterrestrial Intelligence (2024)", BggLink = "https://boardgamegeek.com/boardgame/418059/seti-search-for-extraterrestrial-intelligence" });
                _games.Add(new Game() { Name = "Champions of Midgard (2015)", BggLink = "https://boardgamegeek.com/boardgame/172287/champions-of-midgard" });
                _games.Add(new Game() { Name = "PARKS (2019)", BggLink = "https://boardgamegeek.com/boardgame/266524/parks" });
                _games.Add(new Game() { Name = "Roll for the Galaxy (2014)", BggLink = "https://boardgamegeek.com/boardgame/132531/roll-for-the-galaxy" });
                _games.Add(new Game() { Name = "Rajas of the Ganges (2017)", BggLink = "https://boardgamegeek.com/boardgame/220877/rajas-of-the-ganges" });
                _games.Add(new Game() { Name = "Pandemic (2008)", BggLink = "https://boardgamegeek.com/boardgame/30549/pandemic" });
                _games.Add(new Game() { Name = "Targi (2012)", BggLink = "https://boardgamegeek.com/boardgame/118048/targi" });
                _games.Add(new Game() { Name = "Hadrian's Wall (2021)", BggLink = "https://boardgamegeek.com/boardgame/304783/hadrians-wall" });
                _games.Add(new Game() { Name = "Cartographers (2019)", BggLink = "https://boardgamegeek.com/boardgame/263918/cartographers" });
                _games.Add(new Game() { Name = "Res Arcana (2019)", BggLink = "https://boardgamegeek.com/boardgame/262712/res-arcana" });
                _games.Add(new Game() { Name = "ISS Vanguard (2022)", BggLink = "https://boardgamegeek.com/boardgame/325494/iss-vanguard" });
                _games.Add(new Game() { Name = "The Isle of Cats (2019)", BggLink = "https://boardgamegeek.com/boardgame/281259/the-isle-of-cats" });
                _games.Add(new Game() { Name = "Tyrants of the Underdark (2016)", BggLink = "https://boardgamegeek.com/boardgame/189932/tyrants-of-the-underdark" });
                _games.Add(new Game() { Name = "Star Realms (2014)", BggLink = "https://boardgamegeek.com/boardgame/147020/star-realms" });
                _games.Add(new Game() { Name = "Magic: The Gathering (1993)", BggLink = "https://boardgamegeek.com/boardgame/463/magic-the-gathering" });
                _games.Add(new Game() { Name = "Watergate (2019)", BggLink = "https://boardgamegeek.com/boardgame/274364/watergate" });
                _games.Add(new Game() { Name = "Alchemists (2014)", BggLink = "https://boardgamegeek.com/boardgame/161970/alchemists" });
                _games.Add(new Game() { Name = "It's a Wonderful World (2019)", BggLink = "https://boardgamegeek.com/boardgame/271324/its-a-wonderful-world" });
                _games.Add(new Game() { Name = "Ticket to Ride: Europe (2005)", BggLink = "https://boardgamegeek.com/boardgame/14996/ticket-to-ride-europe" });
                _games.Add(new Game() { Name = "Nucleum (2023)", BggLink = "https://boardgamegeek.com/boardgame/396790/nucleum" });
                _games.Add(new Game() { Name = "Undaunted: Normandy (2019)", BggLink = "https://boardgamegeek.com/boardgame/268864/undaunted-normandy" });
                _games.Add(new Game() { Name = "Too Many Bones: Undertow (2018)", BggLink = "https://boardgamegeek.com/boardgame/235802/too-many-bones-undertow" });
                _games.Add(new Game() { Name = "The Lord of the Rings: The Card Game (2011)", BggLink = "https://boardgamegeek.com/boardgame/77423/the-lord-of-the-rings-the-card-game" });
                _games.Add(new Game() { Name = "Stone Age (2008)", BggLink = "https://boardgamegeek.com/boardgame/34635/stone-age" });
                _games.Add(new Game() { Name = "Praga Caput Regni (2020)", BggLink = "https://boardgamegeek.com/boardgame/308765/praga-caput-regni" });
                _games.Add(new Game() { Name = "Kemet (2012)", BggLink = "https://boardgamegeek.com/boardgame/127023/kemet" });
                _games.Add(new Game() { Name = "Sherlock Holmes Consulting Detective: The Thames Murders & Other Cases (1982)", BggLink = "https://boardgamegeek.com/boardgame/2511/sherlock-holmes-consulting-detective-the-thames-mu" });
                _games.Add(new Game() { Name = "Terraforming Mars: Ares Expedition (2021)", BggLink = "https://boardgamegeek.com/boardgame/328871/terraforming-mars-ares-expedition" });
                _games.Add(new Game() { Name = "Endeavor: Age of Sail (2018)", BggLink = "https://boardgamegeek.com/boardgame/233398/endeavor-age-of-sail" });
                _games.Add(new Game() { Name = "Dominion (Second Edition) (2016)", BggLink = "https://boardgamegeek.com/boardgame/209418/dominion-second-edition" });
                _games.Add(new Game() { Name = "War Chest (2018)", BggLink = "https://boardgamegeek.com/boardgame/249259/war-chest" });
                _games.Add(new Game() { Name = "Istanbul (2014)", BggLink = "https://boardgamegeek.com/boardgame/148949/istanbul" });
                _games.Add(new Game() { Name = "Viscounts of the West Kingdom (2020)", BggLink = "https://boardgamegeek.com/boardgame/296151/viscounts-of-the-west-kingdom" });
                _games.Add(new Game() { Name = "Legendary Encounters: An Alien Deck Building Game (2014)", BggLink = "https://boardgamegeek.com/boardgame/146652/legendary-encounters-an-alien-deck-building-game" });
                _games.Add(new Game() { Name = "Planet Unknown (2022)", BggLink = "https://boardgamegeek.com/boardgame/258779/planet-unknown" });
                _games.Add(new Game() { Name = "Unmatched: Cobble & Fog (2020)", BggLink = "https://boardgamegeek.com/boardgame/294484/unmatched-cobble-and-fog" });
                _games.Add(new Game() { Name = "Star Wars: X-Wing Miniatures Game (2012)", BggLink = "https://boardgamegeek.com/boardgame/103885/star-wars-x-wing-miniatures-game" });
                _games.Add(new Game() { Name = "Glen More II: Chronicles (2019)", BggLink = "https://boardgamegeek.com/boardgame/265188/glen-more-ii-chronicles" });
                _games.Add(new Game() { Name = "Marco Polo II: In the Service of the Khan (2019)", BggLink = "https://boardgamegeek.com/boardgame/283948/marco-polo-ii-in-the-service-of-the-khan" });
                _games.Add(new Game() { Name = "Radlands (2021)", BggLink = "https://boardgamegeek.com/boardgame/329082/radlands" });
                _games.Add(new Game() { Name = "That's Pretty Clever! (2018)", BggLink = "https://boardgamegeek.com/boardgame/244522/thats-pretty-clever" });
                _games.Add(new Game() { Name = "Xia: Legends of a Drift System (2014)", BggLink = "https://boardgamegeek.com/boardgame/82222/xia-legends-of-a-drift-system" });
                _games.Add(new Game() { Name = "Jaipur (2009)", BggLink = "https://boardgamegeek.com/boardgame/54043/jaipur" });
                _games.Add(new Game() { Name = "Welcome To... (2018)", BggLink = "https://boardgamegeek.com/boardgame/233867/welcome-to" });
                _games.Add(new Game() { Name = "Earth (2023)", BggLink = "https://boardgamegeek.com/boardgame/350184/earth" });
                _games.Add(new Game() { Name = "Welcome to the Moon (2021)", BggLink = "https://boardgamegeek.com/boardgame/339789/welcome-to-the-moon" });
                _games.Add(new Game() { Name = "Title", BggLink = "https://boardgamegeek.com/browse/boardgame?sort=title" });
                _games.Add(new Game() { Name = "Chaos in the Old World (2009)", BggLink = "https://boardgamegeek.com/boardgame/43111/chaos-in-the-old-world" });
                _games.Add(new Game() { Name = "Star Wars: Outer Rim (2019)", BggLink = "https://boardgamegeek.com/boardgame/271896/star-wars-outer-rim" });
                _games.Add(new Game() { Name = "War of the Ring (2004)", BggLink = "https://boardgamegeek.com/boardgame/9609/war-of-the-ring" });
                _games.Add(new Game() { Name = "This War of Mine: The Board Game (2017)", BggLink = "https://boardgamegeek.com/boardgame/188920/this-war-of-mine-the-board-game" });
                _games.Add(new Game() { Name = "Arkham Horror: The Card Game (Revised Edition) (2021)", BggLink = "https://boardgamegeek.com/boardgame/359609/arkham-horror-the-card-game-revised-edition" });
                _games.Add(new Game() { Name = "Memoir '44 (2004)", BggLink = "https://boardgamegeek.com/boardgame/10630/memoir-44" });
                _games.Add(new Game() { Name = "Great Western Trail: Argentina (2022)", BggLink = "https://boardgamegeek.com/boardgame/364011/great-western-trail-argentina" });
                _games.Add(new Game() { Name = "The Red Cathedral (2020)", BggLink = "https://boardgamegeek.com/boardgame/227224/the-red-cathedral" });
                _games.Add(new Game() { Name = "Descent: Journeys in the Dark (Second Edition) (2012)", BggLink = "https://boardgamegeek.com/boardgame/104162/descent-journeys-in-the-dark-second-edition" });
                _games.Add(new Game() { Name = "Meadow (2021)", BggLink = "https://boardgamegeek.com/boardgame/314491/meadow" });
                _games.Add(new Game() { Name = "Sekigahara: The Unification of Japan (2011)", BggLink = "https://boardgamegeek.com/boardgame/25021/sekigahara-the-unification-of-japan" });
                _games.Add(new Game() { Name = "Sagrada (2017)", BggLink = "https://boardgamegeek.com/boardgame/199561/sagrada" });
                _games.Add(new Game() { Name = "Cosmic Encounter (2008)", BggLink = "https://boardgamegeek.com/boardgame/39463/cosmic-encounter" });
                _games.Add(new Game() { Name = "Castles of Mad King Ludwig (2014)", BggLink = "https://boardgamegeek.com/boardgame/155426/castles-of-mad-king-ludwig" });
                _games.Add(new Game() { Name = "Horrified (2019)", BggLink = "https://boardgamegeek.com/boardgame/282524/horrified" });
                _games.Add(new Game() { Name = "Space Base (2018)", BggLink = "https://boardgamegeek.com/boardgame/242302/space-base" });
                _games.Add(new Game() { Name = "7 Wonders (Second Edition) (2020)", BggLink = "https://boardgamegeek.com/boardgame/316377/7-wonders-second-edition" });
                _games.Add(new Game() { Name = "Modern Art (1992)", BggLink = "https://boardgamegeek.com/boardgame/118/modern-art" });
                _games.Add(new Game() { Name = "The Resistance: Avalon (2012)", BggLink = "https://boardgamegeek.com/boardgame/128882/the-resistance-avalon" });
                _games.Add(new Game() { Name = "Railways of the World (2005)", BggLink = "https://boardgamegeek.com/boardgame/17133/railways-of-the-world" });
                _games.Add(new Game() { Name = "Paleo (2020)", BggLink = "https://boardgamegeek.com/boardgame/300531/paleo" });
                _games.Add(new Game() { Name = "Azul: Summer Pavilion (2019)", BggLink = "https://boardgamegeek.com/boardgame/287954/azul-summer-pavilion" });
                _games.Add(new Game() { Name = "Ticket to Ride: Nordic Countries (2007)", BggLink = "https://boardgamegeek.com/boardgame/31627/ticket-to-ride-nordic-countries" });
                _games.Add(new Game() { Name = "A Game of Thrones: The Board Game (Second Edition) (2011)", BggLink = "https://boardgamegeek.com/boardgame/103343/a-game-of-thrones-the-board-game-second-edition" });
                _games.Add(new Game() { Name = "Ora et Labora (2011)", BggLink = "https://boardgamegeek.com/boardgame/70149/ora-et-labora" });
                _games.Add(new Game() { Name = "Go (-2200)", BggLink = "https://boardgamegeek.com/boardgame/188/go" });
                _games.Add(new Game() { Name = "Forest Shuffle (2023)", BggLink = "https://boardgamegeek.com/boardgame/391163/forest-shuffle" });
                _games.Add(new Game() { Name = "Star Wars: The Deckbuilding Game (2023)", BggLink = "https://boardgamegeek.com/boardgame/374173/star-wars-the-deckbuilding-game" });
                _games.Add(new Game() { Name = "Commands & Colors: Ancients (2006)", BggLink = "https://boardgamegeek.com/boardgame/14105/commands-and-colors-ancients" });
                _games.Add(new Game() { Name = "Captain Sonar (2016)", BggLink = "https://boardgamegeek.com/boardgame/171131/captain-sonar" });
                _games.Add(new Game() { Name = "Suburbia (2012)", BggLink = "https://boardgamegeek.com/boardgame/123260/suburbia" });
                _games.Add(new Game() { Name = "Carcassonne (2000)", BggLink = "https://boardgamegeek.com/boardgame/822/carcassonne" });
                _games.Add(new Game() { Name = "Village (2011)", BggLink = "https://boardgamegeek.com/boardgame/104006/village" });
                _games.Add(new Game() { Name = "Splendor (2014)", BggLink = "https://boardgamegeek.com/boardgame/148228/splendor" });
                _games.Add(new Game() { Name = "Clash of Cultures: Monumental Edition (2021)", BggLink = "https://boardgamegeek.com/boardgame/299659/clash-of-cultures-monumental-edition" });
                _games.Add(new Game() { Name = "Dead of Winter: A Crossroads Game (2014)", BggLink = "https://boardgamegeek.com/boardgame/150376/dead-of-winter-a-crossroads-game" });
                _games.Add(new Game() { Name = "Return to Dark Tower (2022)", BggLink = "https://boardgamegeek.com/boardgame/256680/return-to-dark-tower" });
                _games.Add(new Game() { Name = "Ankh: Gods of Egypt (2021)", BggLink = "https://boardgamegeek.com/boardgame/285967/ankh-gods-of-egypt" });
                _games.Add(new Game() { Name = "Aeon's End: Legacy (2019)", BggLink = "https://boardgamegeek.com/boardgame/241451/aeons-end-legacy" });
                _games.Add(new Game() { Name = "Tichu (1991)", BggLink = "https://boardgamegeek.com/boardgame/215/tichu" });
                _games.Add(new Game() { Name = "Nidavellir (2020)", BggLink = "https://boardgamegeek.com/boardgame/293014/nidavellir" });
                _games.Add(new Game() { Name = "Star Realms: Colony Wars (2015)", BggLink = "https://boardgamegeek.com/boardgame/182631/star-realms-colony-wars" });
                _games.Add(new Game() { Name = "The Taverns of Tiefenthal (2019)", BggLink = "https://boardgamegeek.com/boardgame/269207/the-taverns-of-tiefenthal" });
                _games.Add(new Game() { Name = "John Company: Second Edition (2022)", BggLink = "https://boardgamegeek.com/boardgame/332686/john-company-second-edition" });
                _games.Add(new Game() { Name = "YINSH (2003)", BggLink = "https://boardgamegeek.com/boardgame/7854/yinsh" });
                _games.Add(new Game() { Name = "Under Falling Skies (2020)", BggLink = "https://boardgamegeek.com/boardgame/306735/under-falling-skies" });
                _games.Add(new Game() { Name = "Coimbra (2018)", BggLink = "https://boardgamegeek.com/boardgame/245638/coimbra" });
                _games.Add(new Game() { Name = "Paths of Glory (1999)", BggLink = "https://boardgamegeek.com/boardgame/91/paths-of-glory" });
                _games.Add(new Game() { Name = "Ticket to Ride (2004)", BggLink = "https://boardgamegeek.com/boardgame/9209/ticket-to-ride" });
                _games.Add(new Game() { Name = "Nations (2013)", BggLink = "https://boardgamegeek.com/boardgame/126042/nations" });
                _games.Add(new Game() { Name = "Near and Far (2017)", BggLink = "https://boardgamegeek.com/boardgame/195421/near-and-far" });
                _games.Add(new Game() { Name = "Calico (2020)", BggLink = "https://boardgamegeek.com/boardgame/283155/calico" });
                _games.Add(new Game() { Name = "My City (2020)", BggLink = "https://boardgamegeek.com/boardgame/295486/my-city" });
                _games.Add(new Game() { Name = "Cyclades (2009)", BggLink = "https://boardgamegeek.com/boardgame/54998/cyclades" });
                _games.Add(new Game() { Name = "KLASK (2014)", BggLink = "https://boardgamegeek.com/boardgame/165722/klask" });
                _games.Add(new Game() { Name = "Combat Commander: Europe (2006)", BggLink = "https://boardgamegeek.com/boardgame/21050/combat-commander-europe" });
                _games.Add(new Game() { Name = "Secret Hitler (2016)", BggLink = "https://boardgamegeek.com/boardgame/188834/secret-hitler" });
                _games.Add(new Game() { Name = "Dinosaur Island (2017)", BggLink = "https://boardgamegeek.com/boardgame/221194/dinosaur-island" });
                _games.Add(new Game() { Name = "Camel Up (Second Edition) (2018)", BggLink = "https://boardgamegeek.com/boardgame/260605/camel-up-second-edition" });
                _games.Add(new Game() { Name = "La Granja (2014)", BggLink = "https://boardgamegeek.com/boardgame/146886/la-granja" });
                _games.Add(new Game() { Name = "Codenames: Duet (2017)", BggLink = "https://boardgamegeek.com/boardgame/224037/codenames-duet" });
                _games.Add(new Game() { Name = "Fantasy Realms (2017)", BggLink = "https://boardgamegeek.com/boardgame/223040/fantasy-realms" });
                _games.Add(new Game() { Name = "Legendary: A Marvel Deck Building Game (2012)", BggLink = "https://boardgamegeek.com/boardgame/129437/legendary-a-marvel-deck-building-game" });
                _games.Add(new Game() { Name = "Century: Golem Edition (2017)", BggLink = "https://boardgamegeek.com/boardgame/232832/century-golem-edition" });
                _games.Add(new Game() { Name = "Hanamikoji (2013)", BggLink = "https://boardgamegeek.com/boardgame/158600/hanamikoji" });
                _games.Add(new Game() { Name = "Unmatched: Battle of Legends, Volume One (2019)", BggLink = "https://boardgamegeek.com/boardgame/274637/unmatched-battle-of-legends-volume-one" });
                _games.Add(new Game() { Name = "Deception: Murder in Hong Kong (2014)", BggLink = "https://boardgamegeek.com/boardgame/156129/deception-murder-in-hong-kong" });
                _games.Add(new Game() { Name = "MicroMacro: Crime City (2020)", BggLink = "https://boardgamegeek.com/boardgame/318977/micromacro-crime-city" });
                _games.Add(new Game() { Name = "Sushi Go Party! (2016)", BggLink = "https://boardgamegeek.com/boardgame/192291/sushi-go-party" });
                _games.Add(new Game() { Name = "Zombicide: Black Plague (2015)", BggLink = "https://boardgamegeek.com/boardgame/176189/zombicide-black-plague" });
                _games.Add(new Game() { Name = "Flamme Rouge (2016)", BggLink = "https://boardgamegeek.com/boardgame/199478/flamme-rouge" });
                _games.Add(new Game() { Name = "Isle of Skye: From Chieftain to King (2015)", BggLink = "https://boardgamegeek.com/boardgame/176494/isle-of-skye-from-chieftain-to-king" });
                _games.Add(new Game() { Name = "The Princes of Florence (2000)", BggLink = "https://boardgamegeek.com/boardgame/555/the-princes-of-florence" });
                _games.Add(new Game() { Name = "Samurai (1998)", BggLink = "https://boardgamegeek.com/boardgame/3/samurai" });
                _games.Add(new Game() { Name = "Roll Player (2016)", BggLink = "https://boardgamegeek.com/boardgame/169426/roll-player" });
                _games.Add(new Game() { Name = "Glory to Rome (2005)", BggLink = "https://boardgamegeek.com/boardgame/19857/glory-to-rome" });
                _games.Add(new Game() { Name = "Vindication (2018)", BggLink = "https://boardgamegeek.com/boardgame/224783/vindication" });
                _games.Add(new Game() { Name = "Hallertau (2020)", BggLink = "https://boardgamegeek.com/boardgame/300322/hallertau" });
                _games.Add(new Game() { Name = "Western Legends (2018)", BggLink = "https://boardgamegeek.com/boardgame/232405/western-legends" });
                _games.Add(new Game() { Name = "Pulsar 2849 (2017)", BggLink = "https://boardgamegeek.com/boardgame/228341/pulsar-2849" });
                _games.Add(new Game() { Name = "Kanban: Driver's Edition (2014)", BggLink = "https://boardgamegeek.com/boardgame/109276/kanban-drivers-edition" });
                _games.Add(new Game() { Name = "Hero Realms (2016)", BggLink = "https://boardgamegeek.com/boardgame/198994/hero-realms" });
                _games.Add(new Game() { Name = "Santorini (2016)", BggLink = "https://boardgamegeek.com/boardgame/194655/santorini" });
                _games.Add(new Game() { Name = "Goa: A New Expedition (2004)", BggLink = "https://boardgamegeek.com/boardgame/9216/goa-a-new-expedition" });
                _games.Add(new Game() { Name = "Tapestry (2019)", BggLink = "https://boardgamegeek.com/boardgame/286096/tapestry" });
                _games.Add(new Game() { Name = "1830: Railways & Robber Barons (1986)", BggLink = "https://boardgamegeek.com/boardgame/421/1830-railways-and-robber-barons" });
                _games.Add(new Game() { Name = "Shogun (2006)", BggLink = "https://boardgamegeek.com/boardgame/20551/shogun" });
                _games.Add(new Game() { Name = "Anno 1800: The Board Game (2020)", BggLink = "https://boardgamegeek.com/boardgame/311193/anno-1800-the-board-game" });
                _games.Add(new Game() { Name = "Galaxy Trucker (2007)", BggLink = "https://boardgamegeek.com/boardgame/31481/galaxy-trucker" });
                _games.Add(new Game() { Name = "Monikers (2015)", BggLink = "https://boardgamegeek.com/boardgame/156546/monikers" });
                _games.Add(new Game() { Name = "Viticulture (2013)", BggLink = "https://boardgamegeek.com/boardgame/128621/viticulture" });
                _games.Add(new Game() { Name = "Endless Winter: Paleoamericans (2022)", BggLink = "https://boardgamegeek.com/boardgame/305096/endless-winter-paleoamericans" });
                _games.Add(new Game() { Name = "1960: The Making of the President (2007)", BggLink = "https://boardgamegeek.com/boardgame/27708/1960-the-making-of-the-president" });
                _games.Add(new Game() { Name = "Detective: A Modern Crime Board Game (2018)", BggLink = "https://boardgamegeek.com/boardgame/223321/detective-a-modern-crime-board-game" });
                _games.Add(new Game() { Name = "Lewis & Clark: The Expedition (2013)", BggLink = "https://boardgamegeek.com/boardgame/140620/lewis-and-clark-the-expedition" });
                _games.Add(new Game() { Name = "So Clover! (2021)", BggLink = "https://boardgamegeek.com/boardgame/329839/so-clover" });
                _games.Add(new Game() { Name = "Mind MGMT: The Psychic Espionage “Game.” (2021)", BggLink = "https://boardgamegeek.com/boardgame/284653/mind-mgmt-the-psychic-espionage-game" });
                _games.Add(new Game() { Name = "Chronicles of Crime (2018)", BggLink = "https://boardgamegeek.com/boardgame/239188/chronicles-of-crime" });
                _games.Add(new Game() { Name = "Bitoku (2021)", BggLink = "https://boardgamegeek.com/boardgame/323612/bitoku" });
                _games.Add(new Game() { Name = "Oath (2021)", BggLink = "https://boardgamegeek.com/boardgame/291572/oath" });
                _games.Add(new Game() { Name = "Title", BggLink = "https://boardgamegeek.com/browse/boardgame?sort=title" });
                _games.Add(new Game() { Name = "Bora Bora (2013)", BggLink = "https://boardgamegeek.com/boardgame/127060/bora-bora" });
                _games.Add(new Game() { Name = "Kemet: Blood and Sand (2021)", BggLink = "https://boardgamegeek.com/boardgame/297562/kemet-blood-and-sand" });
                _games.Add(new Game() { Name = "Kingdomino (2016)", BggLink = "https://boardgamegeek.com/boardgame/204583/kingdomino" });
                _games.Add(new Game() { Name = "Skull King (2013)", BggLink = "https://boardgamegeek.com/boardgame/150145/skull-king" });
                _games.Add(new Game() { Name = "Tiletum (2022)", BggLink = "https://boardgamegeek.com/boardgame/351913/tiletum" });
                _games.Add(new Game() { Name = "Marvel United (2020)", BggLink = "https://boardgamegeek.com/boardgame/298047/marvel-united" });
                _games.Add(new Game() { Name = "Steam (2009)", BggLink = "https://boardgamegeek.com/boardgame/27833/steam" });
                _games.Add(new Game() { Name = "Star Realms: Frontiers (2018)", BggLink = "https://boardgamegeek.com/boardgame/230253/star-realms-frontiers" });
                _games.Add(new Game() { Name = "Battle Line (2000)", BggLink = "https://boardgamegeek.com/boardgame/760/battle-line" });
                _games.Add(new Game() { Name = "Hive (2001)", BggLink = "https://boardgamegeek.com/boardgame/2655/hive" });
                _games.Add(new Game() { Name = "Rococo (2013)", BggLink = "https://boardgamegeek.com/boardgame/144344/rococo" });
                _games.Add(new Game() { Name = "Aeon's End: The New Age (2019)", BggLink = "https://boardgamegeek.com/boardgame/270633/aeons-end-the-new-age" });
                _games.Add(new Game() { Name = "Forgotten Waters (2020)", BggLink = "https://boardgamegeek.com/boardgame/302723/forgotten-waters" });
                _games.Add(new Game() { Name = "Distilled (2023)", BggLink = "https://boardgamegeek.com/boardgame/295895/distilled" });
                _games.Add(new Game() { Name = "T.I.M.E Stories (2015)", BggLink = "https://boardgamegeek.com/boardgame/146508/time-stories" });
                _games.Add(new Game() { Name = "Cryptid (2018)", BggLink = "https://boardgamegeek.com/boardgame/246784/cryptid" });
                _games.Add(new Game() { Name = "Ethnos (2017)", BggLink = "https://boardgamegeek.com/boardgame/206718/ethnos" });
                _games.Add(new Game() { Name = "Cthulhu Wars (2015)", BggLink = "https://boardgamegeek.com/boardgame/139976/cthulhu-wars" });
                _games.Add(new Game() { Name = "Sea Salt & Paper (2022)", BggLink = "https://boardgamegeek.com/boardgame/367220/sea-salt-and-paper" });
                _games.Add(new Game() { Name = "Dice Throne: Season Two – Battle Chest (2018)", BggLink = "https://boardgamegeek.com/boardgame/244271/dice-throne-season-two-battle-chest" });
                _games.Add(new Game() { Name = "Turing Machine (2022)", BggLink = "https://boardgamegeek.com/boardgame/356123/turing-machine" });
                _games.Add(new Game() { Name = "Foundations of Rome (2022)", BggLink = "https://boardgamegeek.com/boardgame/284189/foundations-of-rome" });
                _games.Add(new Game() { Name = "Seasons (2012)", BggLink = "https://boardgamegeek.com/boardgame/108745/seasons" });
                _games.Add(new Game() { Name = "Dixit: Odyssey (2011)", BggLink = "https://boardgamegeek.com/boardgame/92828/dixit-odyssey" });
                _games.Add(new Game() { Name = "Faraway (2023)", BggLink = "https://boardgamegeek.com/boardgame/385761/faraway" });
                _games.Add(new Game() { Name = "Apiary (2023)", BggLink = "https://boardgamegeek.com/boardgame/400314/apiary" });
                _games.Add(new Game() { Name = "Arcadia Quest (2014)", BggLink = "https://boardgamegeek.com/boardgame/155068/arcadia-quest" });
                _games.Add(new Game() { Name = "Thunder Road: Vendetta (2023)", BggLink = "https://boardgamegeek.com/boardgame/342070/thunder-road-vendetta" });
                _games.Add(new Game() { Name = "Long Shot: The Dice Game (2022)", BggLink = "https://boardgamegeek.com/boardgame/295374/long-shot-the-dice-game" });
                _games.Add(new Game() { Name = "Telestrations (2009)", BggLink = "https://boardgamegeek.com/boardgame/46213/telestrations" });
                _games.Add(new Game() { Name = "Arkham Horror (Third Edition) (2018)", BggLink = "https://boardgamegeek.com/boardgame/257499/arkham-horror-third-edition" });
                _games.Add(new Game() { Name = "The Manhattan Project: Energy Empire (2016)", BggLink = "https://boardgamegeek.com/boardgame/176734/the-manhattan-project-energy-empire" });
                _games.Add(new Game() { Name = "Onitama (2014)", BggLink = "https://boardgamegeek.com/boardgame/160477/onitama" });
                _games.Add(new Game() { Name = "Indonesia (2005)", BggLink = "https://boardgamegeek.com/boardgame/19777/indonesia" });
                _games.Add(new Game() { Name = "Love Letter (2019)", BggLink = "https://boardgamegeek.com/boardgame/277085/love-letter" });
                _games.Add(new Game() { Name = "Imperial (2006)", BggLink = "https://boardgamegeek.com/boardgame/24181/imperial" });
                _games.Add(new Game() { Name = "Navegador (2010)", BggLink = "https://boardgamegeek.com/boardgame/66589/navegador" });
                _games.Add(new Game() { Name = "Lost Cities (1999)", BggLink = "https://boardgamegeek.com/boardgame/50/lost-cities" });
                _games.Add(new Game() { Name = "Imperium: Classics (2021)", BggLink = "https://boardgamegeek.com/boardgame/318184/imperium-classics" });
                _games.Add(new Game() { Name = "Dungeon Petz (2011)", BggLink = "https://boardgamegeek.com/boardgame/97207/dungeon-petz" });
                _games.Add(new Game() { Name = "The Witcher: Old World (2023)", BggLink = "https://boardgamegeek.com/boardgame/331106/the-witcher-old-world" });
                _games.Add(new Game() { Name = "For Sale (1997)", BggLink = "https://boardgamegeek.com/boardgame/172/for-sale" });
                _games.Add(new Game() { Name = "Cloudspire (2019)", BggLink = "https://boardgamegeek.com/boardgame/262211/cloudspire" });
                _games.Add(new Game() { Name = "Space Alert (2008)", BggLink = "https://boardgamegeek.com/boardgame/38453/space-alert" });
                _games.Add(new Game() { Name = "Power Grid Deluxe: Europe/North America (2014)", BggLink = "https://boardgamegeek.com/boardgame/155873/power-grid-deluxe-europenorth-america" });
                _games.Add(new Game() { Name = "Acquire (1964)", BggLink = "https://boardgamegeek.com/boardgame/5/acquire" });
                _games.Add(new Game() { Name = "Age of Empires III: The Age of Discovery (2007)", BggLink = "https://boardgamegeek.com/boardgame/22545/age-of-empires-iii-the-age-of-discovery" });
                _games.Add(new Game() { Name = "The Great Zimbabwe (2012)", BggLink = "https://boardgamegeek.com/boardgame/111341/the-great-zimbabwe" });
                _games.Add(new Game() { Name = "Cat in the Box: Deluxe Edition (2022)", BggLink = "https://boardgamegeek.com/boardgame/345972/cat-in-the-box-deluxe-edition" });
                _games.Add(new Game() { Name = "The King Is Dead: Second Edition (2020)", BggLink = "https://boardgamegeek.com/boardgame/319966/the-king-is-dead-second-edition" });
                _games.Add(new Game() { Name = "Love Letter (2012)", BggLink = "https://boardgamegeek.com/boardgame/129622/love-letter" });
                _games.Add(new Game() { Name = "Survive: Escape from Atlantis! (1982)", BggLink = "https://boardgamegeek.com/boardgame/2653/survive-escape-from-atlantis" });
                _games.Add(new Game() { Name = "Imperial Settlers (2014)", BggLink = "https://boardgamegeek.com/boardgame/154203/imperial-settlers" });
                _games.Add(new Game() { Name = "The King's Dilemma (2019)", BggLink = "https://boardgamegeek.com/boardgame/245655/the-kings-dilemma" });
                _games.Add(new Game() { Name = "Summoner Wars (Second Edition) (2021)", BggLink = "https://boardgamegeek.com/boardgame/332800/summoner-wars-second-edition" });
                _games.Add(new Game() { Name = "Nemo's War (Second Edition) (2017)", BggLink = "https://boardgamegeek.com/boardgame/187617/nemos-war-second-edition" });
                _games.Add(new Game() { Name = "Glass Road (2013)", BggLink = "https://boardgamegeek.com/boardgame/143693/glass-road" });
                _games.Add(new Game() { Name = "Tikal (1999)", BggLink = "https://boardgamegeek.com/boardgame/54/tikal" });
                _games.Add(new Game() { Name = "Bunny Kingdom (2017)", BggLink = "https://boardgamegeek.com/boardgame/184921/bunny-kingdom" });
                _games.Add(new Game() { Name = "Wayfarers of the South Tigris (2022)", BggLink = "https://boardgamegeek.com/boardgame/350316/wayfarers-of-the-south-tigris" });
                _games.Add(new Game() { Name = "Century: Spice Road (2017)", BggLink = "https://boardgamegeek.com/boardgame/209685/century-spice-road" });
                _games.Add(new Game() { Name = "Fury of Dracula (Third/Fourth Edition) (2015)", BggLink = "https://boardgamegeek.com/boardgame/181279/fury-of-dracula-thirdfourth-edition" });
                _games.Add(new Game() { Name = "Imperial 2030 (2009)", BggLink = "https://boardgamegeek.com/boardgame/54138/imperial-2030" });
                _games.Add(new Game() { Name = "Bruges (2013)", BggLink = "https://boardgamegeek.com/boardgame/136888/bruges" });
                _games.Add(new Game() { Name = "Summoner Wars: Master Set (2011)", BggLink = "https://boardgamegeek.com/boardgame/93260/summoner-wars-master-set" });
                _games.Add(new Game() { Name = "Tiny Epic Galaxies (2015)", BggLink = "https://boardgamegeek.com/boardgame/163967/tiny-epic-galaxies" });
                _games.Add(new Game() { Name = "Awkward Guests: The Walton Case (2016)", BggLink = "https://boardgamegeek.com/boardgame/188866/awkward-guests-the-walton-case" });
                _games.Add(new Game() { Name = "Heaven & Ale (2017)", BggLink = "https://boardgamegeek.com/boardgame/227789/heaven-and-ale" });
                _games.Add(new Game() { Name = "Nusfjord (2017)", BggLink = "https://boardgamegeek.com/boardgame/234277/nusfjord" });
                _games.Add(new Game() { Name = "Dorfromantik: The Board Game (2022)", BggLink = "https://boardgamegeek.com/boardgame/370591/dorfromantik-the-board-game" });
                _games.Add(new Game() { Name = "Chinatown (1999)", BggLink = "https://boardgamegeek.com/boardgame/47/chinatown" });
                _games.Add(new Game() { Name = "Destinies (2021)", BggLink = "https://boardgamegeek.com/boardgame/285192/destinies" });
                _games.Add(new Game() { Name = "Andromeda's Edge (2024)", BggLink = "https://boardgamegeek.com/boardgame/358661/andromedas-edge" });
                _games.Add(new Game() { Name = "Runewars (2010)", BggLink = "https://boardgamegeek.com/boardgame/59294/runewars" });
                _games.Add(new Game() { Name = "Alien Frontiers (2010)", BggLink = "https://boardgamegeek.com/boardgame/48726/alien-frontiers" });
                _games.Add(new Game() { Name = "Project L (2020)", BggLink = "https://boardgamegeek.com/boardgame/260180/project-l" });
                _games.Add(new Game() { Name = "Exit: The Game – The Abandoned Cabin (2016)", BggLink = "https://boardgamegeek.com/boardgame/203420/exit-the-game-the-abandoned-cabin" });
                _games.Add(new Game() { Name = "Flamecraft (2022)", BggLink = "https://boardgamegeek.com/boardgame/336986/flamecraft" });
                _games.Add(new Game() { Name = "Small World (2009)", BggLink = "https://boardgamegeek.com/boardgame/40692/small-world" });
                _games.Add(new Game() { Name = "Burgle Bros. (2015)", BggLink = "https://boardgamegeek.com/boardgame/172081/burgle-bros" });
                _games.Add(new Game() { Name = "Raiders of Scythia (2020)", BggLink = "https://boardgamegeek.com/boardgame/301880/raiders-of-scythia" });
                _games.Add(new Game() { Name = "Above and Below (2015)", BggLink = "https://boardgamegeek.com/boardgame/172818/above-and-below" });
                _games.Add(new Game() { Name = "Innovation (2010)", BggLink = "https://boardgamegeek.com/boardgame/63888/innovation" });
                _games.Add(new Game() { Name = "Merchants & Marauders (2010)", BggLink = "https://boardgamegeek.com/boardgame/25292/merchants-and-marauders" });
                _games.Add(new Game() { Name = "Bonfire (2020)", BggLink = "https://boardgamegeek.com/boardgame/304420/bonfire" });
                _games.Add(new Game() { Name = "Dead of Winter: The Long Night (2016)", BggLink = "https://boardgamegeek.com/boardgame/193037/dead-of-winter-the-long-night" });
                _games.Add(new Game() { Name = "Neuroshima Hex (2006)", BggLink = "https://boardgamegeek.com/boardgame/21241/neuroshima-hex" });
                _games.Add(new Game() { Name = "Sidereal Confluence (2017)", BggLink = "https://boardgamegeek.com/boardgame/202426/sidereal-confluence" });
                _games.Add(new Game() { Name = "Arboretum (2015)", BggLink = "https://boardgamegeek.com/boardgame/140934/arboretum" });
                _games.Add(new Game() { Name = "Mindbug: First Contact (2022)", BggLink = "https://boardgamegeek.com/boardgame/345584/mindbug-first-contact" });
                _games.Add(new Game() { Name = "Furnace (2020)", BggLink = "https://boardgamegeek.com/boardgame/318084/furnace" });
                _games.Add(new Game() { Name = "Antiquity (2004)", BggLink = "https://boardgamegeek.com/boardgame/13122/antiquity" });
                _games.Add(new Game() { Name = "Bärenpark (2017)", BggLink = "https://boardgamegeek.com/boardgame/219513/barenpark" });
                _games.Add(new Game() { Name = "Sid Meier's Civilization: The Board Game (2010)", BggLink = "https://boardgamegeek.com/boardgame/77130/sid-meiers-civilization-the-board-game" });
                _games.Add(new Game() { Name = "Mission: Red Planet (Second/Third Edition) (2015)", BggLink = "https://boardgamegeek.com/boardgame/176920/mission-red-planet-secondthird-edition" });
                _games.Add(new Game() { Name = "Letters from Whitechapel (2011)", BggLink = "https://boardgamegeek.com/boardgame/59959/letters-from-whitechapel" });
                _games.Add(new Game() { Name = "Gizmos (2018)", BggLink = "https://boardgamegeek.com/boardgame/246192/gizmos" });
                _games.Add(new Game() { Name = "Marvel United: X-Men (2021)", BggLink = "https://boardgamegeek.com/boardgame/336382/marvel-united-x-men" });
                _games.Add(new Game() { Name = "Dixit (2008)", BggLink = "https://boardgamegeek.com/boardgame/39856/dixit" });
                _games.Add(new Game() { Name = "Dungeon Lords (2009)", BggLink = "https://boardgamegeek.com/boardgame/45315/dungeon-lords" });
                _games.Add(new Game() { Name = "Tekhenu: Obelisk of the Sun (2020)", BggLink = "https://boardgamegeek.com/boardgame/297030/tekhenu-obelisk-of-the-sun" });
                _games.Add(new Game() { Name = "Harry Potter: Hogwarts Battle (2016)", BggLink = "https://boardgamegeek.com/boardgame/199042/harry-potter-hogwarts-battle" });
                _games.Add(new Game() { Name = "The Guild of Merchant Explorers (2022)", BggLink = "https://boardgamegeek.com/boardgame/350933/the-guild-of-merchant-explorers" });
                _games.Add(new Game() { Name = "Pax Renaissance: 2nd Edition (2021)", BggLink = "https://boardgamegeek.com/boardgame/308119/pax-renaissance-2nd-edition" });
                _games.Add(new Game() { Name = "BattleLore: Second Edition (2013)", BggLink = "https://boardgamegeek.com/boardgame/146439/battlelore-second-edition" });
                _games.Add(new Game() { Name = "The Lord of the Rings: The Card Game – Revised Core Set (2022)", BggLink = "https://boardgamegeek.com/boardgame/349067/the-lord-of-the-rings-the-card-game-revised-core-s" });
                _games.Add(new Game() { Name = "Ginkgopolis (2012)", BggLink = "https://boardgamegeek.com/boardgame/128271/ginkgopolis" });
                _games.Add(new Game() { Name = "Takenoko (2011)", BggLink = "https://boardgamegeek.com/boardgame/70919/takenoko" });
                _games.Add(new Game() { Name = "Dead Reckoning (2022)", BggLink = "https://boardgamegeek.com/boardgame/276182/dead-reckoning" });
                _games.Add(new Game() { Name = "Expeditions (2023)", BggLink = "https://boardgamegeek.com/boardgame/379078/expeditions" });
                _games.Add(new Game() { Name = "Endeavor: Deep Sea (2024)", BggLink = "https://boardgamegeek.com/boardgame/367966/endeavor-deep-sea" });
                _games.Add(new Game() { Name = "Frostpunk: The Board Game (2022)", BggLink = "https://boardgamegeek.com/boardgame/311988/frostpunk-the-board-game" });
                _games.Add(new Game() { Name = "Mysterium (2015)", BggLink = "https://boardgamegeek.com/boardgame/181304/mysterium" });
                _games.Add(new Game() { Name = "Air, Land, & Sea (2019)", BggLink = "https://boardgamegeek.com/boardgame/247367/air-land-and-sea" });
                _games.Add(new Game() { Name = "Maria (2009)", BggLink = "https://boardgamegeek.com/boardgame/40354/maria" });
                _games.Add(new Game() { Name = "Ghost Stories (2008)", BggLink = "https://boardgamegeek.com/boardgame/37046/ghost-stories" });
                _games.Add(new Game() { Name = "Carpe Diem (2018)", BggLink = "https://boardgamegeek.com/boardgame/245934/carpe-diem" });
                _games.Add(new Game() { Name = "Abyss (2014)", BggLink = "https://boardgamegeek.com/boardgame/155987/abyss" });
                _games.Add(new Game() { Name = "San Juan (2004)", BggLink = "https://boardgamegeek.com/boardgame/8217/san-juan" });
                _games.Add(new Game() { Name = "The Resistance (2009)", BggLink = "https://boardgamegeek.com/boardgame/41114/the-resistance" });
                _games.Add(new Game() { Name = "Schotten Totten (1999)", BggLink = "https://boardgamegeek.com/boardgame/372/schotten-totten" });
                _games.Add(new Game() { Name = "Agricola: All Creatures Big and Small (2012)", BggLink = "https://boardgamegeek.com/boardgame/119890/agricola-all-creatures-big-and-small" });
                _games.Add(new Game() { Name = "Blood Bowl: Team Manager – The Card Game (2011)", BggLink = "https://boardgamegeek.com/boardgame/90137/blood-bowl-team-manager-the-card-game" });
                _games.Add(new Game() { Name = "Unmatched: Robin Hood vs. Bigfoot (2019)", BggLink = "https://boardgamegeek.com/boardgame/274638/unmatched-robin-hood-vs-bigfoot" });
                _games.Add(new Game() { Name = "Star Wars: Armada (2015)", BggLink = "https://boardgamegeek.com/boardgame/163745/star-wars-armada" });
                _games.Add(new Game() { Name = "Lords of Hellas (2018)", BggLink = "https://boardgamegeek.com/boardgame/222509/lords-of-hellas" });
                _games.Add(new Game() { Name = "51st State: Master Set (2016)", BggLink = "https://boardgamegeek.com/boardgame/192458/51st-state-master-set" });
                _games.Add(new Game() { Name = "Hannibal: Rome vs. Carthage (1996)", BggLink = "https://boardgamegeek.com/boardgame/234/hannibal-rome-vs-carthage" });
                _games.Add(new Game() { Name = "Marrakesh (2022)", BggLink = "https://boardgamegeek.com/boardgame/342810/marrakesh" });
                _games.Add(new Game() { Name = "Smartphone Inc. (2018)", BggLink = "https://boardgamegeek.com/boardgame/246684/smartphone-inc" });
                _games.Add(new Game() { Name = "Newton (2018)", BggLink = "https://boardgamegeek.com/boardgame/244711/newton" });
                _games.Add(new Game() { Name = "Altiplano (2017)", BggLink = "https://boardgamegeek.com/boardgame/234487/altiplano" });
                _games.Add(new Game() { Name = "The Pillars of the Earth (2006)", BggLink = "https://boardgamegeek.com/boardgame/24480/the-pillars-of-the-earth" });
                _games.Add(new Game() { Name = "Notre Dame (2007)", BggLink = "https://boardgamegeek.com/boardgame/25554/notre-dame" });
                _games.Add(new Game() { Name = "Fall of Rome (2018)", BggLink = "https://boardgamegeek.com/boardgame/260428/fall-of-rome" });
                _games.Add(new Game() { Name = "Keep the Heroes Out! (2022)", BggLink = "https://boardgamegeek.com/boardgame/333255/keep-the-heroes-out" });
                _games.Add(new Game() { Name = "Twice as Clever! (2019)", BggLink = "https://boardgamegeek.com/boardgame/269210/twice-as-clever" });
                _games.Add(new Game() { Name = "The Godfather: Corleone's Empire (2017)", BggLink = "https://boardgamegeek.com/boardgame/195539/the-godfather-corleones-empire" });
                _games.Add(new Game() { Name = "At the Gates of Loyang (2009)", BggLink = "https://boardgamegeek.com/boardgame/39683/at-the-gates-of-loyang" });
                _games.Add(new Game() { Name = "Mage Wars Arena (2012)", BggLink = "https://boardgamegeek.com/boardgame/101721/mage-wars-arena" });
                _games.Add(new Game() { Name = "Saint Petersburg (2004)", BggLink = "https://boardgamegeek.com/boardgame/9217/saint-petersburg" });
                _games.Add(new Game() { Name = "Gùgōng (2018)", BggLink = "https://boardgamegeek.com/boardgame/250458/gugong" });
                _games.Add(new Game() { Name = "Cartographers Heroes (2021)", BggLink = "https://boardgamegeek.com/boardgame/315767/cartographers-heroes" });
                _games.Add(new Game() { Name = "Honey Buzz (2020)", BggLink = "https://boardgamegeek.com/boardgame/284742/honey-buzz" });
                _games.Add(new Game() { Name = "Arkham Horror (2005)", BggLink = "https://boardgamegeek.com/boardgame/15987/arkham-horror" });
                _games.Add(new Game() { Name = "Chess (1475)", BggLink = "https://boardgamegeek.com/boardgame/171/chess" });
                _games.Add(new Game() { Name = "Akropolis (2022)", BggLink = "https://boardgamegeek.com/boardgame/357563/akropolis" });
                _games.Add(new Game() { Name = "King of Tokyo (2011)", BggLink = "https://boardgamegeek.com/boardgame/70323/king-of-tokyo" });
                _games.Add(new Game() { Name = "Blitzkrieg!: World War Two in 20 Minutes (2019)", BggLink = "https://boardgamegeek.com/boardgame/258210/blitzkrieg-world-war-two-in-20-minutes" });
                _games.Add(new Game() { Name = "Skull (2011)", BggLink = "https://boardgamegeek.com/boardgame/92415/skull" });
                _games.Add(new Game() { Name = "Dice Forge (2017)", BggLink = "https://boardgamegeek.com/boardgame/194594/dice-forge" });
                _games.Add(new Game() { Name = "Downforce (2017)", BggLink = "https://boardgamegeek.com/boardgame/215311/downforce" });
                _games.Add(new Game() { Name = "Ready Set Bet (2022)", BggLink = "https://boardgamegeek.com/boardgame/351040/ready-set-bet" });
                _games.Add(new Game() { Name = "Die Macher (1986)", BggLink = "https://boardgamegeek.com/boardgame/1/die-macher" });
                _games.Add(new Game() { Name = "IKI (2015)", BggLink = "https://boardgamegeek.com/boardgame/177478/iki" });
                _games.Add(new Game() { Name = "Tiny Towns (2019)", BggLink = "https://boardgamegeek.com/boardgame/265736/tiny-towns" });
                _games.Add(new Game() { Name = "In the Year of the Dragon (2007)", BggLink = "https://boardgamegeek.com/boardgame/31594/in-the-year-of-the-dragon" });
                _games.Add(new Game() { Name = "Descent: Legends of the Dark (2021)", BggLink = "https://boardgamegeek.com/boardgame/322708/descent-legends-of-the-dark" });
                _games.Add(new Game() { Name = "Endeavor (2009)", BggLink = "https://boardgamegeek.com/boardgame/33160/endeavor" });
                _games.Add(new Game() { Name = "Ticket to Ride: Märklin (2006)", BggLink = "https://boardgamegeek.com/boardgame/21348/ticket-to-ride-marklin" });
                _games.Add(new Game() { Name = "London (Second Edition) (2017)", BggLink = "https://boardgamegeek.com/boardgame/236191/london-second-edition" });
                _games.Add(new Game() { Name = "The Vale of Eternity (2023)", BggLink = "https://boardgamegeek.com/boardgame/385529/the-vale-of-eternity" });
                _games.Add(new Game() { Name = "Cubitos (2021)", BggLink = "https://boardgamegeek.com/boardgame/298069/cubitos" });
                _games.Add(new Game() { Name = "Pan Am (2020)", BggLink = "https://boardgamegeek.com/boardgame/303057/pan-am" });
                _games.Add(new Game() { Name = "Boonlake (2021)", BggLink = "https://boardgamegeek.com/boardgame/343905/boonlake" });
                _games.Add(new Game() { Name = "Azul: Stained Glass of Sintra (2018)", BggLink = "https://boardgamegeek.com/boardgame/256226/azul-stained-glass-of-sintra" });
                _games.Add(new Game() { Name = "Bruxelles 1893 (2013)", BggLink = "https://boardgamegeek.com/boardgame/144592/bruxelles-1893" });
                _games.Add(new Game() { Name = "Codenames: Pictures (2016)", BggLink = "https://boardgamegeek.com/boardgame/198773/codenames-pictures" });
                _games.Add(new Game() { Name = "San Juan (Second Edition) (2014)", BggLink = "https://boardgamegeek.com/boardgame/166669/san-juan-second-edition" });
                _games.Add(new Game() { Name = "Spartacus: A Game of Blood and Treachery (2012)", BggLink = "https://boardgamegeek.com/boardgame/128671/spartacus-a-game-of-blood-and-treachery" });
                _games.Add(new Game() { Name = "King of Tokyo: Dark Edition (2020)", BggLink = "https://boardgamegeek.com/boardgame/293141/king-of-tokyo-dark-edition" });
                _games.Add(new Game() { Name = "Imperial Struggle (2020)", BggLink = "https://boardgamegeek.com/boardgame/206480/imperial-struggle" });
                _games.Add(new Game() { Name = "Civilization (1980)", BggLink = "https://boardgamegeek.com/boardgame/71/civilization" });
                _games.Add(new Game() { Name = "Evolution: Climate (2016)", BggLink = "https://boardgamegeek.com/boardgame/182134/evolution-climate" });
                _games.Add(new Game() { Name = "Firefly: The Game (2013)", BggLink = "https://boardgamegeek.com/boardgame/138161/firefly-the-game" });
                _games.Add(new Game() { Name = "Biblios (2007)", BggLink = "https://boardgamegeek.com/boardgame/34219/biblios" });
                _games.Add(new Game() { Name = "Here I Stand (2006)", BggLink = "https://boardgamegeek.com/boardgame/17392/here-i-stand" });
                _games.Add(new Game() { Name = "Mythic Battles: Pantheon (2017)", BggLink = "https://boardgamegeek.com/boardgame/186751/mythic-battles-pantheon" });
                _games.Add(new Game() { Name = "The Great Wall (2021)", BggLink = "https://boardgamegeek.com/boardgame/292375/the-great-wall" });
                _games.Add(new Game() { Name = "Wavelength (2019)", BggLink = "https://boardgamegeek.com/boardgame/262543/wavelength" });
                _games.Add(new Game() { Name = "Imperium: Legends (2021)", BggLink = "https://boardgamegeek.com/boardgame/318182/imperium-legends" });
                _games.Add(new Game() { Name = "Sword & Sorcery (2017)", BggLink = "https://boardgamegeek.com/boardgame/170771/sword-and-sorcery" });
                _games.Add(new Game() { Name = "Santa Maria (2017)", BggLink = "https://boardgamegeek.com/boardgame/229220/santa-maria" });
                _games.Add(new Game() { Name = "Millennium Blades (2016)", BggLink = "https://boardgamegeek.com/boardgame/151347/millennium-blades" });
                _games.Add(new Game() { Name = "Space Hulk (Third Edition) (2009)", BggLink = "https://boardgamegeek.com/boardgame/54625/space-hulk-third-edition" });
                _games.Add(new Game() { Name = "Sprawlopolis (2018)", BggLink = "https://boardgamegeek.com/boardgame/251658/sprawlopolis" });
                _games.Add(new Game() { Name = "Sherlock Holmes Consulting Detective: Jack the Ripper & West End Adventures (2016)", BggLink = "https://boardgamegeek.com/boardgame/204305/sherlock-holmes-consulting-detective-jack-the-ripp" });
                _games.Add(new Game() { Name = "Fresco (2010)", BggLink = "https://boardgamegeek.com/boardgame/66188/fresco" });
                _games.Add(new Game() { Name = "Flash Point: Fire Rescue (2011)", BggLink = "https://boardgamegeek.com/boardgame/100901/flash-point-fire-rescue" });
                _games.Add(new Game() { Name = "Macao (2009)", BggLink = "https://boardgamegeek.com/boardgame/55670/macao" });
                _games.Add(new Game() { Name = "Clash of Cultures (2012)", BggLink = "https://boardgamegeek.com/boardgame/40765/clash-of-cultures" });
                _games.Add(new Game() { Name = "Nippon (2015)", BggLink = "https://boardgamegeek.com/boardgame/154809/nippon" });
                _games.Add(new Game() { Name = "Stockpile (2015)", BggLink = "https://boardgamegeek.com/boardgame/161614/stockpile" });
                _games.Add(new Game() { Name = "Woodcraft (2022)", BggLink = "https://boardgamegeek.com/boardgame/355093/woodcraft" });
                _games.Add(new Game() { Name = "Kingsburg (2007)", BggLink = "https://boardgamegeek.com/boardgame/27162/kingsburg" });
                _games.Add(new Game() { Name = "Commands & Colors: Napoleonics (2010)", BggLink = "https://boardgamegeek.com/boardgame/62222/commands-and-colors-napoleonics" });
                _games.Add(new Game() { Name = "Dune: War for Arrakis (2024)", BggLink = "https://boardgamegeek.com/boardgame/367150/dune-war-for-arrakis" });
                _games.Add(new Game() { Name = "Caper: Europe (2022)", BggLink = "https://boardgamegeek.com/boardgame/328565/caper-europe" });
                _games.Add(new Game() { Name = "PitchCar (1995)", BggLink = "https://boardgamegeek.com/boardgame/150/pitchcar" });
                _games.Add(new Game() { Name = "Mice and Mystics (2012)", BggLink = "https://boardgamegeek.com/boardgame/124708/mice-and-mystics" });
            }
        }

        /// <summary>
        /// Backing member variable for the single instance of the MainViewModel class, accessed via the public get
        /// accessor for Instance.
        /// </summary>
        private static MainViewModel _Instance = null;

        /// <summary>
        /// Lock to be held when creating the single instance for the MainViewModel class.
        /// </summary>
        private static readonly LobsterLock _InstanceLock = new LobsterLock();

    }
}
