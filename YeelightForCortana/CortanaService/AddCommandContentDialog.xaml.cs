using CortanaService.LightActionPage;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace CortanaService
{
    public sealed partial class AddCommandContentDialog : ContentDialog
    {
        private bool isConfirm = false;
        private string setting = "";

        public bool IsConfirm
        {
            get
            {
                return isConfirm;
            }

            set
            {
                isConfirm = value;
            }
        }

        public string Setting
        {
            get
            {
                return setting;
            }

            set
            {
                setting = value;
            }
        }

        public AddCommandContentDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            if (string.IsNullOrEmpty(tbListenFor.Text))
                return;
            if (string.IsNullOrEmpty(tbFeedback.Text))
                return;
            if (lbDeviceList.SelectedItems.Count == 0)
                return;
            if (frameLightAction.Content == null)
                return;

            args.Cancel = false;

            var page = (ILightActionPage)frameLightAction.Content;

            JObject setting = new JObject();
            JArray deviceIds = new JArray();

            foreach (Yeelight item in lbDeviceList.SelectedItems)
                deviceIds.Add(item.Id);

            setting["deviceIds"] = deviceIds;
            setting["listenFor"] = tbListenFor.Text;
            setting["feedBack"] = tbFeedback.Text;
            setting["action"] = JObject.Parse(page.GetValue());

            this.IsConfirm = true;
            this.Setting = setting.ToString();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置动作下拉列表源
            ObservableCollection<ComboBoxItem> actionList = new ObservableCollection<ComboBoxItem>();
            actionList.Add(new ComboBoxItem() { Content = "电源控制", Tag = LightActionType.Power });
            actionList.Add(new ComboBoxItem() { Content = "亮度控制", Tag = LightActionType.Bright });
            cbLightAction.ItemsSource = actionList;

            // 设置电灯列表源
            lbDeviceList.ItemsSource = await YeelightAPI.YeelightUtils.SearchDeviceAsync(2000);

            // 启用表单
            svBody.IsEnabled = true;
            // 隐藏Loading
            prLoading.IsActive = false;

        }

        // 动作选择变更
        private void cbLightAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)cbLightAction.SelectedItem;
            LightActionType type = (LightActionType)item.Tag;
            Type pageType;

            switch (type)
            {
                case LightActionType.Power:
                    pageType = typeof(LightActionPage.LightActionPowerPage);
                    break;
                case LightActionType.Bright:
                    pageType = typeof(LightActionPage.LightActionBrightPage);
                    break;
                default:
                    throw new Exception("选择错误");

            }

            frameLightAction.Navigate(pageType);
        }
    }
}
