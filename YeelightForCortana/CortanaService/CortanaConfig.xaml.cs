using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace CortanaService
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CortanaConfig : Page
    {
        public CortanaConfig()
        {
            this.InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            //// 安装语音命令文件
            //Windows.Storage.StorageFile vcdStorageFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"YeelightVoiceCommands.xml");
            //await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);

            //var folder = ApplicationData.Current.LocalFolder;
            //var file = await folder.CreateFileAsync("Voice.xml", CreationCollisionOption.ReplaceExisting);
            //var reader = await file.OpenStreamForWriteAsync();

            //using (var stream = await file.OpenStreamForWriteAsync())
            //{
            //    using (var writer = new StreamWriter(stream))
            //    {
            //        //writer.WriteAsync();
            //    }
            //}
            await SettingHelper.CortanaSetting();
        }

        // 加载中
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // 注册后退事件
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += PageBackRequested;
            // 显示后退按钮
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
        }
        // 卸载中
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // 取消注册后退事件
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= PageBackRequested;
        }
        // 后退事件处理
        private void PageBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            // 回退
            Frame.GoBack();
        }

        private async void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            AddCommandContentDialog dialog = new AddCommandContentDialog();
            await dialog.ShowAsync();

            if (!dialog.IsConfirm)
                return;

            JObject setting = JObject.Parse(dialog.Setting);

            await SaveSetting(setting);

            listBox.Items.Add(setting["listenFor"]);
        }

        private async Task SaveSetting(JObject setting)
        {
            await SettingHelper.AddCommand(setting);
        }

        private async void button_Click_1(object sender, RoutedEventArgs e)
        {
            await xmlgen();
        }
    }
}
