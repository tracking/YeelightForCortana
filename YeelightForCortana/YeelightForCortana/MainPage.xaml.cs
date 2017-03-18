using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                // 设备列表
                DeviceList = new DeviceList()
                {
                    new Device() { Id = 0, Name="全部"},
                    new Device() { Id = 1, Name="客厅"},
                    new Device() { Id = 2, Name="厕所"},
                    new Device() { Id = 3, Name="走廊"},
                    new Device() { Id = 4, Name="书房"},
                    new Device() { Id = 5, Name="卧室"}
                },
                // 左区分组列表
                DeviceGroupList = new DeviceGroupList()
                {
                    new DeviceGroup() { Id = 0, Name="全部"},
                    new DeviceGroup() { Id = 1, Name="客厅"},
                    new DeviceGroup() { Id = 2, Name="厕所"},
                    new DeviceGroup() { Id = 3, Name="走廊"},
                    new DeviceGroup() { Id = 4, Name="书房"},
                    new DeviceGroup() { Id = 5, Name="卧室"}
                },
                // 中区Split设备列表
                DeviceCheckList = new DeviceCheckList(new DeviceList() {
                    new Device() { Id = 0, Name="全部"},
                    new Device() { Id = 1, Name="客厅"},
                    new Device() { Id = 2, Name="厕所"},
                    new Device() { Id = 3, Name="走廊"},
                    new Device() { Id = 4, Name="书房"},
                    new Device() { Id = 5, Name="卧室"}
                }),
                // 中区操作类型
                CommandTypeList = new CommandTypeList()
                {
                    new CommandType() {Id = 0, Name="全部" },
                    new CommandType() {Id = 1, Name="开灯" },
                    new CommandType() {Id = 2, Name="关灯" },
                    new CommandType() {Id = 3, Name="切换颜色" },
                    new CommandType() {Id = 4, Name="增加亮度" },
                    new CommandType() {Id = 5, Name="减少亮度" }
                },
                VoiceCommandSetList = new VoiceCommandSetList()
                {
                    new VoiceCommandSet(new Device() { Id = 0,  Name="厨房灯"})
                    {
                        Id = 1,
                        CommandType = new CommandType() {Id = 1, Name="开灯" },
                        VoiceCommandList = new List<VoiceCommand>()
                        {
                            new VoiceCommand() {Id=1,Say="帮开厨房灯",Answer = "好的，正在帮你打开厨房灯" },
                            new VoiceCommand() {Id=1,Say="帮我打开厨房灯",Answer = "好的，正在帮你打开厨房灯" }
                        }
                    },
                    new VoiceCommandSet(new DeviceGroup() { Id = 1,  Name="客厅"})
                    {
                        Id = 1,
                        CommandType = new CommandType() {Id = 5, Name="减少亮度" },
                        VoiceCommandList = new List<VoiceCommand>()
                        {
                            new VoiceCommand() {Id=1,Say="帮我把客厅的灯亮度调低一点，太亮了受不了",Answer = "好的，正在帮你调低客厅灯的亮度" }
                        }
                    },
                    new VoiceCommandSet(new DeviceGroup() { Id = 2,  Name="卧室"})
                    {
                        Id = 1,
                        CommandType = new CommandType() {Id = 3, Name="切换颜色" },
                        VoiceCommandList = new List<VoiceCommand>()
                        {
                            new VoiceCommand() {Id=1,Say="神圣的光辉将净化污浊的大地，星路之门在此敞开",Answer = "妈的智障" }
                        }
                    }
                },
                SelectedVoiceCommandSet = new VoiceCommandSet(new Device() { Id = 0, Name = "厨房灯" })
                {
                    Id = 1,
                    CommandType = new CommandType() { Id = 1, Name = "开灯" },
                    VoiceCommandList = new List<VoiceCommand>()
                        {
                            new VoiceCommand() {Id=1,Say="帮开厨房灯",Answer = "好的，正在帮你打开厨房灯" },
                            new VoiceCommand() {Id=1,Say="帮我打开厨房灯",Answer = "好的，正在帮你打开厨房灯" }
                        }
                },
                ShowVoiceCommandSetGrid = false,
                ShowNewDeviceGrid = true
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
            titleBar.InactiveBackgroundColor = titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = Colors.Black;
            titleBar.InactiveForegroundColor = titleBar.ForegroundColor = titleBar.ButtonForegroundColor = Colors.White;

            // 设置边框颜色
        }
        /// <summary>
        /// 设置设备全选框状态
        /// </summary>
        /// <returns></returns>
        private void SetSelectAllDeviceCheckBoxState()
        {
            int checkedCount = 0;

            // 计算选中数量
            foreach (DeviceCheck item in LB_DeviceCheckList.Items)
                if (item.IsChecked == true)
                    checkedCount++;

            if (checkedCount == 0)
                CB_SelectAllDevice.IsChecked = false;
            else if (checkedCount == LB_DeviceCheckList.Items.Count)
                CB_SelectAllDevice.IsChecked = true;
            else
                CB_SelectAllDevice.IsChecked = null;
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
        // 添加设备分组按钮按下
        private void BTN_AddDeviceGroup_Click(object sender, RoutedEventArgs e)
        {
            // 打开
            SV_DeviceGroupConfig.IsPaneOpen = true;
        }
        // 设备全选框按下
        private void CB_SelectAllDevice_Click(object sender, RoutedEventArgs e)
        {
            // !!!进入此函数时CheckBox状态已改变!!!

            // 默认全不选
            bool isChecked = false;

            // 如果已经是全选或半选状态则全选
            if (CB_SelectAllDevice.IsChecked == null || CB_SelectAllDevice.IsChecked == true)
                isChecked = true;

            // 设置状态
            CB_SelectAllDevice.IsChecked = isChecked;
            foreach (DeviceCheck item in LB_DeviceCheckList.Items)
                item.IsChecked = isChecked;
        }
        // 设备全选框列表的全选框按下
        private void CB_DeviceListCheckBox_Click(object sender, RoutedEventArgs e)
        {
            // 设置设备全选框状态
            SetSelectAllDeviceCheckBoxState();
        }

    }
}
