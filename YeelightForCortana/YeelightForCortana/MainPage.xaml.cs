using ColorMine.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YeelightAPI;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace YeelightForCortana
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // 是否正在刷新
        private bool deviceIsRefreshing = false;
        // HSV更新时钟
        private Timer updateHsvTimer;

        public MainPage()
        {
            this.InitializeComponent();
        }

        // 搜索设备按钮按下
        private async void abSearchDevice_Click(object sender, RoutedEventArgs e)
        {
            // 设置为正在刷新
            this.deviceIsRefreshing = true;

            // 禁用按钮 显示进度
            abSearchDevice.IsEnabled = false;
            prSearchDevice.IsActive = true;

            // 清空列表
            fvDevice.ItemsSource = null;

            // 获取设备列表
            var itemSource = new ObservableCollection<YeelightFlipViewItem>();
            foreach (var item in await YeelightUtils.SearchDeviceAsync())
                itemSource.Add(new YeelightFlipViewItem(item));
            fvDevice.ItemsSource = itemSource;
            fvDevice.Focus(FocusState.Pointer);

            // 启用按钮 隐藏进度
            abSearchDevice.IsEnabled = true;
            prSearchDevice.IsActive = false;

            // 刷新完成
            await Task.Delay(200);
            this.deviceIsRefreshing = false;
        }

        // 修改设备名字按钮按下
        private async void Button_SetDeviceName_Click(object sender, RoutedEventArgs e)
        {
            // 设置数据上下文
            cdSetDeviceName.DataContext = fvDevice.SelectedItem;

            // 显示
            await cdSetDeviceName.ShowAsync();
        }
        // 修改设备对话框 主按钮按下
        private async void cdSetDeviceName_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // 长度
            if (!string.IsNullOrEmpty(txtDeviceName.Text) && Encoding.UTF8.GetByteCount(txtDeviceName.Text) <= 20)
            {
                YeelightFlipViewItem item = (YeelightFlipViewItem)fvDevice.SelectedItem;
                await item.SetDeviceName(txtDeviceName.Text);
            }

            cdSetDeviceName.Hide();
        }
        // 修改设备对话框 副按钮按下
        private void cdSetDeviceName_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // 隐藏
            cdSetDeviceName.Hide();
        }
        // 电源开关值变更
        private async void tsPower_Toggled(object sender, RoutedEventArgs e)
        {
            // 刷新中不处理
            if (this.deviceIsRefreshing)
                return;

            YeelightFlipViewItem item = (YeelightFlipViewItem)fvDevice.SelectedItem;
            var _this = (ToggleSwitch)sender;

            _this.IsEnabled = false;
            await item.ToggleAsync();
            _this.IsEnabled = true;
        }
        // 色相、饱和度、亮度滑动条值变更
        private void SliderHSV_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // 刷新中不处理
            if (this.deviceIsRefreshing)
                return;

            // 如果为空则初始化
            if (this.updateHsvTimer == null)
                this.updateHsvTimer = new Timer(UpdateHsvTimer_CallBack, null, 200, Timeout.Infinite);

            // 重新倒计时
            this.updateHsvTimer.Change(200, Timeout.Infinite);
        }
        // HSV更新时钟回调
        private async void UpdateHsvTimer_CallBack(object state)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
             {
                 YeelightFlipViewItem item = (YeelightFlipViewItem)fvDevice.SelectedItem;

                 // 支持HSV则设置HSV 不支持只设置亮度（HSV中包含亮度）
                 if (item.HueSliderIsEnabled)
                     await item.SetHSV();
                 else
                     await item.SetBright();
             });
        }

        private void lvDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //// 显示按钮
            //btnToggle.Visibility = Visibility.Visible;
        }

        private async void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            //// 未选中
            //if (lvDeviceList.SelectedItem == null)
            //{
            //    return;
            //}

            //// 禁用按钮
            //btnToggle.IsEnabled = false;

            //Yeelight yeelightItem = (Yeelight)lvDeviceList.SelectedItem;
            //await yeelightItem.ToggleAsync();
            //lvDeviceList.SelectedItem = yeelightItem;
            //// 启用按钮
            //btnToggle.IsEnabled = true;
        }

        private async void btnCortanaSetting_Click(object sender, RoutedEventArgs e)
        {
            // 安装语音命令文件
            Windows.Storage.StorageFile vcdStorageFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"YeelightVoiceCommands.xml");
            await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            //Yeelight yeelightItem = (Yeelight)lvDeviceList.SelectedItem;
            //await yeelightItem.DebugAction();
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            //int h = Convert.ToInt32(textBox_r.Text);
            //int s = Convert.ToInt32(textBox_g.Text);
            //byte b = Convert.ToByte(textBox_b.Text);
            //;
            //var hsv = new Hsb { H = h, S = (double)s / 100, B = 1 };
            //var rgb = hsv.To<Rgb>();

            //grid1.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)rgb.R, (byte)rgb.G, (byte)rgb.B));
            //Hsb
            //Yeelight yeelightItem = (Yeelight)lvDeviceList.SelectedItem;
            //await yeelightItem.DebugAction(h, s);
            //RgbToHueEffect
            //Yeelight yeelightItem = (Yeelight)lvDeviceList.SelectedItem;
            //await yeelightItem.DebugAction(textBox.Text);

        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //YeelightFlipViewItem item = (YeelightFlipViewItem)flipView.SelectedItem;
            //await item.ToggleAsync();
            //flipView.Items[flipView.SelectedIndex] = flipView.Items[flipView.SelectedIndex];
            //await yeelightItem.SetColorTemperatureAsync(6500);
        }
    }
}
