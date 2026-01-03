using System.Globalization;
using Microsoft.Maui.Controls;

namespace mg3.foundry.Converters
{
    public class BoolToMessageStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isUser && isUser)
                return Application.Current.Resources["UserMessage"];
            return Application.Current.Resources["AssistantMessage"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}