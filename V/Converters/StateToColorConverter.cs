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
