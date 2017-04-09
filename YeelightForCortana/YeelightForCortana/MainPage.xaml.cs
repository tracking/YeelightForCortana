using ConfigStorage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using YeelightAPI;
using YeelightForCortana.ViewModel;

namespace YeelightForCortana
{
    /// <summary>
    /// 主页面
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // 配置存储对象
        private IConfigStorage configStorage;
        // ViewModel
        private MainPageViewModel viewModel;
        // 设备搜索中
        private bool deviceSearching;

        public MainPage()
        {
            this.InitializeComponent();
            this.Init();
        }

        private async void Init()
        {
            // 样式初始化
            this.PageStyleInit();
            // 滚动条开始滚动
            this.SetLoading(true);
            // 数据初始化
            await this.DataInit();
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
        /// 数据初始化
        /// </summary>
        /// <returns></returns>
        private async Task DataInit()
        {
            // 实例化viewModel
            viewModel = new MainPageViewModel();
            // 绑定数据上下文
            DataContext = viewModel;
            // 显示默认面板
            viewModel.ShowVoiceCommandSetGrid = true;
            // 添加默认分组
            viewModel.DeviceGroupList.Add(new DeviceGroup() { Id = "0", Name = "全部" });

            // 实例化配置存储对象
            configStorage = new JsonConfigStorage();
            // 加载配置
            await configStorage.LoadAsync();

            // 初始化设备
            var devices = configStorage.GetDevices();

            foreach (var item in devices)
            {
                viewModel.DeviceList.Add(new Device()
                {
                    Id = item.Id,
                    Name = item.Name
                });
            }

            // 初始化分组
            var groups = configStorage.GetGroups();

            foreach (var item in groups)
            {
                viewModel.DeviceGroupList.Add(new DeviceGroup()
                {
                    Id = item.Id,
                    Name = item.Name,
                    DeviceList = item.Devices.ToList()
                });
            }

            SetLoading(false);
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
        /// <summary>
        /// 设置加载中状态
        /// </summary>
        /// <param name="IsLoading">是否加载中</param>
        private void SetLoading(bool IsLoading)
        {
            if (IsLoading)
            {
                TopProgressStoryboard.Begin();
                TopProgressStoryboard.RepeatBehavior = RepeatBehavior.Forever;
            }
            else
            {
                TopProgressStoryboard.RepeatBehavior = new RepeatBehavior(1);
            }
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
        // 分组编辑面板关闭中
        private void SV_DeviceGroupConfig_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            if (string.IsNullOrEmpty(TXT_AddDeviceGroupName.Text))
            {
                args.Cancel = true;
            }

            // 添加分组
            var group = new ConfigStorage.Entiry.Group() { Id = Guid.NewGuid().ToString(), Name = TXT_AddDeviceGroupName.Text };
            configStorage.AddGroup(group);
            viewModel.DeviceGroupList.Add(new DeviceGroup() { Id = group.Id, Name = group.Name });
            configStorage.SaveAsync();
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

        // 查找设备按钮按下
        private async void BTN_SearchDevice_Click(object sender, RoutedEventArgs e)
        {
            // 显示设备管理面板
            viewModel.ShowDeviceGrid = true;
            // 显示新设备面板
            viewModel.ShowNewDeviceGrid = true;

            // 正在搜索中
            if (deviceSearching)
            {
                return;
            }

            // 设置当前正在搜索中状态
            deviceSearching = true;
            // 清空
            viewModel.NewDeviceList = new DeviceList();

            // 查找设备
            List<Yeelight> yeelights = (List<Yeelight>)await YeelightUtils.SearchDeviceAsync(5000);

            foreach (var item in yeelights)
            {
                // 不存在
                if (!configStorage.HasDevice(item.Id))
                {
                    var device = new ConfigStorage.Entiry.Device() { Id = item.Id, Name = item.Id, IP = item.Ip };
                    configStorage.AddDevice(device);
                    viewModel.NewDeviceList.Add(new Device() { Id = device.Id, Name = device.Name, Online = true });
                }
            }

            // 设置搜索完成状态
            deviceSearching = false;
        }
    }
}
