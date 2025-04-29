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

namespace LobsterConnect.V
{
    /// <summary>
    /// For labels showing the state of a game session object (on the table in the main page UI).  It colours OPEN
    /// in dark green, FULL in dark blue, ABANDONED in dark red, and anything else (there should not be anything
    /// else) in black.
    /// </summary>
    public sealed class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo ci)
        {
            if (value is string)
            {
                if (value as string == "OPEN")
                    return Colors.DarkGreen;
                else if (value as string == "FULL")
                    return Colors.DarkBlue;
                else if (value as string == "ABANDONED")
                    return Colors.DarkRed;
                else
                    return Colors.Black;

            }
            else
                return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo ci)
        {
            return "???";
        }
    }
}
