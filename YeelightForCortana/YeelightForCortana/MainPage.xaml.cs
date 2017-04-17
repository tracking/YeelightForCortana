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
using Windows.UI.Popups;
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
        // 设备状态刷新中
        private bool deviceStatusRefreshing;
        // 当前编辑的分组
        private DeviceGroup editDeviceGroup;

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
            // 添加默认操作类型
            viewModel.CommandTypeList.Add(new CommandType(ActionType.PowerOn));
            viewModel.CommandTypeList.Add(new CommandType(ActionType.PowerOff));
            viewModel.CommandTypeList.Add(new CommandType(ActionType.BrightUp));
            viewModel.CommandTypeList.Add(new CommandType(ActionType.BrightDown));
            viewModel.CommandTypeList.Add(new CommandType(ActionType.SwitchColor));
            viewModel.CommandTypeDisplayList.Add(new CommandType());   // 全部
            foreach (var item in viewModel.CommandTypeList)
                viewModel.CommandTypeDisplayList.Add(item);

            // 实例化配置存储对象
            configStorage = new JsonConfigStorage();
            // 加载配置
            await configStorage.LoadAsync();

            // 初始化设备
            var devices = configStorage.GetDevices();

            foreach (var item in devices)
            {
                viewModel.DeviceList.Add(new Device(item.RawDeviceInfo) { Name = item.Name });
            }

            // 刷新设备状态（无需等待）
            RefreshDevicesStatus();

            // 初始化分组
            var groups = configStorage.GetGroups();

            foreach (var item in groups)
            {
                // 组建设备列表
                var deviceList = new List<Device>();
                if (viewModel.DeviceList.Count > 0)
                {
                    foreach (var deviceId in item.Devices)
                    {
                        deviceList.AddRange(viewModel.DeviceList.Where(d => d.Id == deviceId));
                    }
                }
                // 生成
                viewModel.DeviceGroupList.Add(new DeviceGroup()
                {
                    Id = item.Id,
                    Name = item.Name,
                    DeviceList = deviceList
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
        /// 刷新选择对象下拉框
        /// </summary>
        private void RefreshSelectTargetCombobox()
        {
            // 清空菜单
            var flyoutBase = (MenuFlyout)FlyoutBase.GetAttachedFlyout(CBB_SelectTarget);
            flyoutBase.Items.Clear();

            // 创建菜单对象
            var singleDogDeviceList = new List<Device>(viewModel.DeviceList);
            foreach (var group in viewModel.DeviceGroupList)
            {
                // 全部
                if (group.Id == "0")
                {
                    var item = new MenuFlyoutItem() { Text = "全部", DataContext = null };
                    item.Click += CBB_SelectTarget_MenuItem_Click;
                    flyoutBase.Items.Add(item);
                    continue;
                }

                // 创建父菜单
                var menuSubItem = new MenuFlyoutSubItem() { DataContext = group };
                menuSubItem.SetBinding(MenuFlyoutSubItem.TextProperty, new Binding() { Path = new PropertyPath("Name") });
                menuSubItem.Tapped += CBB_SelectTarget_MenuSubItem_Tapped;

                foreach (var device in group.DeviceList)
                {
                    // 从单身列表中移除
                    singleDogDeviceList.Remove(device);
                    // 菜单项
                    var item = new MenuFlyoutItem() { DataContext = device };
                    item.SetBinding(MenuFlyoutItem.TextProperty, new Binding() { Path = new PropertyPath("Name") });
                    item.Click += CBB_SelectTarget_MenuItem_Click;
                    // 加入父菜单
                    menuSubItem.Items.Add(item);
                }

                // 添加菜单
                flyoutBase.Items.Add(menuSubItem);
            }
            foreach (var device in singleDogDeviceList)
            {
                // 菜单项
                var item = new MenuFlyoutItem() { DataContext = device };
                item.SetBinding(MenuFlyoutItem.TextProperty, new Binding() { Path = new PropertyPath("Name") });
                item.Click += CBB_SelectTarget_MenuItem_Click;
                flyoutBase.Items.Add(new MenuFlyoutItem() { Text = device.Name, DataContext = device });
            }
        }
        /// <summary>
        /// 刷新设备状态
        /// </summary>
        private async Task RefreshDeviceStatus(Device device)
        {
            try
            {
                // 更新设备信息
                await device.RawDevice.UpdateDeviceInfo();
                // 无异常表示在线
                device.Online = true;
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 刷新所有设备状态
        /// </summary>
        private async void RefreshDevicesStatus()
        {
            if (deviceStatusRefreshing)
            {
                return;
            }

            // 设置当前状态为刷新
            deviceStatusRefreshing = true;

            List<Task> taskList = new List<Task>();

            for (int i = 0; i < viewModel.DeviceList.Count; i++)
            {
                // 启动task并加入列表
                taskList.Add(RefreshDeviceStatus(viewModel.DeviceList[i]));
            }

            // 等待所有完成
            await Task.WhenAll(taskList.ToArray());
            // 改变状态
            deviceStatusRefreshing = false;
        }

        /// <summary>
        /// 设置加载中状态
        /// </summary>
        /// <param name="IsLoading">是否加载中</param>
        private void SetLoading(bool IsLoading)
        {
            if (IsLoading)
            {
                TopProgressStoryboard.Stop();
                TopProgressStoryboard.RepeatBehavior = RepeatBehavior.Forever;
                TopProgressStoryboard.Begin();
            }
            else
            {
                TopProgressStoryboard.RepeatBehavior = new RepeatBehavior(1);
            }
        }
        /// <summary>
        /// 删除焦点
        /// http://windowsapptutorials.com/tips/general-tips/how-to-make-textbox-lose-its-focus-in-windows-phone/
        /// </summary>
        /// <param name="sender"></param>
        private void LoseFocus(object sender)
        {
            var control = sender as Control;
            var isTabStop = control.IsTabStop;
            control.IsTabStop = false;
            control.IsEnabled = false;
            control.IsEnabled = true;
            control.IsTabStop = isTabStop;
        }
        /// <summary>
        /// 显示提示对话框
        /// </summary>
        /// <param name="msg">显示内容</param>
        /// <param name="title">标题</param>
        /// <param name="okLabel">确定文本</param>
        /// <returns></returns>
        private async Task<bool> ShowMessageDialog(string msg, string title = "提示", string okLabel = "确定")
        {
            var dialog = new CustomControl.ConfirmDialog(msg, title, okLabel, null);
            await dialog.ShowAsync();
            return dialog.Result;
        }
        /// <summary>
        /// 显示确认对话框
        /// </summary>
        /// <param name="msg">显示内容</param>
        /// <param name="title">标题</param>
        /// <param name="okLabel">确定文本</param>
        /// <param name="cancelLabel">取消文本</param>
        /// <returns></returns>
        private async Task<bool> ShowConfirmDialog(string msg, string title = "提示", string okLabel = "确定", string cancelLabel = "取消")
        {
            var dialog = new CustomControl.ConfirmDialog(msg, title, okLabel, cancelLabel);
            await dialog.ShowAsync();
            return dialog.Result;
        }

        // 设备组列表鼠标点击事件 弹出菜单
        private void LB_DeviceGroupListItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var senderElement = (FrameworkElement)sender;
            var flyoutBase = (MenuFlyout)FlyoutBase.GetAttachedFlyout(senderElement);
            var group = (DeviceGroup)senderElement.DataContext;

            // 全部分组不显示菜单
            if (group.Id == "0")
            {
                return;
            }

            flyoutBase.ShowAt(senderElement, e.GetPosition(senderElement));
        }
        // 删除设备分组菜单按下
        private async void MFI_DeviceGroupDelete_Click(object sender, RoutedEventArgs e)
        {
            var group = (DeviceGroup)((FrameworkElement)sender).DataContext;
            var isConfirm = await ShowConfirmDialog(string.Format("是否删除分组“{0}”", group.Name));

            if (isConfirm)
            {
                // 删除
                configStorage.DeleteGroup(group.Id);
                viewModel.DeviceGroupList.Remove(group);

                // 保存
                await configStorage.SaveAsync();
            }
        }
        // 编辑设备分组菜单按下
        private void MFI_DeviceGroupEdit_Click(object sender, RoutedEventArgs e)
        {
            var group = editDeviceGroup = (DeviceGroup)((FrameworkElement)sender).DataContext;

            // 准备设备列表
            viewModel.DeviceCheckList.Clear();
            viewModel.DeviceCheckList.AddRange(viewModel.DeviceList);

            for (int i = 0; i < group.DeviceList.Count; i++)
            {
                if (viewModel.DeviceCheckList.Count == 0)
                    break;

                var deviceCheck = viewModel.DeviceCheckList.First(item => item.Device.Id == group.DeviceList[i].Id);
                if (deviceCheck != null)
                {
                    deviceCheck.IsChecked = true;
                }
            }
            // 设置文本框内容
            TXT_AddDeviceGroupName.Text = group.Name;
            // 设置设备全选框状态
            SetSelectAllDeviceCheckBoxState();
            // 打开
            SV_DeviceGroupConfig.IsPaneOpen = true;
        }
        // 设备组列表项选中事件
        private void LB_DeviceGroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(e.AddedItems);
        }
        // 添加设备分组按钮按下
        private void BTN_AddDeviceGroup_Click(object sender, RoutedEventArgs e)
        {
            // 准备设备列表
            viewModel.DeviceCheckList.Clear();
            viewModel.DeviceCheckList.AddRange(viewModel.DeviceList);
            // 设置默认分组名
            TXT_AddDeviceGroupName.Text = "未命名";
            // 设置全选框状态
            SetSelectAllDeviceCheckBoxState();
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

            // 获取选中的设备
            var checkedDevices = viewModel.DeviceCheckList.Count == 0
                ? new List<DeviceCheck>()
                : viewModel.DeviceCheckList.Where(item => item.IsChecked).ToList();
            var checkedDeviceList = new List<string>();
            var deviceList = new List<Device>();

            // 加入列表
            for (int i = 0; i < checkedDevices.Count; i++)
            {
                checkedDeviceList.Add(checkedDevices[i].Device.Id);
                deviceList.Add(checkedDevices[i].Device);
            }

            // 新增
            if (editDeviceGroup == null)
            {
                // 添加分组
                var group = new ConfigStorage.Entiry.Group() { Id = Guid.NewGuid().ToString(), Name = TXT_AddDeviceGroupName.Text, Devices = checkedDeviceList };
                configStorage.AddGroup(group);
                viewModel.DeviceGroupList.Add(new DeviceGroup() { Id = group.Id, Name = group.Name, DeviceList = deviceList });
            }
            // 修改
            else
            {
                // 获取分组
                var group = configStorage.GetGroup(editDeviceGroup.Id);
                // 修改分组
                editDeviceGroup.Name = group.Name = TXT_AddDeviceGroupName.Text;
                editDeviceGroup.DeviceList = deviceList;
                group.Devices = checkedDeviceList;
            }

            // 清除
            editDeviceGroup = null;
            // 保存
            configStorage.SaveAsync();
        }
        // 设备全选选择框框按下
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
        // 设备列表的选择框按下
        private void CB_DeviceListCheckBox_Click(object sender, RoutedEventArgs e)
        {
            // 设置设备全选框状态
            SetSelectAllDeviceCheckBoxState();
        }

        // 查找设备按钮按下
        private async void BTN_SearchDevice_Click(object sender, RoutedEventArgs e)
        {
            // 刷新设备状态
            RefreshDevicesStatus();

            // 显示设备管理面板
            viewModel.ShowDeviceGrid = true;
            // 显示新设备面板
            viewModel.ShowNewDeviceGrid = true;

            // 正在搜索中
            if (deviceSearching)
            {
                return;
            }

            // loading
            SetLoading(true);
            // 设置当前正在搜索中状态
            deviceSearching = true;
            // 清空
            viewModel.NewDeviceList.Clear();

            // 查找设备
            List<Yeelight> yeelights = (List<Yeelight>)await YeelightUtils.SearchDeviceAsync(3000);

            foreach (var item in yeelights)
            {
                // 不存在
                if (!configStorage.HasDevice(item.Id))
                {
                    viewModel.NewDeviceList.Add(new Device(item) { Name = item.Id, Online = true });
                }
            }

            // 设置搜索完成状态
            deviceSearching = false;

            // loading
            SetLoading(false);
        }
        // 设备管理按钮按下
        private void BTN_DeviceManage_Click(object sender, RoutedEventArgs e)
        {
            // 显示设备管理面板
            viewModel.ShowDeviceGrid = true;
        }
        // 新设备列表项设备名输入框按键抬起
        private void TXT_NewDeviceName_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // 只处理回车
            if (e.Key != Windows.System.VirtualKey.Enter) return;

            var txt = (TextBox)sender;
            var device = (Device)txt.DataContext;

            device.Name = txt.Text;

            // 从新设备中删除, 加入设备列表并保存
            viewModel.NewDeviceList.Remove(device);
            viewModel.DeviceList.Add(device);
            configStorage.AddDevice(new ConfigStorage.Entiry.Device() { Id = device.Id, Name = device.Name, RawDeviceInfo = device.RawDevice.RawDevInfo });

            // 保存
            configStorage.SaveAsync();
        }
        // 设备列表项设备名输入框按键抬起
        private void TXT_DeviceName_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // 只处理回车
            if (e.Key != Windows.System.VirtualKey.Enter) return;

            var txt = (TextBox)sender;
            var device = (Device)txt.DataContext;

            // 未改变
            if (device.Name == txt.Text)
            {
                // 取消焦点
                txt.Focus(FocusState.Unfocused);
                return;
            }

            device.Name = txt.Text;

            // 更新设备
            var deviceEntiry = configStorage.GetDevice(device.Id);
            deviceEntiry.Name = device.Name;
            configStorage.UpdateDevice(device.Id, deviceEntiry);

            // 取消焦点
            LoseFocus(txt);

            // 保存
            configStorage.SaveAsync();
        }
        // 设备列表项右键点击事件 弹出菜单
        private void LB_DeviceListItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var senderElement = (FrameworkElement)sender;
            var flyoutBase = (MenuFlyout)FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement, e.GetPosition(senderElement));
        }
        // 删除设备菜单按下
        private async void MFI_DeviceDelete_Click(object sender, RoutedEventArgs e)
        {
            var device = (Device)((FrameworkElement)sender).DataContext;
            var isConfirm = await ShowConfirmDialog(string.Format("是否删除设备“{0}”", device.Name));

            if (isConfirm)
            {
                // 删除
                configStorage.DeleteDevice(device.Id);
                viewModel.DeviceList.Remove(device);

                // 保存
                await configStorage.SaveAsync();
            }
        }
        // 设备电源开关触发
        private async void TS_DevicePower_Toggled(object sender, RoutedEventArgs e)
        {
            var el = (ToggleSwitch)sender;
            var device = (Device)el.DataContext;

            // 设备正忙不进行处理
            if (device.IsBusy)
            {
                el.IsOn = !el.IsOn;
                return;
            }
            // 设置设备正忙
            device.IsBusy = true;

            // 进行开关操作
            try
            {
                await device.RawDevice.SetPower(el.IsOn ? YeelightPower.on : YeelightPower.off);
            }
            catch (Exception)
            {
                el.IsOn = !el.IsOn;
            }

            // 设置电源状态
            device.Power = el.IsOn;
            // 恢复
            device.IsBusy = false;
        }
        // 添加语音命令集按钮按下
        private void BTN_AddVoiceCommand_Click(object sender, RoutedEventArgs e)
        {
            // 刷新选择对象下拉框
            RefreshSelectTargetCombobox();
            // 创建新的语音命令集
            viewModel.VoiceCommandSetDetail = new VoiceCommandSet() { Id = Guid.NewGuid().ToString() };
            // 置空
            CBB_CommandType.DataContext = null;
            // 设置语音命令详情为已编辑状态 显示遮罩
            viewModel.VoiceCommandSetDetailIsEdit = true;
            // 显示Say框
            viewModel.ShowVoiceCommandSetDetailSayGrid = true;
        }
        // 选择对象选择框点击
        private void CBB_SelectTarget_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 显示菜单(假装是下拉框)
            var senderElement = (FrameworkElement)sender;
            var flyoutBase = (MenuFlyout)FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }
        // 选择对象菜单项按下
        private void CBB_SelectTarget_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // 隐藏菜单
            FlyoutBase.GetAttachedFlyout(CBB_SelectTarget).Hide();
            // 数据
            var dataContext = ((MenuFlyoutItemBase)sender).DataContext;
            // 设置显示dataContext文本
            CBB_SelectTarget.DataContext = dataContext;
            // 显示内容
            string content = "";

            // 全部
            if (dataContext == null)
                content = "全部";

            // 是设备
            if (dataContext != null && dataContext.GetType() == typeof(Device))
                content = ((Device)dataContext).Name;
            else if (dataContext != null && dataContext.GetType() == typeof(DeviceGroup))
                content = ((DeviceGroup)dataContext).Name;

            // 清空
            CBB_SelectTarget.Items.Clear();
            // 创建项
            var cbbItem = new ComboBoxItem() { Content = content, DataContext = dataContext };
            CBB_SelectTarget.Items.Add(cbbItem);
            // 选中
            CBB_SelectTarget.SelectedItem = cbbItem;
            // 设置语音命令详情为已编辑状态 显示遮罩
            viewModel.VoiceCommandSetDetailIsEdit = true;
        }
        // 选择对象父菜单项按下
        private void CBB_SelectTarget_MenuSubItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 触发
            CBB_SelectTarget_MenuItem_Click(sender, null);
        }
        // 选择操作选择框项改变
        private void CBB_SelectAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 全隐藏
            viewModel.ShowSetBrightGrid = false;
            viewModel.ShowSetBrightGrid = false;

            // 未选择
            if (CBB_SelectAction.SelectedItem == null)
            {
                return;
            }

            var action = (CommandType)CBB_SelectAction.SelectedItem;

            switch (action.Type)
            {
                case ActionType.PowerOn:
                    break;
                case ActionType.PowerOff:
                    break;
                case ActionType.BrightUp:
                case ActionType.BrightDown:
                    viewModel.ShowSetBrightGrid = true;
                    break;
                case ActionType.SwitchColor:
                    viewModel.ShowSwitchColorGrid = true;
                    break;
            }

            // 设置语音命令详情为已编辑状态 显示遮罩
            viewModel.VoiceCommandSetDetailIsEdit = true;
        }
        // 语音命令集详情遮罩点击
        private async void VoiceCommandSetDetailMaskGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (await ShowConfirmDialog("是否放弃当前的修改?", null, "是", "否"))
            {
                // 清空
                viewModel.VoiceCommandSetDetail = null;
                // 设置状态为未编辑
                viewModel.VoiceCommandSetDetailIsEdit = false;
            }
        }
        // Say输入框按键抬起
        private void TXT_Say_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // 只处理回车
            if (e.Key != Windows.System.VirtualKey.Enter) return;
            // 空的
            if (string.IsNullOrEmpty(TXT_Say.Text.Trim())) return;


            // 创建新的语音命令
            var voiceCommand = new VoiceCommand() { Id = Guid.NewGuid().ToString(), Say = TXT_Say.Text };
            // 加入列表
            viewModel.VoiceCommandSetDetail.VoiceCommandList.Add(voiceCommand);
            // 显示Answer框
            viewModel.ShowVoiceCommandSetDetailAnswerGrid = true;
            // 切入焦点
            TXT_Answer.Text = "";
            TXT_Answer.Focus(FocusState.Programmatic);
            // 设置语音命令详情为已编辑状态 显示遮罩
            viewModel.VoiceCommandSetDetailIsEdit = true;
        }
        // Answer输入框按键抬起
        private void TXT_Answer_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // 只处理回车
            if (e.Key != Windows.System.VirtualKey.Enter) return;
            // 空的
            if (string.IsNullOrEmpty(TXT_Answer.Text.Trim())) return;

            // 设置最后一条命令的回答
            var voiceCommand = viewModel.VoiceCommandSetDetail.VoiceCommandList.Last();
            voiceCommand.Answer = TXT_Answer.Text;
            // 显示Say框
            viewModel.ShowVoiceCommandSetDetailSayGrid = true;
            // 切入焦点
            TXT_Say.Text = "";
            TXT_Say.Focus(FocusState.Programmatic);
            // 设置语音命令详情为已编辑状态 显示遮罩
            viewModel.VoiceCommandSetDetailIsEdit = true;
        }
        // 删除语音命令按钮按下
        private void DeleteVoiceCommandButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var dataContext = (VoiceCommand)((FrameworkElement)sender).DataContext;

            // 删除
            viewModel.VoiceCommandSetDetail.VoiceCommandList.Remove(dataContext);

            // 设置语音命令详情为已编辑状态 显示遮罩
            viewModel.VoiceCommandSetDetailIsEdit = true;
        }
        // 删除语音命令集详情按钮按下
        private async void ABB_DeleteVoiceCommandSet_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (await ShowConfirmDialog("是否删除语音命令?", null, "是", "否"))
            {
                // 删除
                configStorage.DeleteVoiceCommandSet(viewModel.VoiceCommandSetDetail.Id);
                viewModel.VoiceCommandSetList.Remove(viewModel.VoiceCommandSetDetail);
                // 清空
                viewModel.VoiceCommandSetDetail = null;
                // 设置状态为未编辑
                viewModel.VoiceCommandSetDetailIsEdit = false;
                // 保存
                await configStorage.SaveAsync();
            }
        }
        // 保存语音命令集详情按钮按下
        private async void ABB_SaveVoiceCommandSet_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 检查基本设置
            if (CBB_CommandType.DataContext == null)
            {
                await ShowMessageDialog("请选择对象");
                PVT_VoiceCommandSetDetail.SelectedIndex = 0;
                return;
            }
            if (CBB_SelectAction.SelectedItem == null)
            {
                await ShowMessageDialog("请选择操作");
                PVT_VoiceCommandSetDetail.SelectedIndex = 0;
                return;
            }
            // 检查语音指令
            if (viewModel.VoiceCommandSetDetail.VoiceCommandList.Count == 0 )
            {
                await ShowMessageDialog("请添加语音指令");
                PVT_VoiceCommandSetDetail.SelectedIndex = 1;
                return;
            }
            if (string.IsNullOrEmpty(viewModel.VoiceCommandSetDetail.VoiceCommandList.Last().Answer))
            {
                await ShowMessageDialog("请将语音指令补充完整");
                PVT_VoiceCommandSetDetail.SelectedIndex = 1;
                return;
            }

            //TODO

            // 设置状态为未编辑
            viewModel.VoiceCommandSetDetailIsEdit = false;
            // 保存
            await configStorage.SaveAsync();
        }
    }
}
