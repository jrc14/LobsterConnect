using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.VM
{
    public class SessionTime : IComparable
    {
        public SessionTime(int timeSlotNumber=0)
        {
            this._timeSlotNumber = timeSlotNumber;
        }


        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.DayLabel))
                return this.TimeLabel;
            else
                return this.DayLabel + " " + this.TimeLabel;
        }

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

        public static void SetEventType(string eventType)
        {
            SetUpLabelsAndTimeSlots(eventType);
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
                int l = 0;

                for (int h = 12; h <= 24; h++) // Friday's hours
                {
                    _DayLabels[l] = "Fri";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);

                    l++;
                }
                Debug.Assert(l == 13);

                for (int h = 9; h <= 24; h++) // Saturday's hours
                {
                    _DayLabels[l] = "Sat";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);

                    l++;
                }
                Debug.Assert(l == 13 + 16);

                for (int h = 9; h <= 24; h++) // Sunday's hours
                {
                    _DayLabels[l] = "Sun";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);

                    l++;
                }
                Debug.Assert(l == 13 + 16 + 16);

                for (int h = 9; h <= 12; h++) // Monday's hours
                {
                    _DayLabels[l] = "Mon";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);

                    l++;
                }
                Debug.Assert(l == 13 + 16 + 16 + 4);
            }
            else if (eventType == "EVENING")
            {
                _NumberOfTimeSlots = 3;
                _DayLabels = new string[3];
                _TimeLabels = new string[3];
                int l = 0;


                for (int h = 18; h <= 20; h++) //s
                {
                    _DayLabels[l] = "";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);

                    l++;
                }
                Debug.Assert(l == 3);
            }
            else if (eventType == "DAY")
            {
                _NumberOfTimeSlots = 12;
                _DayLabels = new string[12];
                _TimeLabels = new string[12];
                int l = 0;


                for (int h = 9; h <= 20; h++) //s
                {
                    _DayLabels[l] = "";
                    _TimeLabels[l] = string.Format("{0:D2}h00", h);

                    l++;
                }
                Debug.Assert(l == 12);

            }
        }

        private static string[] _DayLabels = null;
        private static string[] _TimeLabels = null;

    }
}
