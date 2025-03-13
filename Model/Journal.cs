using LobsterConnect.VM;
using System.Runtime.InteropServices;


namespace LobsterConnect.Model
{
    /// <summary>
    /// The journal of changes to the LobsterConnect data store.  By replaying a journal, you bring the datastore up to date.
    /// The types of entity and the available types of changes are:
    /// 
    /// Game (Create and Update only): a game that can be played at a session
    ///     ID is the game's full name
    ///     Attributes are:
    ///         BGGLINK: the URL for the game on the BGG site
    ///         ISACTIVE: True or False; if false then the game is not available for sign-up
    ///         
    /// Person (Create and Update only): a person who can sign in to the app, and organise and join gaming sessions
    ///     ID is the person's handle
    ///     Attributes are:
    ///         FULLNAME: the full name of the person
    ///         PHONENUMBER: the person's phone number
    ///         EMAIL: the person's email address
    ///         PASSWORD: the hash of the person's password         
    ///         ISACTIVE: True or False; if false then the person is not allowed to organise or participate in games
    ///         ISADMIN: True or False; if true then the UI will allow this person change things belonging to other people
    ///         
    /// Session (Create and Update only): a session to play a certain game, at a certain event, at a certain time
    ///     ID is an opaque ID (normally a GUID).
    ///     Journal entries for sessions must always include EVENTNAME, the name of the gaming event at which the session happens.
    ///     This value cannot be updated after a session has been created.
    ///     Attributes are:
    ///         EVENTNAME (Mandatory in all journal entries, and immutable): the name of the event at which the session is happening
    ///         PROPOSER: the handle of the person who's organising the session (mandatory and immutable)
    ///         TOPLAY: the name of the game that will be played (mandatory and immutable)
    ///         STARTAT: the day/time of the session; it's a label string, then ':', then a number which is the session number in the event
    ///         WHATSAPPLINK: a link to a WhatsApp chat for discussing the game session
    ///         BGGLINK: a link to the BGG site describing the game
    ///         NOTES: explanatory notes written by the person organising the session
    ///         SITSMINIMUM: an integer, the lowest number of people who are being sought to play the game at this session
    ///         SITSMAXIMUM: an integer, the highest number of people who are being sought to play the game at this session
    ///         STATE: one of: OPEN = looking for people to play; FULL = no more players are wanted, the game will take place; ABANDONED = the game will not take place
    ///         
    /// SignUp (Create and Delete only): a record that a certain person is going to play in a certain session
    ///     ID consists of a person ID then ',' and then a Session ID.
    ///     Journal entries for sign-ups must always include EVENTNAME, the name of the gaming event at which the session happens
    ///     Attributes are:
    ///         EVENTNAME (Mandatory in all journal entries): the name of the gaming event at which the session is happening
    ///         MODIFIEDBY: the handle of the person making this change (creating or deleting the sign-up)
    ///         
    /// GamingEvent (Create and Update only): a record of an event (an evening, a gaming day, a convention) at which games can be played.
    ///     ID consists of the event's name. Note that EVENTTYPE is immutable; you can specify it in a Create entry, but it can't be
    ///     modified by a subsequent Update entry (because changing an event's type would allow changes that would invalidate
    ///     existing sessions' slot times).
    ///     Attributes are:
    ///         EVENTTYPE: EVENING, DAY, CONVENTION; Application logic dictates how many session times
    ///         are available for an event, according to its event type.  This attribute is immutable.
    ///         ISACTIVE: True or False; if false then sessions can't be set up at this event     
    ///     
    /// NB: IDs and attribute values are all strings.  It is an error if any of these strings contains the characters '\' or '|',
    ///     or the new-line character '\n'.
    ///     The IDs of person entities are, in addition, not allowed to include the character ','
    /// 
    /// </summary>
    public class Journal
    {
        public static void AddJournalEntry(EntityType entityType, OperationType operationType, string entityId, params string[] journalParameters)
        {
            if (journalParameters == null || journalParameters.Length == 0)
            {
                AddJournalEntry(entityType, operationType, entityId, new List<string>());
            }
            else
            {
                List<string> paramsList = new List<string>(journalParameters);

                AddJournalEntry(entityType, operationType, entityId, paramsList);
            }
        }

        public static void AddJournalEntry(EntityType entityType, OperationType operationType, string entityId, List<string> journalParameters)
        {
            if (journalParameters.Count % 2 == 1)
            {
                Logger.LogMessage(Logger.Level.ERROR, "Journal.AddJournalEntry", "The number of parameters is odd - it must be even");
                return;
            }

            JournalEntry e;
            lock (_LocalJournalLock)
            {
                e = new JournalEntry(0, _LocalJournalNextSeq++, Model.Utilities.InstallationId, entityType, operationType, entityId, journalParameters);
                _LocalJournal.Add(e);
            }

            Journal.AddEntryToQueue(e);
        }

        public enum EntityType
        {
            Game, Person, Session, SignUp, GamingEvent
        }

        public enum OperationType
        {
            Create, Update, Delete
        }

        private static List<JournalEntry> _LocalJournal = new List<JournalEntry>();
        private static Int64 _LocalJournalNextSeq = 1;
        private static LobsterLock _LocalJournalLock = new LobsterLock(); // lock while changing or viewing the two items above

        public class JournalEntry()
        {
            public JournalEntry(Int64 cloudSeq, Int64 localSeq, string installationId, EntityType e, OperationType o, string i, List<string> p) : this()
            {
                _cloudSeq = cloudSeq;
                _localSeq = localSeq;
                _installationId = installationId;

                if (e == EntityType.Session || e == EntityType.SignUp)
                {
                    _gamingEventFilter = GetParameterValue("EVENTNAME", p,"");
                }
                else
                {
                    _gamingEventFilter = "";
                }

                _entityType = e;
                _operationType = o;
                _entityId = i;
                _parameters = p;

                if (this._gamingEventFilter.Contains('\\'))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "JournalEntry ctor", "event filter '" + this._gamingEventFilter + "' contains invalid character \\");
                    this._gamingEventFilter = this._gamingEventFilter.Replace('\\', '_');
                }

                if (this._gamingEventFilter.Contains('|'))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "JournalEntry ctor", "event filter '" + this._gamingEventFilter + "' contains invalid character |");
                    this._gamingEventFilter = this._gamingEventFilter.Replace('|', '_');
                }

                if (this._gamingEventFilter.Contains('\n'))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "JournalEntry ctor", "event filter '" + this._gamingEventFilter + "' contains invalid new-line character");
                    this._gamingEventFilter = this._gamingEventFilter.Replace('\n', '_');
                }

                if (this._entityId.Contains('\\'))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "JournalEntry ctor", "id '" + this._entityId + "' contains invalid character \\");
                    this._entityId = this._entityId.Replace('\\', '_');
                }

                if (this._entityId.Contains('|'))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "JournalEntry ctor", "id '" + this._entityId + "' contains invalid character |");
                    this._entityId = this._entityId.Replace('|', '_');
                }

                if (this._entityId.Contains('\n'))
                {
                    Logger.LogMessage(Logger.Level.ERROR, "JournalEntry ctor", "id '" + this._entityId + "' contains invalid new-line character");
                    this._entityId = this._entityId.Replace('\n', '_');
                }

                for (int ii=1; ii<_parameters.Count; ii+=2)
                {
                    if (this._parameters[ii].Contains('\\'))
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "JournalEntry ctor", "parameter value '" + this._parameters[ii] + "' contains invalid character \\");
                        this._parameters[ii] = this._parameters[ii].Replace('\\', '_');
                    }

                    if (this._parameters[ii].Contains('|'))
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "JournalEntry ctor", "parameter value '" + this._parameters[ii] + "' contains invalid character |");
                        this._parameters[ii] = this._parameters[ii].Replace('|', '_');
                    }

                    if (this._parameters[ii].Contains('|'))
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "JournalEntry ctor", "parameter value '" + this._parameters[ii] + "' contains invalid new-line character");
                        this._parameters[ii] = this._parameters[ii].Replace('\n', '_');
                    }
                }
            }

            public JournalEntry(string s) : this()
            {
                if (string.IsNullOrEmpty(s))
                    throw new Exception("JournalEntry(string) ctor: string cannot be null or empty");

                if(!s.Contains('\\'))
                    throw new Exception("JournalEntry(string) ctor: string is badly formatted");

                string cloudSeqString = null;
                string localSeqString = null;
                string installationId = null;
                string gamingEventFilter = null;
                string entityTypeString = null;
                string operationTypeString = null;
                string entityIdString = null;
                string parameters = null;

                string[] ss = s.Split('\\');
                if(ss.Length<7)
                {
                    throw new Exception("JournalEntry(string) ctor: string contains too few attributes");
                }
                cloudSeqString = ss[0];
                localSeqString = ss[1];
                installationId = ss[2];
                gamingEventFilter = ss[3];
                entityTypeString = ss[4];
                operationTypeString = ss[5];
                entityIdString = ss[6];
                if (ss.Length == 7)
                    parameters = "";
                else
                    parameters = ss[7];

                if(!Int64.TryParse(cloudSeqString,
                    System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture,
                    out this._cloudSeq))
                {
                    throw new Exception("JournalEntry(string) ctor: cloud sequence number malformed");
                }

                if (!Int64.TryParse(localSeqString,
                    System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture,
                    out this._localSeq))
                {
                    throw new Exception("JournalEntry(string) ctor: local sequence number malformed");
                }

                this._installationId = installationId;

                this._gamingEventFilter = gamingEventFilter;

                switch(entityTypeString.ToUpperInvariant())
                {
                    case "GAMINGEVENT": this._entityType = EntityType.GamingEvent; break;
                    case "GAME": this._entityType = EntityType.Game; break;
                    case "PERSON": this._entityType = EntityType.Person; break;
                    case "SESSION": this._entityType = EntityType.Session; break;
                    case "SIGNUP": this._entityType = EntityType.SignUp; break;
                    default: throw new Exception("JournalEntry(string) ctor: entity type invalid");
                }

                switch (operationTypeString.ToUpperInvariant())
                {
                    case "CREATE": this._operationType = OperationType.Create; break;
                    case "UPDATE": this._operationType = OperationType.Update; break;
                    case "DELETE": this._operationType = OperationType.Delete; break;
                    default: throw new Exception("JournalEntry(string) ctor: operation type invalid");
                }

                this._entityId = entityIdString;

                if (string.IsNullOrEmpty(parameters))
                    this._parameters = new List<string>();
                else if (!parameters.Contains('|'))
                {
                    throw new Exception("JournalEntry(string) ctor: parameter string is malformed");
                }
                else
                {
                    this._parameters = new List<string>(parameters.Split('|'));

                    if(this._parameters.Count%2!=0)
                    {
                        throw new Exception("JournalEntry(string) ctor: parameter contains an odd number of items");
                    }
                }
            }

            public override string ToString()
            {
                string cloudSeqString = _cloudSeq.ToString("X16");
                string localSeqString = _localSeq.ToString("X16");
                string entityTypeString = null;
                switch(this._entityType)
                {
                    case EntityType.GamingEvent: entityTypeString = "GamingEvent"; break;
                    case EntityType.Game: entityTypeString = "Game"; break;
                    case EntityType.Person: entityTypeString = "Person"; break;
                    case EntityType.Session: entityTypeString = "Session"; break;
                    case EntityType.SignUp: entityTypeString = "SignUp"; break;
                    default: throw new Exception("JournalEntry.ToString: invalid entity tpe");
                }
                string operationTypeString = null;
                switch(this._operationType)
                {
                    case OperationType.Create: operationTypeString = "Create"; break;
                    case OperationType.Delete: operationTypeString = "Delete"; break;
                    case OperationType.Update: operationTypeString = "Update"; break;
                    default: throw new Exception("JournalEntry.ToString: invalid operation type");
                }

                string s = cloudSeqString + "\\" + localSeqString + "\\" + _installationId + "\\" + _gamingEventFilter + "\\" + entityTypeString + "\\"+ operationTypeString+"\\" + this._entityId + "\\";
                if(this._parameters.Count>0)
                {
                    string p = string.Join('|', this._parameters);
                    s += p;
                }
                return s;
            }

            /// <summary>
            /// Replay this journal entry, into the viewmodel specified, updating its contents accordingly.  You will want to
            /// do this after initially loading the local journal file, and also whenever you've fetched more journal entries
            /// from the cloud.  Note that this method will make various calls to the vm.SyncCheck... methods, which will
            /// detect changes needing a notification to the logged on user (and also changes to the 'event selection' drop
            /// down menu).  If you want to suppress the noticifications to the logged on user, you shoud ensure that, when
            /// you call the Replay method, vm.LoggedOnUser is set to null.
            /// </summary>
            /// <param name="vm">the viewmodel to be updated accordingly to the content of this journal entry</param>
            /// <exception cref="Exception"></exception>
            public void Replay(MainViewModel vm)
            {
                if(!DispatcherHelper.UIDispatcherHasThreadAccess)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "JournalEntry.Replay", "must be called on the dispatcher thread");
                    DispatcherHelper.RunAsyncOnUI(() =>
                        {
                            Replay(vm);
                        });
                    return;
                }

                switch(this._entityType)
                {
                    case EntityType.GamingEvent: ReplayGamingEvent(vm); break;
                    case EntityType.Game: ReplayGame(vm); break;
                    case EntityType.Person: ReplayPerson(vm); break;
                    case EntityType.Session: ReplaySession(vm); break;
                    case EntityType.SignUp: ReplaySignUp(vm); break;
                    default: throw new Exception("JournalEntry.Replay: invalid event type");
                }
            }

            /// <summary>
            /// Replay a gaming event journal entry
            /// </summary>
            /// <param name="vm"></param>
            /// <exception cref="Exception"></exception>
            private void ReplayGamingEvent(MainViewModel vm)
            {
                if(this._operationType==OperationType.Create)
                {
                    vm.CreateGamingEvent(false, this._entityId,
                        GetParameterValue("EVENTTYPE", this._parameters),
                        GetParameterValueBool("ISACTIVE", this._parameters));

                    vm.SyncCheckGamingEvent(this._entityId, GetParameterValueBool("ISACTIVE", this._parameters));

                }
                else if (this._operationType == OperationType.Update)
                {
                    GamingEvent gamingEvent = vm.GetGamingEvent(this._entityId);
                    if(gamingEvent == null)
                    {
                        throw new Exception("JournalEntry.ReplayGamingEvent: gaming event id is invalid");
                    }

                    // Note: EVENTTYPE is immutable; attempts to update it are ignored.
                    vm.UpdateGamingEvent(false, gamingEvent,
                        GetParameterValueBool("ISACTIVE", this._parameters));

                    vm.SyncCheckGamingEvent(this._entityId, GetParameterValueBool("ISACTIVE", this._parameters));
                }
                else
                {
                    throw new Exception("JournalEntry.ReplayGamingEvent: invalid operation type");
                }
            }

            /// <summary>
            /// Replay a game journal entry
            /// </summary>
            /// <param name="vm"></param>
            /// <exception cref="Exception"></exception>
            private void ReplayGame(MainViewModel vm)
            {
                if (this._operationType == OperationType.Create)
                {
                    vm.CreateGame(false, this._entityId,
                        GetParameterValue("BGGLINK", this._parameters,""),
                        GetParameterValueBool("ISACTIVE", this._parameters));
                }
                else if (this._operationType == OperationType.Update)
                {
                    Game game = vm.GetGame(this._entityId);
                    if (game == null)
                    {
                        throw new Exception("JournalEntry.ReplayGame: game id is invalid");
                    }
                    vm.UpdateGame(false, game,
                        GetParameterValue("BGGLINK", this._parameters),
                        GetParameterValueBool("ISACTIVE", this._parameters));
                }
                else
                {
                    throw new Exception("JournalEntry.ReplayGame: invalid operation type");
                }
            }

            /// <summary>
            /// Replay a person journal entry
            /// </summary>
            /// <param name="vm"></param>
            /// <exception cref="Exception"></exception>
            private void ReplayPerson(MainViewModel vm)
            {
                if (this._operationType == OperationType.Create)
                {
                    vm.CreatePerson(false, this._entityId,
                        GetParameterValue("FULLNAME", this._parameters,""),
                        GetParameterValue("PHONENUMBER", this._parameters,""),
                        GetParameterValue("EMAIL", this._parameters,""),
                        GetParameterValue("PASSWORD", this._parameters,""),
                        GetParameterValueBool("ISACTIVE", this._parameters),
                        GetParameterValueBool("ISADMIN", this._parameters));
                }
                else if (this._operationType == OperationType.Update)
                {
                    Person person = vm.GetPerson(this._entityId);
                    if (person == null)
                    {
                        throw new Exception("JournalEntry.ReplayPerson: person hansle is invalid");
                    }
                    vm.UpdatePerson(false, person,
                        GetParameterValue("FULLNAME", this._parameters),
                        GetParameterValue("PHONENUMBER", this._parameters),
                        GetParameterValue("EMAIL", this._parameters),
                        GetParameterValue("PASSWORD", this._parameters),
                        GetParameterValueBool("ISACTIVE", this._parameters),
                        GetParameterValueBool("ISADMIN", this._parameters));

                    vm.SyncCheckPersonUpdate(person.Handle, GetParameterValueBool("ISACTIVE", this._parameters));
                }
                else
                {
                    throw new Exception("JournalEntry.ReplayPerson: invalid operation type");
                }
            }

            /// <summary>
            /// Replay a session journal entry
            /// </summary>
            /// <param name="vm"></param>
            /// <exception cref="Exception"></exception>
            private void ReplaySession(MainViewModel vm)
            {
                if (this._operationType == OperationType.Create)
                {
                    string sessionId = this._entityId;

                    string startTimeText = GetParameterValue("STARTAT", this._parameters,"");
                    if(!startTimeText.Contains(':'))
                    {
                        throw new Exception("JournalEntry.ReplaySession: invalid start time");
                    }
                    string slotTimeString = startTimeText.Split(':')[1];
                    int slotTimeNumber;
                    if(!int.TryParse(slotTimeString, System.Globalization.CultureInfo.InvariantCulture, out slotTimeNumber))
                    {
                        throw new Exception("JournalEntry.ReplaySession: invalid slot number for start time");
                    }
                    SessionTime startAt = new SessionTime(slotTimeNumber);

                    int? sitsMinimum = GetParameterValueInt("SITSMINIMUM", this._parameters);
                    if (sitsMinimum == null) sitsMinimum = 0;

                    int? sitsMaximum = GetParameterValueInt("SITSMAXIMUM", this._parameters);
                    if (sitsMaximum == null) sitsMinimum = 0;

                    string proposer = GetParameterValue("PROPOSER", this._parameters);
                    if(string.IsNullOrEmpty(proposer))
                    {
                        throw new Exception("JournalEntry.ReplaySession: proposer name is null or missing");
                    }

                    string toPlay = GetParameterValue("TOPLAY", this._parameters);
                    if (string.IsNullOrEmpty(toPlay))
                    {
                        throw new Exception("JournalEntry.ReplaySession: game name is null or missing");
                    }

                    vm.CreateSession(false, sessionId,
                        proposer,
                        toPlay,
                        GetParameterValue("EVENTNAME", this._parameters),
                        startAt,
                        false,
                        GetParameterValue("NOTES", this._parameters,""),
                        GetParameterValue("WHATSAPPLINK", this._parameters,""),
                        GetParameterValue("BGGLINK", this._parameters,""),
                        (int) sitsMinimum,
                        (int) sitsMaximum,
                        GetParameterValue("STATE", this._parameters));
                }
                else if (this._operationType == OperationType.Update)
                {
                    Session session = vm.GetSession(this._entityId);
                    vm.UpdateSession(false, session,
                        GetParameterValue("NOTES", this._parameters),
                        GetParameterValue("WHATSAPPLINK", this._parameters),
                        GetParameterValue("BGGLINK", this._parameters),
                        GetParameterValueInt("SITSMINIMUM", this._parameters),
                        GetParameterValueInt("SITSMAXIMUM", this._parameters),
                        GetParameterValue("STATE", this._parameters));

                    vm.SyncCheckSessionUpdate(session, GetParameterValue("STATE", this._parameters), GetParameterValue("NOTES", this._parameters));
                }
                else
                {
                    throw new Exception("JournalEntry.ReplaySession: invalid operation type");
                }
            }

            /// <summary>
            /// Replay a sign-up journal entry
            /// </summary>
            /// <param name="vm"></param>
            /// <exception cref="Exception"></exception>
            private void ReplaySignUp(MainViewModel vm)
            {
                string personAndSession = this._entityId;
                if (!personAndSession.Contains(','))
                {
                    throw new Exception("JournalEntry.ReplaySignUp: id (personHandle,sessionId expected)");
                }
                string personHandle = personAndSession.Split(',')[0];
                string sessionId = personAndSession.Split(',')[1];

                string modifiedBy = GetParameterValue("MODIFIEDBY", this._parameters);

                if (!vm.CheckPersonHandleExists(personHandle))
                {
                    throw new Exception("JournalEntry.ReplaySignUp: personHandle is not recognised: '"+personHandle+"'");
                }

                if (vm.GetSession(sessionId)==null)
                {
                    throw new Exception("JournalEntry.ReplaySignUp: sessionId is not recognised: '" + sessionId + "'");
                }

                if (this._operationType == OperationType.Create)
                {
                    vm.SignUp(false, personHandle, sessionId, false,
                        modifiedBy);

                    vm.SyncCheckSignUp(sessionId, personHandle, modifiedBy);

                }
                else if (this._operationType == OperationType.Delete)
                {
                    vm.CancelSignUp(false, personHandle, sessionId, modifiedBy);

                    vm.SyncCheckCancelSignUp(sessionId, personHandle, modifiedBy);
                }
                else
                {
                    throw new Exception("JournalEntry.ReplaySignUp: invalid operation type");
                }
            }

            /// <summary>
            /// A utility method for retrieving a parameter value from a journal entry parameter list,
            /// which is a list of strings with an even (possibly zero) number of elements whose even elements
            /// are parameter names and whose odd elements are parameter values.
            /// </summary>
            /// <param name="paramName">what parameter to look for (using case-insensitive comparison)</param>
            /// <param name="parameters">the parameter list to search</param>
            /// <param name="defaultValue">the value to return if the indicated parameter is not found</param>
            /// <returns>the parameter value if found, otherwise the defaultValue</returns>
            public static string GetParameterValue(string paramName, List<string> parameters, string defaultValue=null)
            {
                if(parameters==null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "Journal.GetParamterValue", "parameter list should not be null");
                    return defaultValue;
                }
                else if (parameters.Count%2!=0)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "Journal.GetParamterValue", "parameter list should have an even number of elements");
                    return defaultValue;
                }
                else
                {
                    for(int i=0; i<parameters.Count; i+=2)
                    {
                        if (parameters[i].ToUpperInvariant() == paramName.ToUpperInvariant())
                        {
                            return parameters[i + 1];
                        }
                    }
                    return defaultValue;
                }
            }
            /// <summary>
            /// A utility method for retrieving an integer parameter value from a journal entry parameter list,
            /// which is a list of strings with an even (possibly zero) number of elements whose even elements
            /// are parameter names and whose odd elements are parameter values.
            /// </summary>
            /// <param name="paramName">what parameter to look for (using case-insensitive comparison)</param>
            /// <param name="parameters">the parameter list to search</param>
            /// <returns>the parameter value if found, otherwise null</returns>
            public static int? GetParameterValueInt(string paramName, List<string> parameters)
            {
                if (parameters == null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "Journal.GetParameterValueInt", "parameter list should not be null");
                    return null;
                }
                else if (parameters.Count % 2 != 0)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "Journal.GetParameterValueInt", "parameter list should have an even number of elements");
                    return null;
                }
                else
                {
                    for (int i = 0; i < parameters.Count; i += 2)
                    {
                        if (parameters[i].ToUpperInvariant() == paramName.ToUpperInvariant())
                        {
                            int parseResult;
                            if(int.TryParse(parameters[i + 1], System.Globalization.CultureInfo.InvariantCulture, out parseResult))
                            {
                                return parseResult;
                            }
                            else
                            {
                                Logger.LogMessage(Logger.Level.ERROR, "Journal.GetParameterValueInt", "parameter value is not an integer");
                                return null;
                            }
                        }
                    }
                    return null;
                }
            }

            /// <summary>
            /// A utility method for retrieving a bool parameter value from a journal entry parameter list,
            /// which is a list of strings with an even (possibly zero) number of elements whose even elements
            /// are parameter names and whose odd elements are parameter values.
            /// </summary>
            /// <param name="paramName">what parameter to look for (using case-insensitive comparison)</param>
            /// <param name="parameters">the parameter list to search</param>
            /// <returns>the parameter value if found, otherwise null</returns>
            public static bool? GetParameterValueBool(string paramName, List<string> parameters)
            {
                if (parameters == null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "Journal.GetParameterValueBool", "parameter list should not be null");
                    return null;
                }
                else if (parameters.Count % 2 != 0)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "Journal.GetParameterValueBool", "parameter list should have an even number of elements");
                    return null;
                }
                else
                {
                    for (int i = 0; i < parameters.Count; i += 2)
                    {
                        if (parameters[i].ToUpperInvariant() == paramName.ToUpperInvariant())
                        {
                            bool parseResult;
                            if (bool.TryParse(parameters[i + 1], out parseResult))
                            {
                                return parseResult;
                            }
                            else
                            {
                                Logger.LogMessage(Logger.Level.ERROR, "Journal.GetParameterValueBool", "parameter value is not a boolean");
                                return null;
                            }
                        }
                    }
                    return null;
                }
            }

            /// <summary>
            /// The locally created sequence number of this entry, on the device that originated the entry
            /// </summary>
            public Int64 LocalSeq
            {
                get
                {
                    return this._localSeq;
                }
            }

            /// <summary>
            /// The sequence number of this entry in the cloud journal store (it will be 0 in the case of entries
            /// that we haven't yet told the cloud store about - when we hear back from the cloud sync service that
            /// an entry has been uploaded to the cloud store, we update the sequence number to the value provided
            /// by the cloud sync service).
            /// </summary>
            public Int64 CloudSeq
            {
                get
                {
                    return this._cloudSeq;
                }
            }

            /// <summary>
            /// Change the cloud sequence number on this entry.  The appropriate time to do this is when the cloud sync
            /// services has acknlowledged that the entry has been uploaded, and has given us a new sequence number for it.
            /// </summary>
            /// <param name="c"></param>
            public void SetCloudSeq(Int64 c)
            {
                this._cloudSeq = c;
            }

            /// <summary>
            /// The installation id of the device on which this entry originated (i.e. on which it was created by a user
            /// action).
            /// </summary>
            public string InstallationId
            {
                get
                {
                    return this._installationId;
                }
            }

            /// <summary>
            /// The gaming event that this entry relates to.  It's not doing much at the moment, but in the future it could be
            /// used to support selective fetching and loading of journal entries.  The logic would be something like:
            ///  * Set it to null or "" to mean this entry must always be loaded, no matter what the viewmodel's current
            ///    gaming event is.  Gaming events, games and persons would match this case.
            ///  * Set it to the name of a certain gaming event, if this entry only needs loading when the viewmodel's
            ///    gaming event is set to the specified value.  This would be appropriate for sessions and sign-ups.
            /// </summary>
            public string GamingEventFilter
            {
                get
                {
                    return this._gamingEventFilter;
                }
            }

            Int64 _cloudSeq;
            Int64 _localSeq;
            string _installationId;
            string _gamingEventFilter;

            private Journal.EntityType _entityType;
            private Journal.OperationType _operationType;
            string _entityId;
            List<string> _parameters;
        }

        /// <summary>
        /// Call this on the UI thread to read the current local journal file from disc, and replay (i.e. apply) all the
        /// journalled actions in it (loading them all into the viewmodel).
        /// </summary>
        public static void LoadJournal(MainViewModel vm)
        {
            if(!DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "Journal.LoadJournal", "Must be called from the UI thread ");
                DispatcherHelper.RunAsyncOnUI(() => LoadJournal(vm));
                return;
            }

            List<JournalEntry> toReplay = new List<JournalEntry>();
            lock (_LocalJournalLock)
            {
                _LocalJournal.Clear();
                _LocalJournalNextSeq = 0;

                lock (JournalFileLock)
                {
                    string journalFilePath = Path.Combine(App.ProgramFolder, "localjournal.txt");

                    using (Stream fs = System.IO.File.Open(journalFilePath, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        using (StreamReader journalFileReader = new StreamReader(fs))
                        {
                            string line = null;
                            while ((line = journalFileReader.ReadLine()) != null)
                            {
                                try
                                {
                                    JournalEntry entry = new JournalEntry(line);
                                    _LocalJournal.Add(entry);
                                    if (entry.InstallationId == Utilities.InstallationId)
                                    {
                                        if(entry.LocalSeq> _LocalJournalNextSeq)
                                            _LocalJournalNextSeq = entry.LocalSeq;
                                        else
                                        {
                                            Logger.LogMessage(Logger.Level.WARNING, "Journal.LoadJournal", "local sequence number is out of order ("+ entry.LocalSeq.ToString()+ "<" + _LocalJournalNextSeq.ToString()+ ") reading line: " + line);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogMessage(Logger.Level.ERROR, "Journal.LoadJournal", ex, "while reading line: " + line);
                                }
                            }
                        }
                    }
                }
                _LocalJournalNextSeq++;

                Logger.LogMessage(Logger.Level.INFO, "Journal.LoadJournal: loaded " + _LocalJournal.Count.ToString() + " entries");
                Logger.LogMessage(Logger.Level.INFO, "Journal.LoadJournal: next entry to be created will be given local sequence number " + _LocalJournalNextSeq.ToString());


                foreach (JournalEntry entry in _LocalJournal)
                    toReplay.Add(entry);
            }

            // Having released the lock on the journal, now replay all the entries we fetched from it
            foreach (JournalEntry entry in _LocalJournal)
            {
                try
                {
                    entry.Replay(vm);
                }
                catch (Exception ex)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "Journal.LoadJournal", ex, "while replaying: " + entry.ToString());
                }
            }
        }


        /// <summary>
        /// Call this on the UI thread (or any other thread except the worker thread for BackgroundJournalWriter),
        /// to add a new journal entry to the queue.  It will, some time in the next 1,000ms, be picked up from the queue, and written
        /// to the local journal file (i.e. saved to disc).  Some time later, it will be uploaded to the
        /// cloud sync service.
        /// </summary>
        /// <param name="e"></param>
        public static void AddEntryToQueue(JournalEntry e)
        {
            EnsureJournalWorkerRunning();

            lock (QLock)
            {
                Q.Add(e);
            }
        }

        private static List<JournalEntry> Q = new List<JournalEntry>();

        /// <summary>
        /// The worker thread will hold this lock when it needs to prevent another thread from making updates to the queue.
        /// Basically that means 'when it is picking up the entries from the queue so it can process them'.
        /// </summary>
        private static LobsterLock QLock = new LobsterLock();

        /// <summary>
        /// Set this to tell the journal worker that you've done something that you'd like to sync with the cloud now
        /// (rather than waiting for the 'every sixty seconds' refresh cycle.  Don't do it too often, because the sync
        /// operation takes a while, and means re-writing the whole journal file.  But it does make sense to set this
        /// value if the user has used the UI to make a change to something.
        /// </summary>
        public static bool CloudSyncRequested = false;

        /// <summary>
        /// The journal worker will set this variable when it's in the process of doing a journal save or sync cycle,
        /// so as to discourage another cycle from starting.  Maybe some day we could link this to a UI indicator showing
        /// when the app was busy.
        /// </summary>
        public static bool DoingJournalWork = false;

        /// <summary>
        /// Do a cycle of save and sync work.  Every time it's called, this method will empty the queue and write it's entries
        /// to the local journal file.  We expect that to happen every second.
        /// Whenever it's called with an interation number having mod 60 == 1 (or if CloudSyncRequested is true) it will
        /// sync the current journal with the cloud store.  That means that all local journal entries that haven't yet been
        /// assigned cloud sequence numbers will be sent to the cloud (and the new sequence numbers will be fetched and 
        /// applied to the entries in the local journal).  In addition, cloud journal entries that we haven't seen before will be
        /// downloaded and added to the local journal (these correspond to journal entries coming from other devices).
        /// </summary>
        /// <param name="w"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public static bool DoJournalWork(LobsterWorker w, int iteration)
        {
            if (DoingJournalWork)
            {
                Logger.LogMessage(Logger.Level.INFO, "Journal.DoJournalWork","entered while a previous iteration is still running - doing nothing.");
                return true;
            }
            DoingJournalWork = true;
            try
            {
                List<JournalEntry> toDo = new List<JournalEntry>();

                lock (QLock)
                {
                    toDo.AddRange(Q);
                    Q.Clear();
                }

                if (toDo.Count > 0)
                {
                    if (System.Threading.Monitor.TryEnter(JournalFileLock, 10000))
                    {
                        try
                        {
                            // Write the queued entries onto the end of the local journal file.
                            string journalFilePath = Path.Combine(App.ProgramFolder, "localjournal.txt");

                            using (Stream fs = System.IO.File.Open(journalFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                fs.Seek(0, SeekOrigin.End);

                                using (StreamWriter journalFileWriter = new StreamWriter(fs))
                                {
                                    foreach (JournalEntry e in toDo)
                                    {
                                        try
                                        {
                                            journalFileWriter.WriteLine(e.ToString());
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.LogMessage(Logger.Level.ERROR, "Journal.DoJournalWork", ex, "while writing line to incremental journal file");
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            Monitor.Exit(JournalFileLock);
                        }

                    }
                    else
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "Journal.DoJournalWork", "journal file in use");

                        Model.DispatcherHelper.Sleep(10000);

                        lock (QLock)
                        {
                            Q.AddRange(toDo); // Put the entries back on the queue; this is quite unlikely to help, but is worth a go :-)
                        }
                    }
                }

                // Perform the cloud sync
                if (iteration % 60 == 1 || CloudSyncRequested) // iteration = number of seconds - so this runs every minute.
                {
                    CloudSyncRequested = false;

                    Logger.LogMessage(Logger.Level.INFO,"Journal.DoJournalWork","iteration number " + iteration.ToString() + ": journal sync starting...");

                    // syncFrom will end up being set to the highest non-zero cloud seq number in our journal enties;
                    // contentString will be all the journal entries that need sending to the cloud sync service, because
                    // their cloud seq numbers are zero and they originated from this device.
                    List<string> toBeSent = new List<string>();
                    foreach(JournalEntry e in _LocalJournal)
                    {
                        if (e.InstallationId != Utilities.InstallationId) // skip entries that didn't come from this device
                            continue;

                        if (e.CloudSeq != 0) // skip entries that already have cloud seq numbers
                            continue;

                        toBeSent.Add(e.ToString());
                    }
                    string contentString = "";
                    if (toBeSent.Count == 1)
                        contentString = toBeSent[0];
                    else if(toBeSent.Count > 1)
                        contentString= string.Join('\n', toBeSent);

                    Int64 syncFrom = 0;
                    foreach (JournalEntry e in _LocalJournal)
                    {
                        if (e.CloudSeq > syncFrom)
                        {
                            syncFrom = e.CloudSeq;
                        }
                    }

                    if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                    {
                        // unavailable
                        Logger.LogMessage(Logger.Level.INFO, "Journal.DoJournalWork", "iteration number " + iteration.ToString() + ": no internet access; journal synchronisation has not been performed");
                        Model.DispatcherHelper.RunAsyncOnUI(() =>
                        {
                            MainViewModel.Instance.LogUserMessage(Logger.Level.WARNING, "No internet: unable to sync");
                        });
                    }
                    else
                    {
                        System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

                        // Ask the backend to send us every record with a higher Cloud Seq Number than syncFrom (the highest one that we have seen so far)
                        // and send it all the local journal records that so far have not been given a Cloud Seq Number).
                        string postQuery = "https://lobsterconbackend.azurewebsites.net/api/JournalSync?syncFrom=" + syncFrom.ToString("X16")+"&remoteDevice="+Model.Utilities.InstallationId;
                        StringContent postContent = new StringContent(contentString);
                        HttpResponseMessage response = client.PostAsync(postQuery, postContent).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            // The backend should respond with a possibly empty text response consisting of one line of text
                            // for each journal entry that needs to be updated or added, to bring the local journal up to date with the
                            // cloud journal.  Records that need updating will be ones which existed only in the local journal and had no
                            // cloud seq number; these will be updated by putting a cloud seq number on them.  Records that must be added to the local
                            // journal (and must be replayed, to load them into the viewmodel) will be the ones coming from other devices (i.e.
                            // having an installation id different from the local installation id).
                            string content = response.Content.ReadAsStringAsync().Result;
                            int contentCount = 0;
                            if (string.IsNullOrEmpty(content))
                            {
                                Logger.LogMessage(Logger.Level.INFO, "Journal.DoJournalWork", "iteration number " + iteration.ToString() + ": synchronising 0 records");

                            }
                            else // the cloud service has told us about some changes we need to make to the local data
                            {
                                if (!content.Contains('\n'))
                                {
                                    Logger.LogMessage(Logger.Level.INFO, "Journal.DoJournalWork", "iteration number " + iteration.ToString() + ": synchronising 1 record");
                                    Logger.LogMessage(Logger.Level.DEBUG, "Journal.DoJournalWork", "iteration number " + iteration.ToString() + ": synchronising " + content);
                                    SyncCloudEntry(content);
                                    contentCount = 1;
                                }
                                else
                                {
                                    string[] cloudRecords = content.Split('\n');
                                    Logger.LogMessage(Logger.Level.INFO, "Journal.DoJournalWork", "iteration number " + iteration.ToString() + ": synchronising " + cloudRecords.Length + " records");
                                    foreach (string s in cloudRecords)
                                    {
                                        Logger.LogMessage(Logger.Level.DEBUG, "Journal.DoJournalWork", "iteration number " + iteration.ToString() + ": synchronising " + s);
                                        SyncCloudEntry(s);
                                    }
                                    contentCount = cloudRecords.Length;
                                }

                                // The local journal now needs to be written out to file in its entirety, to include all the updates to
                                // cloud seq numbers, and the additional journal lines received from the cloud sync service.
                                // We wite it to a temp file, then use that file to replace the existing journal.

                                string tempFilePath = Path.Combine(App.ProgramFolder, Guid.NewGuid().ToString("N") + ".txt");

                                using (Stream fs = System.IO.File.Open(tempFilePath, FileMode.Create, FileAccess.Write))
                                {
                                    using (StreamWriter journalFileWriter = new StreamWriter(fs))
                                    {
                                        foreach (JournalEntry e in _LocalJournal)
                                        {
                                            try
                                            {
                                                journalFileWriter.WriteLine(e.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.LogMessage(Logger.Level.ERROR, "Journal.DoJournalWork", ex, "while writing line to full journal file");
                                            }
                                        }
                                    }
                                }

                                string journalFilePath = Path.Combine(App.ProgramFolder, "localjournal.txt");

                                lock (JournalFileLock)
                                {
                                    Utilities.FileDeleteIfExists(journalFilePath);
                                    Utilities.FileRename(tempFilePath, "localjournal.txt");
                                }
                            }
                            // Log a message to the UI if the sync actually changed anything
                            if (toBeSent.Count != 0 || contentCount != 0)
                            {
                                Model.DispatcherHelper.RunAsyncOnUI(() =>
                                {
                                    MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Sync completed: " + toBeSent.Count.ToString() + " change(s) uploaded, " + contentCount.ToString() + " downloaded");
                                });
                            }
                        }
                        else
                        {
                            // failure response
                            Logger.LogMessage(Logger.Level.INFO, "Journal.DoJournalWork", "iteration number " + iteration.ToString() + ": failed to sync with cloud service: "+response.StatusCode.ToString()+": "+response.ReasonPhrase);
                            Model.DispatcherHelper.RunAsyncOnUI(() =>
                            {
                                MainViewModel.Instance.LogUserMessage(Logger.Level.WARNING, "Sync failed: " + response.StatusCode.ToString() + ": " + response.ReasonPhrase);
                            });
                        }
                    }

                    Logger.LogMessage(Logger.Level.INFO, "Journal.DoJournalWork","iteration number " + iteration.ToString() + ": journal sync completed");
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(Logger.Level.ERROR,"FredaJournal.DoJournalWork", ex);
                Model.DispatcherHelper.RunAsyncOnUI(() =>
                {
                    MainViewModel.Instance.LogUserMessage(Logger.Level.WARNING, "Sync failed: Error");
                });
            }
            finally
            {
                DoingJournalWork = false;
            }

            return true;
        }

        /// <summary>
        /// Deals with a line of text received in the response from the cloud sync service.  The line is either an
        /// acknowledgement of a local journal entry that we uploaded (now being sent back to us, with a cloud seq
        /// number applied to it) or it is an updated originating from some othher device that needs to be applied to
        /// this device to bring it up to date.
        /// </summary>
        /// <param name="entryText">the text received from the cloud sync service</param>
        private static void SyncCloudEntry(string entryText)
        {
            try
            {
                JournalEntry remote = new JournalEntry(entryText);

                // case 1: e has InstallationId equal to the local device, and the local journal has a record corresponding to
                // the same entry.  In this case we expect to update the local entry's cloud sequence number to the one we
                // have received from the cloud.
                if(remote.InstallationId==Model.Utilities.InstallationId)
                {
                    // find the local entry having the same local id as the remote entry, and having installation id equal to this device
                    JournalEntry local = _LocalJournal.Find(l => l.LocalSeq == remote.LocalSeq && l.InstallationId == Model.Utilities.InstallationId);

                    if(local==null)
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "Journal.SyncCloudEntry: no local entry was found, having local seq number " + remote.LocalSeq.ToString("X8"));
                    }
                    else if (local.CloudSeq!=0)
                    {
                        Logger.LogMessage(Logger.Level.DEBUG, "Journal.SyncCloudEntry: local entry having local seq number " + remote.LocalSeq.ToString("X8")+" has already been given a cloud seq number");
                    }
                    else
                    {
                        local.SetCloudSeq(remote.CloudSeq);
                    }                        
                }
                // case 2: e has InstallationId different from the local device.  In this case, it's an entry that we expect to
                // replay, to update the local viewmodel to bring it into line with updates received from other devices
                else
                {
                    // find the local entry having the same local cloud seq as the remote entry; there should not be any
                    JournalEntry localDuplicate = _LocalJournal.Find(l => l.CloudSeq == remote.CloudSeq);

                    if(localDuplicate!=null)
                    {
                        Logger.LogMessage(Logger.Level.DEBUG, "Journal.SyncCloudEntry: there is already a local entry having cloud seq number " + remote.CloudSeq.ToString("X8") + ", so the corresponding cloud record will not be replayed");
                    }
                    else
                    {
                        // add it to the journal
                        _LocalJournal.Add(remote);
                        Model.DispatcherHelper.RunAsyncOnUI(() =>
                        {
                            try
                            {
                                // apply the change(s) to the viewmodel
                                remote.Replay(MainViewModel.Instance);
                            }
                            catch(Exception ex)
                            {
                                Logger.LogMessage(Logger.Level.ERROR, "Journal.SyncCloudError", ex, "Exception thrown while replaying remote journal entry " + remote.ToString());
                            }
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(Logger.Level.ERROR, "Journal.SyncCloudEntry", ex);
            }
        }

        // Lock this lock if you're planning to mess with the journal file (reading it, copying it or anything)
        // so that while you're working on the file, the worker won't try to do anything with the file.
        private static LobsterLock JournalFileLock = new Model.LobsterLock();

        // The background worker that runs the journal save and sync process.  There should be only one.
        static LobsterWorker JournalWorker = null;

        /// <summary>
        /// Start the background worker that runs the journal save and sync process.
        /// </summary>
        /// <returns></returns>
        private static bool CreateAndStartJournalWorker()
        {
            try
            {
                // If the worker is already running try to kill it.
                if (Journal.JournalWorker != null)
                {
                    Logger.LogMessage(Logger.Level.ERROR,"Journal.CreateAndStartJournalWorker","there is already a worker running; attempting to stop it.");

                    try
                    {
                        Journal.JournalWorker.Cancel();

                        Journal.JournalWorker = null;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "Journal.CreateAndStartJournalWorker",ex,"exception while stopping the existing worker");
                    }
                }

                Journal.JournalWorker = new Model.LobsterWorker();
                Journal.JournalWorker.WorkerReportsProgress = false;
                Journal.JournalWorker.WorkerSupportsCancellation = false;
                Journal.JournalWorker.RunWorkerCompleted += (object sender, Model.RunWorkerCompletedEventArgs e) =>
                {
                    // Get the worker that raised this event.
                    Model.LobsterWorker w = sender as Model.LobsterWorker;

                    // First, handle the case where an exception was thrown.
                    if (e.Error != null)
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "Journal.CreateAndStartJournalWorker","worker exited with an exception: " + e.Error.Message);
                    }
                    else if (e.Cancelled)
                    {
                        // Note that due to a race condition in 
                        // the DoWork event handler, the Cancelled
                        // flag may not be set, even though
                        // CancelAsync was called.  In that case we
                        // presumably get the 'normal exit' case.
                        Logger.LogMessage(Logger.Level.ERROR, "Journal.CreateAndStartJournalWorker","worker was cancelled");

                        if (w == Journal.JournalWorker)
                            Journal.JournalWorker = null;
                    }
                    else
                    {
                        // Finally, handle the case where the operation 
                        // succeeded.
                        Logger.LogMessage(Logger.Level.INFO, "Journal.CreateAndStartJournalWorker", "worker exited normally");

                        if (w == Journal.JournalWorker)
                            Journal.JournalWorker = null;
                    }
                };

                // Once we tell the worker to start, we expect it to carry on forever (the DoWork handler runs
                // an infinite loop, only exiting if there is a horrible error). It cakks
                // Journal.DoJournalWork every 1,000ms, with a succesively increasing 'iteration number' value.
                Journal.JournalWorker.DoWork += (object sender, Model.DoWorkEventArgs e) =>
                {
                    try
                    {
                        // Get the worker that raised this event.
                        Model.LobsterWorker w = sender as Model.LobsterWorker;

                        int milliseconds = 1000;
                        int i = 1;
                        while (true)
                        {
                            try
                            {
                                if (w == Journal.JournalWorker)
                                {
                                    Journal.DoJournalWork(w, i);
                                }
                                else if (Journal.JournalWorker == null)
                                {
                                    Logger.LogMessage(Logger.Level.INFO, "Journal.CreateAndStartJournalWorker","this worker is still running, but JournalWorker is now set to NULL; not doing anything.");
                                }
                                else
                                {
                                    Logger.LogMessage(Logger.Level.INFO, "Journal.CreateAndStartJournalWorker", "this worker is still running, but JournalWorker is now pointing at a different worked.  Exiting.");
                                    e.Result = false;
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogMessage(Logger.Level.ERROR, "Journal.CreateAndStartJournalWorker",ex, "Exception in DoWork handler");
                            }

                            new System.Threading.ManualResetEvent(false).WaitOne(milliseconds);

                            i++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "Journal.CreateAndStartJournalWorker", ex, "Exception in DoWork handler");
                        e.Result = false;
                    }
                };

                Journal.JournalWorker.RunWorkerAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(Logger.Level.ERROR, "Journal.CreateAndStartJournalWorker", ex, "Exception while setting up worker");
                return false;
            }
        }

        /// <summary>
        /// Call this method to start the journal save and sync worker thread safely - if the thread is running
        /// already, the method will do nothing, otherwise it will start the worker thread.  If you call it twice in
        /// ve3ry quick succession, you will get a race condition, so don't do that.
        /// </summary>
        /// <returns></returns>
        public static bool EnsureJournalWorkerRunning()
        {
            if (Journal.JournalWorker == null)
            {
                return Journal.CreateAndStartJournalWorker();
            }
            else if (!Journal.JournalWorker.IsBusy)
            {
                Logger.LogMessage(Logger.Level.ERROR, "Journal.EnsureJournalWorkerRunning", "JournalWorker is non-null, but worker is not busy.");
                return Journal.CreateAndStartJournalWorker();
            }

            return true;
        }
    }
}
