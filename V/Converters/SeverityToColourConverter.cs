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
                else if ((value as string).Contains("ALERT:"))
                    return Colors.OrangeRed;
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