using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YeelightForCortana.ViewModel;

namespace YeelightForCortana
{
    /// <summary>
    /// 主页面
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.PageStyleInit();
            this.PageSampleDataInit();
        }

        /// <summary>
        /// 页面示例数据初始化
        /// </summary>
        private void PageSampleDataInit()
        {
            this.DataContext = new MainPageViewModel()
            {
                // 左区分组列表
                DeviceGroupList = new DeviceGroupList()
                {
                    new DeviceGroup() { Id = "0", Name="全部"},
                    new DeviceGroup() { Id = "1", Name="客厅"},
                    new DeviceGroup() { Id = "2", Name="厕所"},
                    new DeviceGroup() { Id = "3", Name="走廊"},
                    new DeviceGroup() { Id = "4", Name="书房"},
                    new DeviceGroup() { Id = "5", Name="卧室"}
                },
                // 中区操作类型
                CommandTypeList = new CommandTypeList()
                {
                    new CommandType() {Id = 0, Name="全部" },
                    new CommandType() {Id = 1, Name="开灯" },
                    new CommandType() {Id = 2, Name="关灯" },
                    new CommandType() {Id = 3, Name="切换颜色" },
                    new CommandType() {Id = 4, Name="调整亮度" }
                }
            };
        }

        /// <summary>
        /// 页面样式初始化
        /// </summary>
        private void PageStyleInit()
        {
            // 设置大小
            ApplicationView.PreferredLaunchViewSize = new Size(1000, 640);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            // 设置标题栏颜色
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = Colors.Black;
            titleBar.ForegroundColor = titleBar.ButtonForegroundColor = Colors.White;
        }

        // 设备组列表鼠标点击事件
        private void LB_DeviceGroupList_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var position = e.GetCurrentPoint(sender as UIElement);

            // 如果是鼠标右键点击
            if (position.Properties.IsRightButtonPressed)
            {
                // 显示右键菜单
                this.MF_DeviceGroupMenu.ShowAt(this.LB_DeviceGroupList, position.Position);
            }
        }
    }
}
