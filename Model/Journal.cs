using System;
using System.Collections.Generic;
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
    ///         SITSMINIMUM: the lowest number of people who are being sought to play the game at this session
    ///         SITSMAXIMUM: the highest number of people who are being sought to play the game at this session
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
            }
        }

        public enum EntityType
        {
            Game, Person, Session, SignUp, GamingEvent
        }

        public enum OperationType
        {
            Create, Update, Delete
        }
    }
}
