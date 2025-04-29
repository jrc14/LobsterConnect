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
    /// For message strings included in the user messages log at the foot of the main page - it colours ERROR
    /// messages in red, ALERT (WARNING) messages in orange-red, and other messages in light gray.
    /// </summary>
    public sealed class SeverityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo ci)
        {
            if (value is string)
            {
                if ((value as string).Contains("ERROR:"))
                    return Colors.Red;
                else if ((value as string).Contains("WARNING:"))
                    return Colors.OrangeRed;
                else if ((value as string).Contains("ALERT:"))
                    return Colors.AliceBlue;
                else
                    return Colors.LightGray;

            }
            else
                return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo ci)
        {
            return "???";
        }
    }
}