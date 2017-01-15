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

namespace YeelightForCortana
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
            await xmlgen();
        }
        private async Task xmlgen()
        {
            var installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var localFolder = ApplicationData.Current.LocalFolder;
            var xx = await installFolder.OpenStreamForReadAsync(@"VoiceCommandTemplate.xml");
            var file = await localFolder.CreateFileAsync(@"Voice.xml", CreationCollisionOption.ReplaceExisting);

            XDocument xml = XDocument.Load(xx);
            XNamespace xn = "http://schemas.microsoft.com/voicecommands/1.2";
            var CommandSet = xml.Elements().First().Elements().First();
            XElement Command = new XElement(xn + "Command");
            XElement Example = new XElement(xn + "Example");
            XElement ListenFor = new XElement(xn + "ListenFor");
            XElement Feedback = new XElement(xn + "Feedback");
            XElement VoiceCommandService = new XElement(xn + "VoiceCommandService");

            // CommandSet
            CommandSet.Add(Command);

            // Command
            Command.SetAttributeValue("Name", "openPower");
            Command.Add(Example);
            Command.Add(ListenFor);
            Command.Add(Feedback);
            Command.Add(VoiceCommandService);

            // Example
            Example.SetValue("给我爆炸");

            // ListenFor
            ListenFor.SetValue("跳起舞来");

            // Feedback
            Feedback.SetValue("尬舞，尬尬尬舞");

            // VoiceCommandService
            VoiceCommandService.SetAttributeValue("Target", "YeelightVoiceCommandService");

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                Stream s = stream.AsStreamForWrite();
                Debug.WriteLine(s.Length);
                xml.Save(s);
                await s.FlushAsync();
                //System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
                //settings.Async = true;
                //using (var writer = System.Xml.XmlWriter.Create(s, settings))
                //{
                //    xdoc.WriteTo(writer);
                //    await writer.FlushAsync();
                //}
            }
            // 安装语音命令文件
            //Windows.Storage.StorageFile vcdStorageFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"YeelightVoiceCommands.xml");
            await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(file);
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
    }
}
