using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using YeelightAPI;

namespace YeelightForCortana
{
    /// <summary>
    /// Yeelight值转换器
    /// </summary>
    public class YeelightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Yeelight yeelightItem = (Yeelight)value;
            string paramName = (string)parameter;

            // 空数据
            if (yeelightItem == null)
            {
                return "";
            }

            // 根据参数取值
            switch (paramName)
            {
                case "Power":
                    return yeelightItem.Power.ToString();
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
