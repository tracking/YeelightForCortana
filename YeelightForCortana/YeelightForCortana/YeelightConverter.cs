using ColorMine.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
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
            string paramName = parameter.ToString();

            // 根据参数取值
            switch (paramName)
            {
                // 电源状态
                case "Power":
                    return yeelightItem.Power.ToString();
                // 颜色
                case "Color":
                    Hsv hsv = null;

                    // 亮度处理 不至于太暗看不清背景 1-100转到1-15
                    var bright = (yeelightItem.Bright * 0.15) + 85;

                    // 根据颜色模式处理
                    switch (yeelightItem.ColorMode)
                    {
                        // RGB
                        case YeelightColorMode.color:
                            var tempRgb = new Rgb() { R = yeelightItem.R, G = yeelightItem.G, B = yeelightItem.B };
                            // 转成HSV 此时亮度不确定
                            hsv = tempRgb.To<Hsv>();
                            // 加上亮度
                            hsv = new Hsv() { H = hsv.H, S = hsv.S, V = (double)bright / 100 };
                            break;
                        // HSV
                        case YeelightColorMode.hsv:
                            hsv = new Hsv() { H = yeelightItem.HUE, S = (double)yeelightItem.SAT / 100, V = (double)bright / 100 };
                            break;
                        // 色温
                        case YeelightColorMode.temperature:
                            // 固定色相和饱和度
                            hsv = new Hsv() { H = 40, S = 0.25, V = (double)bright / 100 };
                            break;
                    }

                    var rgb = hsv.To<Rgb>();
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)rgb.R, (byte)rgb.G, (byte)rgb.B));
                default:
                    return "";
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
