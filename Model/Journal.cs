using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.Model
{
    /// <summary>
    /// The journal of changes to the LobsterConnect data store.  By replaying a journal, you bring the datastore up to date.
    /// The types of entity and the available types of changes are:
    /// 
    /// Game (Create and Update only): a game that can be played at a session
    ///     ID is the game's full name
    ///     Attributes are:
    ///         ISACTIVE: True or False; if false then the game is not available for sign-up
    ///         BGGLINK: the URL for the game on the BGG site
    ///         
    /// Person (Create and Update only): a person who can sign in to the app, and organise and join gaming sessions
    ///     ID is the person's handle
    ///     Attributes are:
    ///         FULLNAME: the full name of the person
    ///         PHONENUMBER: the person's phone number
    ///         EMAIL: the person's email address
    ///         PASSWORD: the hash of the person's password         
    ///         ISADMIN: True or False; if true then the UI will allow this person change things belonging to other people
    ///         ISACTIVE: True or False; if false then the person is not allowed to organise or participate in games
    ///         
    /// Session (Create and Update only): a session to play a certain game, at a certain event, at a certain time
    ///     ID is an opaque ID (normally a GUID).
    ///     Journal entries for sessions must always include EVENTNAME, the name of the gaming event at which the session happens.
    ///     This value cannot be updated after a session has been created.
    ///     Attributes are:
    ///         EVENTNAME (Mandatory in all journal entries, and immutable): the name of the event at which the session is happening
    ///         PROPOSER: the handle of the person who's organising the session
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
    ///     ID consists of the event's name
    ///     Attributes are:
    ///         EVENTTYPE: EVENING, DAY, CONVENTION; Application logic dictates how many session times are available for an event, according to its event type
    ///         ISACTIVE: True or False; if false then sessions can't be set up at this event
    ///     
    ///     
    /// NB: IDs and attribute values are all strings.  It is an error if any of these strings contains the characters '\' or '|'.
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
            
            JournalEntry e = new JournalEntry(0, _LocalJournalNextSeq++, Model.Utilities.InstallationId, entityType, operationType, entityId, journalParameters);

            _LocalJournal.Add(e);

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

        public class JournalEntry()
        {
            public JournalEntry(Int64 cloudSeq, Int64 localSeq, string installationId, EntityType e, OperationType o, string i, List<string> p) : this()
            {
                _cloudSeq = cloudSeq;
                _localSeq = localSeq;
                _installationId = installationId;

                _entityType = e;
                _operationType = o;
                _entityId = i;
                _parameters = p;

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

                for(int ii=1; ii<_parameters.Count; ii+=2)
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

                string s = cloudSeqString + "\\" + localSeqString + "\\" + _installationId + "\\" + entityTypeString + "\\"+ operationTypeString+"\\" + this._entityId + "\\";
                if(this._parameters.Count>0)
                {
                    string p = string.Join('|', this._parameters);
                    s += p;
                }
                return s;
            }
            

            Int64 _cloudSeq;
            Int64 _localSeq;
            string _installationId;

            private Journal.EntityType _entityType;
            private Journal.OperationType _operationType;
            string _entityId;
            List<string> _parameters;
        }



        /// <summary>
        /// Call this from the UI thread (or any other thread except the worker thread for BackgroundJournalWriter)
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
        /// The worker thread should hold this lock only when it needs to prevent another thread from making updates to the queue.
        /// </summary>
        private static LobsterLock QLock = new LobsterLock();

        public static bool DoingJournalWork = false;
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
                                        Logger.LogMessage(Logger.Level.ERROR, "Journal.DoJournalWork", ex, "while writing line to journal fule");
                                    }
                                }
                            }
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

                if (iteration % 60 == 1) // iteration = number of seconds - so this runs every minute.
                {
                    Logger.LogMessage(Logger.Level.INFO,"Journal.DoJournalWork","iteration number " + iteration.ToString() + ": journal sync code will run.");

                    Logger.LogMessage(Logger.Level.INFO, "Journal.DoJournalWork","iteration number " + iteration.ToString() + ": journal sync completed.");

                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(Logger.Level.ERROR,"FredaJournal.DoJournalWork", ex);
            }
            finally
            {
                DoingJournalWork = false;
            }

            return true;
        }

        // Lock this lock if you're planning to mess with the journal file (copying it or anything)
        // so that while you're working on the file, the worker won't try to do anything with the file.
        private static LobsterLock JournalFileLock = new Model.LobsterLock();

        static LobsterWorker JournalWorker = null;
        private static bool CreateAndStartJournalWorker()
        {
            try
            {
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
