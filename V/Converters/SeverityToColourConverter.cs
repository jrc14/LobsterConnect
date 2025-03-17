namespace LobsterConnect.V
{
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