using ColorMine.ColorSpaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using YeelightAPI;

namespace YeelightForCortana
{
    public class YeelightFlipViewItem : INotifyPropertyChanged
    {
        private Yeelight yeelightItem;
        private Hsv hsv;

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

            // 获取设备颜色
            this.GetDeviceColor();
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
        public bool Power
        {
            get
            {
                return this.yeelightItem.Power == YeelightPower.on ? true : false;
            }
        }
        /// <summary>
        /// 背景颜色
        /// </summary>
        public SolidColorBrush BackgroundColor
        {
            get
            {
                var rgb = this.hsv.To<Rgb>();
                return new SolidColorBrush(Windows.UI.Color.FromArgb(150, (byte)rgb.R, (byte)rgb.G, (byte)rgb.B));
            }
        }
        /// <summary>
        /// 色相
        /// </summary>
        public int Hue
        {
            get
            {
                return Convert.ToInt32(this.hsv.H);
            }
            set
            {
                this.hsv.H = value;

                // 通知背景颜色变更
                onPropertyChanged("BackgroundColor");
            }
        }
        /// <summary>
        /// 饱和度
        /// </summary>
        public int Sat
        {
            get
            {
                return Convert.ToInt32(this.hsv.S * 100);
            }
            set
            {
                this.hsv.S = (double)value / 100;

                // 通知背景颜色变更
                onPropertyChanged("BackgroundColor");
            }
        }
        /// <summary>
        /// 亮度
        /// </summary>
        public int Bright
        {
            get
            {
                return this.yeelightItem.Bright;
            }
            set
            {
                // 亮度处理 不至于太暗看不清背景 1-100转到1-50
                //var bright = (value * 0.5) + 50;

                var bright = value;

                this.hsv.V = (double)bright / 100;

                // 通知背景颜色变更
                onPropertyChanged("BackgroundColor");
            }
        }
        /// <summary>
        /// 色相滑动条是否启用
        /// </summary>
        public bool HueSliderIsEnabled
        {
            get
            {
                return this.yeelightItem.DeviceIsSupportProp("HUE");
            }
        }
        /// <summary>
        /// 饱和度滑动条是否启用
        /// </summary>
        public bool SatSliderIsEnabled
        {
            get
            {
                return this.yeelightItem.DeviceIsSupportProp("SAT");
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
        /// <summary>
        /// 设置HSV颜色
        /// </summary>
        /// <returns></returns>
        public async Task SetHSV()
        {
            await this.yeelightItem.SetHSV(Convert.ToInt32(this.hsv.H), Convert.ToInt32(this.hsv.S * 100));
            await this.yeelightItem.SetBright(Convert.ToInt32(this.hsv.V * 100));
            await this.UpdateDataAsync();
        }
        /// <summary>
        /// 设置亮度
        /// </summary>
        /// <returns></returns>
        public async Task SetBright()
        {
            await this.yeelightItem.SetBright(Convert.ToInt32(this.hsv.V * 100));
            await this.UpdateDataAsync();
        }
        /// <summary>
        /// 设置设备名字
        /// </summary>
        /// <param name="name">设备名字</param>
        /// <returns></returns>
        public async Task SetDeviceName(string name)
        {
            await this.yeelightItem.SetDeviceName(name);
            await this.UpdateDataAsync();
        }

        /// <summary>
        /// 更新设备数据
        /// </summary>
        /// <returns></returns>
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
                        // 获取设备颜色
                        this.GetDeviceColor();
                        onPropertyChanged("Hue");
                        onPropertyChanged("Sat");
                        onPropertyChanged("Bright");
                        onPropertyChanged("BackgroundColor");
                        break;
                    case "name":
                        onPropertyChanged("Name");
                        break;
                }
            }
        }
        /// <summary>
        /// 获取设备颜色
        /// </summary>
        /// <returns>HSV颜色</returns>
        private void GetDeviceColor()
        {
            Hsv hsv = null;

            // 亮度处理 不至于太暗看不清背景 1-100转到1-50
            //var bright = (this.yeelightItem.Bright * 0.5) + 50;
            var bright = this.yeelightItem.Bright;

            // 根据颜色模式处理
            switch (this.yeelightItem.ColorMode)
            {
                // RGB
                case YeelightColorMode.color:
                    var tempRgb = new Rgb() { R = this.yeelightItem.R, G = this.yeelightItem.G, B = this.yeelightItem.B };
                    // 转成HSV 此时亮度不确定
                    hsv = tempRgb.To<Hsv>();
                    // 加上亮度
                    hsv = new Hsv() { H = hsv.H, S = hsv.S, V = (double)bright / 100 };
                    break;
                // HSV
                case YeelightColorMode.hsv:
                    hsv = new Hsv() { H = this.yeelightItem.HUE, S = (double)this.yeelightItem.SAT / 100, V = (double)bright / 100 };
                    break;
                // 色温
                case YeelightColorMode.temperature:
                    // 固定色相和饱和度
                    hsv = new Hsv() { H = 40, S = 0.25, V = (double)bright / 100 };
                    break;
            }

            this.hsv = hsv;
        }
    }
}
