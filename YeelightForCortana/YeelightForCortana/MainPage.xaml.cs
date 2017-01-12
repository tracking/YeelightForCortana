using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 搜索设备按钮按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSearchDevice_Click(object sender, RoutedEventArgs e)
        {
            // 禁用按钮
            btnSearchDevice.IsEnabled = false;
            // 清空列表
            lvDeviceList.ItemsSource = null;

            // 获取设备列表
            lvDeviceList.ItemsSource = await YeelightUtils.SearchDeviceAsync();

            // 启用按钮
            btnSearchDevice.IsEnabled = true;
        }

        private void lvDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 显示按钮
            btnToggle.Visibility = Visibility.Visible;
        }

        private async void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            // 未选中
            if (lvDeviceList.SelectedItem == null)
            {
                return;
            }

            // 禁用按钮
            btnToggle.IsEnabled = false;

            Yeelight yeelightItem = (Yeelight)lvDeviceList.SelectedItem;
            await yeelightItem.ToggleAsync();

            // 启用按钮
            btnToggle.IsEnabled = true;
        }
    }
}
