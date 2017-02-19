using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace YeelightForCortana.Converter
{
    /// <summary>
    /// bool转Visibility
    /// </summary>
    public class BooleanConvertToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                return (Visibility)value == Visibility.Visible ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
