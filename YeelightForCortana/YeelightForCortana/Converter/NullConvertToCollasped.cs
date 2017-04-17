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
    /// null转Collapsed
    /// </summary>
    public class NullConvertToCollasped : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                return value != null ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("不支持反向转换");
        }
    }
}
