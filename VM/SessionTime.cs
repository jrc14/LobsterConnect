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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.VM
{
    /// <summary>
    /// A session time - i.e. a time slot in which a gaming session can start.  Internally it is
    /// just an integer, 0 for the first time slot in a gaming event, going up to
    /// N-1 where N is the number of valid time slots for that gaming event (held in the static
    /// member variable NumberOfTimeSlots and adjusted as necessary when the viewmodel calls
    /// SessionTime.SetEventType, typically in response to changes to which gaming
    /// events is the current event).
    /// 
    /// </summary>
    public class SessionTime : IComparable
    {
        public SessionTime(int timeSlotNumber=0)
        {
            this._timeSlotNumber = timeSlotNumber;
        }

        /// <summary>
        /// Turn self into a string.  Care: the result will depend on the labels set up
        /// by the most recent call to SessionTime.SetUpLabelsAndTimeSlots perhaps via
        /// a call to SessionTime.SetEventType.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.DayLabel))
                return this.TimeLabel;
            else
                return this.DayLabel + " " + this.TimeLabel;
        }

        /// Gets the 'day' part of the label for self.  Care: the result will depend on the labels set up
        /// by the most recent call to SessionTime.SetUpLabelsAndTimeSlots
        public string DayLabel
        {
            get
            {
                if (_timeSlotNumber < 0 || _timeSlotNumber >= _DayLabels.Length)
                    return "ERR";
                else
                    return _DayLabels[_timeSlotNumber];
            }
        }

        /// Gets the 'time' part of the label for self.  Care: the result will depend on the labels set up
        /// by the most recent call to SessionTime.SetUpLabelsAndTimeSlots
        public string TimeLabel
        {
            get
            {
                if (_timeSlotNumber < 0 || _timeSlotNumber >= _TimeLabels.Length)
                    return "ERROR";
                else
                    return _TimeLabels[_timeSlotNumber];
            }
        }

        /// <summary>
        /// The ordinal number of this timeslot, in the range 0 to SessionTime.NumberOfTimeSlots; it represents
        /// where this timeslot appears in the range of possible timeslots.
        /// </summary>
        public int Ordinal
        {
            get
            {
                return _timeSlotNumber;
            }
        }

        private readonly int _timeSlotNumber;

        // Override equality operators to give value semantics based on _timeSlotNumber
        public static bool operator ==(SessionTime b1, SessionTime b2)
        {
            if ((object)b1 == null)
                return (object)b2 == null;

            return b1.Equals(b2);
        }

        public static bool operator !=(SessionTime b1, SessionTime b2)
        {
            return !(b1 == b2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var b2 = (SessionTime)obj;
            return (_timeSlotNumber == b2._timeSlotNumber);
        }

        public override int GetHashCode()
        {
            return _timeSlotNumber.GetHashCode();
        }

        // override comparison operators to give sort by value semantics based on _timeSlotNumber
        public static bool operator >(SessionTime b1, SessionTime b2)
        {
            return b1._timeSlotNumber > b2._timeSlotNumber;
        }

        public static bool operator <(SessionTime b1, SessionTime b2)
        {
            return b1._timeSlotNumber < b2._timeSlotNumber;
        }

        public static bool operator >=(SessionTime b1, SessionTime b2)
        {
            return b1._timeSlotNumber >= b2._timeSlotNumber;
        }

        public static bool operator <=(SessionTime b1, SessionTime b2)
        {
            return b1._timeSlotNumber <= b2._timeSlotNumber;
        }

        public int CompareTo(object that)
        {
            if (that == null)
                throw new ArgumentException("SessionTime.CompareTo: can't compare with null");

            if (that as SessionTime == null)
                throw new ArgumentException("SessionTime.CompareTo: can't compare with a different type");

            return this._timeSlotNumber.CompareTo((that as SessionTime)._timeSlotNumber);
        }

        /// <summary>
        /// Change the event type that will be used for turning instances of SessionTime into
        /// string descriptions.  Note that changing the event type doesn't actually change
        /// the value of the SessionTime (it's always just an int) but it does change the
        /// string representation.
        /// </summary>
        /// <param name="eventType"></param>
        public static void SetEventType(string eventType)
        {
            SetUpLabelsAndTimeSlots(eventType);
        }

        /// <summary>
        /// Change the event date (year, month and day) that will be used for trying to find the 'current' session time
        /// (i.e. the timeslot that DateTime.Now falls within).  This is useful, once an event is ongoing,
        /// for scrolling the session table/list so that is shows the current time.
        /// Values of 0 mean that the event date can't be determined (for instance because event name
        /// didn't contain a YYYY-MM-DD string).
        /// </summary>
        /// <param name="year">year number (such as 2025, or 0)</param>
        /// <param name="month">month number (such as 8, or 0)</param>
        /// <param name="day">day number (such as 18, or 0)</param>
        public static void SetEventDate(int year, int month, int day)
        {
            _EventDateYYYY = year;
            _EventDateMM = month;
            _EventDateDD = day;
        }
        private static int _EventDateYYYY = 0;
        private static int _EventDateMM = 0;
        private static int _EventDateDD = 0;

        /// <summary>
        /// The session time corresponding to now (if DateTime.Now is before the first sesson time for the current
        /// event, you'll get the first session time of the event, if DateTime.Now is after the last session time for
        /// the current event you'll get the last session time of the event, if we couldn't determine the event date
        /// you'll get the first session time for the current event.
        /// </summary>
        public static SessionTime Current
        {
            get
            {
                if(_NumberOfTimeSlots==0)
                {
                    return new SessionTime(0);
                }
                if(_EventDateYYYY==0 || _EventDateMM==0 || _EventDateDD == 0)
                {
                    return new SessionTime(0);
                }
                try
                {
                    // midnight preceding the start of the event
                    DateTime eventStart = new DateTime(_EventDateYYYY, _EventDateMM, _EventDateDD, 0, 0, 0);
                    DateTime now = DateTime.Now;

                    for(int s = _NumberOfTimeSlots-1; s>=0; s--)
                    {
                        DateTime slotStart = eventStart.AddHours(_HourOffsets[s]);
                        if (now > slotStart)
                            return new SessionTime(s);
                    }
                    return new SessionTime(0);
                }
                catch (Exception)
                {
                    return new SessionTime(0);
                }
            }
        }

        public static int NumberOfTimeSlots
        {
            get
            {
                return _NumberOfTimeSlots;
            }
        }
        private static int _NumberOfTimeSlots = 0;

        private static void SetUpLabelsAndTimeSlots(string eventType)
        {
            if (eventType == "CONVENTION")
            {
                _NumberOfTimeSlots = 13 + 16 + 16 + 4; // 13 slots on Fri, 16 slots on Sat, 16 slots on Sun, 4 slots on Mon
                _DayLabels = new string[_NumberOfTimeSlots];
                _TimeLabels = new string[_NumberOfTimeSlots];
                _HourOffsets = new int[_NumberOfTimeSlots];
                int l = 0;

                for (int h = 12; h <= 24; h++) // Friday's hours
                {
                    _DayLabels[l] = "Fri";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);
                    _HourOffsets[l] = h;

                    l++;
                }
                Debug.Assert(l == 13);

                for (int h = 9; h <= 24; h++) // Saturday's hours
                {
                    _DayLabels[l] = "Sat";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);
                    _HourOffsets[l] = 24+h;

                    l++;
                }
                Debug.Assert(l == 13 + 16);

                for (int h = 9; h <= 24; h++) // Sunday's hours
                {
                    _DayLabels[l] = "Sun";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);
                    _HourOffsets[l] = 48 + h;

                    l++;
                }
                Debug.Assert(l == 13 + 16 + 16);

                for (int h = 9; h <= 12; h++) // Monday's hours
                {
                    _DayLabels[l] = "Mon";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);
                    _HourOffsets[l] = 72 + h;

                    l++;
                }
                Debug.Assert(l == 13 + 16 + 16 + 4);
            }
            else if (eventType == "EVENING")
            {
                _NumberOfTimeSlots = 3;
                _DayLabels = new string[3];
                _TimeLabels = new string[3];
                _HourOffsets = new int[3];

                int l = 0;


                for (int h = 18; h <= 20; h++) //s
                {
                    _DayLabels[l] = "";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);
                    _HourOffsets[l] = h;

                    l++;
                }
                Debug.Assert(l == 3);
            }
            else if (eventType == "DAY")
            {
                _NumberOfTimeSlots = 12;
                _DayLabels = new string[12];
                _TimeLabels = new string[12];
                _HourOffsets = new int[12];

                int l = 0;

                for (int h = 9; h <= 20; h++) //s
                {
                    _DayLabels[l] = "";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);
                    _HourOffsets[l] = h;
                    l++;
                }
                Debug.Assert(l == 12);

            }
        }

        private static string[] _DayLabels = null;
        private static string[] _TimeLabels = null;
        private static int[] _HourOffsets = null;

    }
}
