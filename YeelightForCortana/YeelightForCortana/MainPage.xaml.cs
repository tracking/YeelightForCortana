using ConfigStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
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
        }

        // 页面加载完成
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
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
            titleBar.InactiveBackgroundColor = titleBar.BackgroundColor = titleBar.ButtonInactiveBackgroundColor = titleBar.ButtonBackgroundColor = Colors.Black;
            titleBar.InactiveForegroundColor = titleBar.ForegroundColor = titleBar.ButtonInactiveForegroundColor = titleBar.ButtonForegroundColor = Colors.White;
        }
        /// <summary>
        /// 数据初始化
        /// </summary>
        /// <returns></returns>
        private async Task DataInit()
        {
            // 实例化配置存储对象
            configStorage = new JsonConfigStorage();
            // 加载配置
            await configStorage.LoadAsync();

            // 实例化viewModel
            var viewModel = new MainPageViewModel();
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

            // 初始化设备
            var devices = configStorage.GetDevices();

            foreach (var item in devices)
            {
                viewModel.DeviceList.Add(new Device(item.RawDeviceInfo) { Name = item.Name });
            }

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

            this.viewModel = viewModel;

            // 默认选择第一个
            this.viewModel.CommandTypeListSelectedIndex = 0;

            SetLoading(false);

            // 刷新设备状态（无需等待）
            RefreshDevicesStatus();
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
        /// 刷新语音命令集列表
        /// </summary>
        private void RefreshVoiceCommandSetList()
        {
            var group = (DeviceGroup)LB_DeviceGroupList.SelectedItem;
            var commandType = (CommandType)CBB_CommandType.SelectedItem;

            if (group == null || commandType == null)
            {
                viewModel.VoiceCommandSetList.Clear();
                return;
            }

            // 获取所有命令
            var vcss = configStorage.GetVoiceCommandSets()
                // 查找
                .Where(item =>
                {
                    // 操作类型不相同 并且 不是全部
                    if (commandType.Type != null && item.Action != commandType.Type.ToString())
                        return false;

                    // 分组不相同 并且 设备不在该组内 并且 不是全部
                    if (group.Id != "0" && item.GroupId != group.Id && group.DeviceList.Where(i => i.Id == item.DeviceId).Count() == 0)
                        return false;

                    return true;
                })
                // 转换
                .Select(item =>
                {
                    try
                    {
                        var vcs = new VoiceCommandSet();
                        vcs.Id = item.Id;
                        vcs.ActionParams = item.ActionParams;
                        vcs.CommandType = new CommandType((ActionType)Enum.Parse(typeof(ActionType), item.Action));

                        if (item.DeviceId != null)
                            vcs.Device = viewModel.DeviceList.First(i => i.Id == item.DeviceId);
                        if (item.GroupId != null)
                            vcs.DeviceGroup = viewModel.DeviceGroupList.First(i => i.Id == item.GroupId);

                        vcs.VoiceCommandList = new System.Collections.ObjectModel.ObservableCollection<VoiceCommand>(item.VoiceCommands.Select(i => new VoiceCommand()
                        {
                            Id = i.Id,
                            Answer = i.Answer,
                            Say = i.Say
                        }));

                        return vcs;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                });

            // 清空
            viewModel.VoiceCommandSetList.Clear();
            // 加入查到的
            foreach (var item in vcss.ToList())
                viewModel.VoiceCommandSetList.Add(item);
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
            foreach (var group in viewModel.DeviceGroupList)
            {
                // 创建父菜单
                var menuSubItem = new MenuFlyoutSubItem() { DataContext = group };
                menuSubItem.SetBinding(MenuFlyoutSubItem.TextProperty, new Binding() { Path = new PropertyPath("Name") });
                menuSubItem.Tapped += CBB_SelectTarget_MenuSubItem_Tapped;

                var deviceList = group.DeviceList;

                // 全部
                if (group.Id == "0")
                    deviceList = viewModel.DeviceList.ToList();

                foreach (var device in deviceList)
                {
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
        }
        /// <summary>
        /// 转换操作参数
        /// </summary>
        /// <param name="type">操作类型</param>
        /// <returns>用于保存的操作参数</returns>
        private string TransformActionParams(ActionType? type)
        {
            switch (type)
            {
                case ActionType.BrightUp:
                case ActionType.BrightDown:
                    return SLD_Bright.Value.ToString();
                case ActionType.SwitchColor:
                    return string.Format("{0},{1},{2}", CS_ColorSelector.Hsv.H, CS_ColorSelector.Hsv.S, CS_ColorSelector.Hsv.V);
                case ActionType.PowerOn:
                case ActionType.PowerOff:
                default:
                    return null;
            }
        }
        /// <summary>
        /// 转换操作参数到控件
        /// </summary>
        /// <param name="type">操作类型</param>
        /// <param name="param">参数</param>
        private void TransformActionParams(ActionType? type, string param)
        {
            if (type == null) return;

            try
            {
                switch (type)
                {
                    case ActionType.BrightUp:
                    case ActionType.BrightDown:
                        SLD_Bright.Value = Convert.ToInt32(param);
                        break;
                    case ActionType.SwitchColor:
                        var hsv = param.Split(',');
                        CS_ColorSelector.Hsv = new ColorMine.ColorSpaces.Hsv()
                        {
                            H = Convert.ToDouble(hsv[0]),
                            S = Convert.ToDouble(hsv[1]),
                            V = Convert.ToDouble(hsv[2])
                        };
                        break;
                    case ActionType.PowerOn:
                    case ActionType.PowerOff:
                    default:
                        break;
                }
            }
            catch (Exception) { }
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
                // 电源
                device.Power = device.RawDevice.Power == YeelightPower.on;
                // 无异常表示在线
                device.Online = true;
            }
            catch (Exception)
            {
                // 异常表示离线
                device.Online = false;
            }
        }
        /// <summary>
        /// 刷新所有设备状态
        /// </summary>
        private async Task RefreshDevicesStatus()
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
                Grid_TopProgress.Visibility = Visibility.Visible;
                TopProgressStoryboard.Stop();
                TopProgressStoryboard.RepeatBehavior = RepeatBehavior.Forever;
                TopProgressStoryboard.Begin();
            }
            else
            {
                Grid_TopProgress.Visibility = Visibility.Collapsed;
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
        /// <summary>
        /// 保存小娜配置
        /// </summary>
        /// <returns></returns>
        private async Task SaveCortanaConfig()
        {
            // 整合语音命令
            var vcss = configStorage.GetVoiceCommandSets();
            var sayList = new List<string>();

            // 遍历语音命令集
            foreach (var vcs in vcss)
            {
                // 遍历语音命令
                foreach (var vc in vcs.VoiceCommands)
                {
                    if (!sayList.Contains(vc.Say))
                        sayList.Add(vc.Say);
                }
            }

            // 构建XML
            XNamespace xnVoiceCommands = "http://schemas.microsoft.com/voicecommands/1.2";
            XDocument xdoc = new XDocument();
            XElement VoiceCommands = new XElement(xnVoiceCommands + "VoiceCommands");
            XElement CommandSet = new XElement(xnVoiceCommands + "CommandSet");
            XElement AppName = new XElement(xnVoiceCommands + "AppName");
            XElement AppExample = new XElement(xnVoiceCommands + "Example");
            XElement Command = new XElement(xnVoiceCommands + "Command");
            XElement CommandExample = new XElement(xnVoiceCommands + "Example");
            XElement ListenFor = new XElement(xnVoiceCommands + "ListenFor");
            XElement Feedback = new XElement(xnVoiceCommands + "Feedback");
            XElement VoiceCommandService = new XElement(xnVoiceCommands + "VoiceCommandService");
            XElement PhraseList = new XElement(xnVoiceCommands + "PhraseList");

            // xdoc
            xdoc.Declaration = new XDeclaration("1.0", "utf-8", "");
            xdoc.Add(VoiceCommands);

            // VoiceCommands
            VoiceCommands.SetAttributeValue("xmlns", xnVoiceCommands);
            VoiceCommands.Add(CommandSet);

            // CommandSet
            CommandSet.SetAttributeValue(XNamespace.Xml + "lang", "zh-cn");
            CommandSet.SetAttributeValue("Name", "YeelightVoiceCommandSet_zh-cn");
            CommandSet.Add(AppName);
            CommandSet.Add(AppExample);

            // AppName
            AppName.SetValue("我告诉你");

            // AppExample
            AppExample.SetValue("你好小娜");

            // Command
            Command.SetAttributeValue("Name", "Action");
            Command.Add(CommandExample);

            // Example
            CommandExample.SetValue("你好小娜，帮我开下灯");

            // ListenFor
            ListenFor.SetValue("{Say}");
            Command.Add(ListenFor);


            // Feedback
            Feedback.SetValue("已收到指令");
            Command.Add(Feedback);

            // VoiceCommandService
            VoiceCommandService.SetAttributeValue("Target", "YeelightVoiceCommandService");
            Command.Add(VoiceCommandService);

            // 加入
            CommandSet.Add(Command);

            // PhraseList
            PhraseList.SetAttributeValue("Label", "Say");

            // PhraseListItem
            foreach (var say in sayList)
            {
                XElement Item = new XElement(xnVoiceCommands + "Item");
                Item.SetValue(say);
                PhraseList.Add(Item);
            }

            // 加入
            CommandSet.Add(PhraseList);

            // 写到文件
            var vcdFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(@"Voice.xml", CreationCollisionOption.ReplaceExisting);
            using (var stream = await vcdFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // 创建写入流
                Stream writeStream = stream.AsStreamForWrite();
                // XML数据写入流
                xdoc.Save(writeStream);
                // 写入磁盘
                await writeStream.FlushAsync();
            }

            // 安装语音命令文件
            await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdFile);
        }

        #region 左区-设备组列表相关
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

                // 删除与分组相关的语音命令集
                configStorage.DeleteVoiceCommandSetByGroupId(group.Id);

                // 刷新
                RefreshVoiceCommandSetList();

                // 保存
                await configStorage.SaveAsync();
            }
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
        // 分组编辑面板关闭中
        private void SV_DeviceGroupConfig_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            if (string.IsNullOrEmpty(TXT_AddDeviceGroupName.Text))
            {
                TXT_AddDeviceGroupName.Text = "未命名";
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
        // 设备组列表项选中事件
        private void LB_DeviceGroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel == null) return;

            // 刷新列表
            RefreshVoiceCommandSetList();
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
        #endregion

        #region 左区-设备管理相关
        // 查找设备按钮按下
        private async void BTN_SearchDevice_Click(object sender, RoutedEventArgs e)
        {
            // 刷新设备状态
            RefreshDevicesStatus();

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
                    var device = new Device(item) { Name = item.Id, Online = true };
                    device.Power = device.RawDevice.Power == YeelightPower.on;
                    viewModel.NewDeviceList.Add(device);
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
        #endregion

        #region 中区-设备管理相关
        // 刷新设备状态按钮按下
        private async void BTN_RefreshDeviceStatus_Click(object sender, RoutedEventArgs e)
        {
            SetLoading(true);

            // 刷新设备状态
            await RefreshDevicesStatus();

            SetLoading(false);
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
                LoseFocus(txt);
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

            if (device == null || device.Power == el.IsOn)
            {
                return;
            }

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
        #endregion

        #region 中区-语音命令管理相关
        // 操作类型列表选择变更事件
        private void CBB_CommandType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel == null) return;

            // 刷新列表
            RefreshVoiceCommandSetList();
        }
        // 添加语音命令集按钮按下
        private void BTN_AddVoiceCommand_Click(object sender, RoutedEventArgs e)
        {
            // 清空编辑标志
            PVT_VoiceCommandSetDetail.Tag = null;

            // 刷新选择对象下拉框
            RefreshSelectTargetCombobox();
            // 创建新的语音命令集
            var vcs = new VoiceCommandSet() { Id = Guid.NewGuid().ToString() };
            // 置空
            CBB_SelectTarget.SelectedItem = null;
            CBB_SelectAction.SelectedItem = null;
            SLD_Bright.Value = 0;
            CS_ColorSelector.Hsv = new ColorMine.ColorSpaces.Hsv();
            // 设置语音命令详情为已编辑状态 显示遮罩
            viewModel.VoiceCommandSetDetailIsEdit = true;
            // 选中第一个面板
            PVT_VoiceCommandSetDetail.SelectedIndex = 0;
            // 显示Say框
            viewModel.ShowVoiceCommandSetDetailSayGrid = true;
            // 给值
            viewModel.VoiceCommandSetDetail = vcs;
        }
        // 语音命令集列表选中
        private void LB_VoiceCommandSetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vcs = (VoiceCommandSet)LB_VoiceCommandSetList.SelectedItem;

            if (vcs == null) return;

            // 创建新的语音命令集(防止修改对象引起别处发生变更)
            viewModel.VoiceCommandSetDetail = new VoiceCommandSet()
            {
                Id = vcs.Id,
                ActionParams = vcs.ActionParams,
                CommandType = vcs.CommandType,
                Device = vcs.Device,
                DeviceGroup = vcs.DeviceGroup,
                VoiceCommandList = new System.Collections.ObjectModel.ObservableCollection<VoiceCommand>(vcs.VoiceCommandList.Select(item =>
                {
                    return new VoiceCommand()
                    {
                        Id = item.Id,
                        Answer = item.Answer,
                        Say = item.Say
                    };
                }))
            };

            // 刷新选择对象下拉框
            RefreshSelectTargetCombobox();

            // 选中对应的对象
            // 清空
            CBB_SelectTarget.Items.Clear();
            // 创建项
            var cbbItem = new ComboBoxItem() { DataContext = vcs.Device != null ? (object)vcs.Device : (object)vcs.DeviceGroup };
            cbbItem.SetBinding(ComboBoxItem.ContentProperty, new Binding() { Path = new PropertyPath("Name") });
            CBB_SelectTarget.Items.Add(cbbItem);
            // 选中
            CBB_SelectTarget.SelectedItem = cbbItem;

            // 选中对应的操作
            CBB_SelectAction.SelectedItem = viewModel.CommandTypeList.Where(item =>
            {
                return ((CommandType)item).Type == vcs.CommandType.Type;
            }).ToList()[0];

            // 转换参数到控件
            TransformActionParams(vcs.CommandType.Type, vcs.ActionParams);

            // 显示Say框
            viewModel.ShowVoiceCommandSetDetailSayGrid = true;
            // 设置语音命令详情为未编辑状态
            viewModel.VoiceCommandSetDetailIsEdit = false;
            // 设置编辑标志
            PVT_VoiceCommandSetDetail.Tag = true;
        }
        // 删除语音命令集按钮按下
        private async void DeleteVoiceCommandSetButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vcs = (VoiceCommandSet)((FrameworkElement)sender).DataContext;

            if (await ShowConfirmDialog("是否删除语音命令?", null, "是", "否"))
            {
                // 删除
                configStorage.DeleteVoiceCommandSet(vcs.Id);
                // 保存
                await configStorage.SaveAsync();
                // 刷新列表
                RefreshVoiceCommandSetList();
                // 保存小娜设置
                await SaveCortanaConfig();
            }
        }
        #endregion

        #region 右区-语音命令集详情管理相关
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

            // 清空
            CBB_SelectTarget.Items.Clear();
            // 创建项
            var cbbItem = new ComboBoxItem() { DataContext = dataContext };
            cbbItem.SetBinding(ComboBoxItem.ContentProperty, new Binding() { Path = new PropertyPath("Name") });
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
            if (CBB_SelectAction.SelectedItem == null) return;

            var action = (CommandType)CBB_SelectAction.SelectedItem;

            switch (action.Type)
            {
                case ActionType.PowerOn:
                case ActionType.PowerOff:
                    viewModel.ShowSwitchColorGrid = false;
                    viewModel.ShowSetBrightGrid = false;
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
        // 滑动条数据变更
        private void SLD_Bright_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (viewModel == null) return;

            // 设置语音命令详情为已编辑状态 显示遮罩
            viewModel.VoiceCommandSetDetailIsEdit = true;
        }
        // 颜色选择器颜色变更
        private void CS_ColorSelector_ColorChange(object sender)
        {
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
                // 刷新列表
                RefreshVoiceCommandSetList();
            }
        }
        // Say输入框按键抬起
        private async void TXT_Say_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // 只处理回车
            if (e.Key != Windows.System.VirtualKey.Enter) return;

            // 空的
            if (string.IsNullOrEmpty(TXT_Say.Text.Trim())) return;
            // 不允许输入除中文/字母/数字以外的字符
            if (!new Regex(@"^[\u4e00-\u9fa5a-zA-Z0-9\s]*$").Match(TXT_Say.Text).Success)
            {
                await ShowMessageDialog("不能输入除中文/字母/数字/空格以外的字");
                return;
            }

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
        private async void TXT_Answer_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // 只处理回车
            if (e.Key != Windows.System.VirtualKey.Enter) return;

            // 空的
            if (string.IsNullOrEmpty(TXT_Answer.Text.Trim())) return;
            // 不允许输入除中文/字母/数字以外的字符
            if (!new Regex(@"^[\u4e00-\u9fa5a-zA-Z0-9\s]*$").Match(TXT_Answer.Text).Success)
            {
                await ShowMessageDialog("不能输入除中文/字母/数字/空格以外的字");
                return;
            }

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
                // 清空
                viewModel.VoiceCommandSetDetail = null;
                // 设置状态为未编辑
                viewModel.VoiceCommandSetDetailIsEdit = false;
                // 保存
                await configStorage.SaveAsync();
                // 刷新列表
                RefreshVoiceCommandSetList();
                // 保存小娜设置
                await SaveCortanaConfig();
            }
        }
        // 保存语音命令集详情按钮按下
        private async void ABB_SaveVoiceCommandSet_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 未编辑不保存
            if (!viewModel.VoiceCommandSetDetailIsEdit) return;

            // 检查基本设置
            if (CBB_SelectTarget.SelectedItem == null)
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
            if (viewModel.VoiceCommandSetDetail.VoiceCommandList.Count == 0)
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

            // 操作类型
            var actionType = (CommandType)CBB_SelectAction.SelectedItem;
            // 对象
            var target = ((ComboBoxItem)CBB_SelectTarget.SelectedItem).DataContext;
            // 创建实体
            var voiceCommandSetEntiry = new ConfigStorage.Entiry.VoiceCommandSet()
            {
                Id = viewModel.VoiceCommandSetDetail.Id,
                Action = actionType.Type.ToString(),
                ActionParams = TransformActionParams(actionType.Type)
            };

            // 如果对象是组
            if (target.GetType() == typeof(DeviceGroup))
            {
                voiceCommandSetEntiry.GroupId = ((DeviceGroup)target).Id;

                // 是全部
                if (voiceCommandSetEntiry.GroupId == "0")
                    voiceCommandSetEntiry.IsAll = true;
            }
            // 要么就是设备
            else
            {
                voiceCommandSetEntiry.DeviceId = ((Device)target).Id;
                voiceCommandSetEntiry.IsAll = false;
            }

            // 遍历语音命令
            for (int i = 0; i < viewModel.VoiceCommandSetDetail.VoiceCommandList.Count; i++)
            {
                var item = viewModel.VoiceCommandSetDetail.VoiceCommandList[i];
                voiceCommandSetEntiry.VoiceCommands.Add(new ConfigStorage.Entiry.VoiceCommand() { Id = item.Id, Say = item.Say, Answer = item.Answer });
            }

            // 编辑
            if (PVT_VoiceCommandSetDetail.Tag != null)
                // 更新配置
                configStorage.UpdateVoiceCommandSet(voiceCommandSetEntiry.Id, voiceCommandSetEntiry);
            else
                // 加入配置
                configStorage.AddVoiceCommandSet(voiceCommandSetEntiry);

            // 清空
            viewModel.VoiceCommandSetDetail = null;
            // 设置状态为未编辑
            viewModel.VoiceCommandSetDetailIsEdit = false;
            // 保存
            await configStorage.SaveAsync();

            // 刷新列表
            RefreshVoiceCommandSetList();
            // 保存小娜设置
            await SaveCortanaConfig();
        }
        #endregion
    }
}
