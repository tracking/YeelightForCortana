using ColorMine.ColorSpaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using YeelightAPI;

namespace YeelightForCortana
{
    public class YeelightFlipViewItem : INotifyPropertyChanged
    {
        private Yeelight yeelightItem;

        /// <summary>
        /// 属性值变更事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性值变更事件
        /// </summary>
        /// <param name="propertyName">属性名</param>
        private void onPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="y">Yeelight对象</param>
        public YeelightFlipViewItem(Yeelight y)
        {
            this.yeelightItem = y;
        }

        /// <summary>
        /// 名字/标题
        /// </summary>
        public string Name
        {
            get
            {
                return !string.IsNullOrEmpty(this.yeelightItem.Name) ? this.yeelightItem.Name : this.yeelightItem.Id;
            }
        }
        /// <summary>
        /// 电源状态
        /// </summary>
        public string Power
        {
            get
            {
                return this.yeelightItem.Power.ToString();
            }
        }
        /// <summary>
        /// 背景颜色
        /// </summary>
        public SolidColorBrush BackgroundColor
        {
            get
            {
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
            }
        }

        /// <summary>
        /// 开/关灯
        /// </summary>
        /// <returns></returns>
        public async Task ToggleAsync()
        {
            await this.yeelightItem.ToggleAsync();
            await this.UpdateDataAsync();
        }

        private async Task UpdateDataAsync()
        {
            // 更新设备信息
            List<string> updateParamList = (List<string>)await this.yeelightItem.UpdateDeviceInfo();

            foreach (var item in updateParamList)
            {
                switch (item)
                {
                    case "power":
                        onPropertyChanged("Power");
                        break;
                    case "ct":
                    case "color_mode":
                    case "bright":
                    case "rgb":
                    case "hue":
                    case "sat":
                        onPropertyChanged("BackgroundColor");
                        break;
                    case "name":
                        onPropertyChanged("Name");
                        break;
                }
            }
        }
    }
}
