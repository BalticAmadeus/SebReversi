using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Game.DebugClient.Converters
{
    public class CellStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string code = null;
            if (value is int)
                code = value.ToString();

            var s = value as string;
            if (s != null)
                code = s;

            switch (code)
            {
                case "*":
                    return ((SolidColorBrush)new BrushConverter().ConvertFromString("#75af66"));
                case "0":
                    return new SolidColorBrush(Colors.Black);
                case "1":
                    return new SolidColorBrush(Colors.White);
                case "!":
                    return new SolidColorBrush(Colors.Red);
                default:
                    throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}