using System;
using Windows.UI.Xaml.Data;

namespace UWP_Demo.Converters
{
    public class FirstCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string text = value as string;
            return string.IsNullOrEmpty(text) ? "?" : text.Substring(0, 1).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}