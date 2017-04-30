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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">是否反转</param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (parameter != null && parameter.ToString() == "Reversal")
                    return (bool)value ? Visibility.Collapsed : Visibility.Visible;

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
                if (parameter != null && parameter.ToString() == "Reversal")
                    return (Visibility)value == Visibility.Visible ? false : true;

                return (Visibility)value == Visibility.Visible ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
