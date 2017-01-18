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
            await xmlgen();
        }
        private async Task xmlgen()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var vcdFile = await localFolder.CreateFileAsync(@"Voice.xml", CreationCollisionOption.ReplaceExisting);
            var configFile = await localFolder.CreateFileAsync(@"setting.json", CreationCollisionOption.OpenIfExists);
            JArray commands;

            using (var stream = new StreamReader(await configFile.OpenStreamForReadAsync()))
            {
                string data = await stream.ReadToEndAsync();
                commands = (JArray)JObject.Parse(data)["commands"];
            }

            XNamespace xnVoiceCommands = "http://schemas.microsoft.com/voicecommands/1.2";
            XDocument xdoc = new XDocument();
            XElement VoiceCommands = new XElement(xnVoiceCommands + "VoiceCommands");
            XElement CommandSet = new XElement(xnVoiceCommands + "CommandSet");
            XElement AppName = new XElement(xnVoiceCommands + "AppName");
            XElement AppExample = new XElement(xnVoiceCommands + "Example");

            // xdoc
            xdoc.Declaration = new XDeclaration("1.0", "utf-8", "");
            xdoc.Add(VoiceCommands);

            // VoiceCommands
            VoiceCommands.SetAttributeValue("xmlns", xnVoiceCommands);
            VoiceCommands.Add(CommandSet);

            // CommandSet
            CommandSet.SetAttributeValue(XNamespace.Xml + "lang", "zh-cn");
            CommandSet.SetAttributeValue("Name", "AdventureWorksCommandSet_zh-cn");
            CommandSet.Add(AppName);
            CommandSet.Add(AppExample);

            // AppName
            AppName.SetValue("你好小娜");

            // AppExample
            AppExample.SetValue("你好小娜");

            for (int i = 0; i < commands.Count; i++)
            {
                XElement Command = new XElement(xnVoiceCommands + "Command");
                XElement CommandExample = new XElement(xnVoiceCommands + "Example");
                XElement ListenFor = new XElement(xnVoiceCommands + "ListenFor");
                XElement Feedback = new XElement(xnVoiceCommands + "Feedback");
                XElement VoiceCommandService = new XElement(xnVoiceCommands + "VoiceCommandService");

                // Command
                Command.SetAttributeValue("Name", i);
                Command.Add(CommandExample);
                Command.Add(ListenFor);
                Command.Add(Feedback);
                Command.Add(VoiceCommandService);

                // Example
                CommandExample.SetValue(commands[i]["listenFor"]);

                // ListenFor
                ListenFor.SetValue(commands[i]["listenFor"]);

                // Feedback
                Feedback.SetValue(commands[i]["feedBack"]);

                // VoiceCommandService
                VoiceCommandService.SetAttributeValue("Target", "YeelightVoiceCommandService");

                // 加入
                CommandSet.Add(Command);
            }

            using (var stream = await vcdFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                Stream s = stream.AsStreamForWrite();
                Debug.WriteLine(s.Length);
                xdoc.Save(s);
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
            await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdFile);
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
            var localFolder = ApplicationData.Current.LocalFolder;
            var file = await localFolder.CreateFileAsync(@"setting.json", CreationCollisionOption.OpenIfExists);
            JObject config;

            using (var stream = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                string data = await stream.ReadToEndAsync();

                if (string.IsNullOrEmpty(data))
                {
                    config = new JObject();
                    config["commands"] = new JArray();
                }
                else
                {
                    config = JObject.Parse(data);
                }
            }

           ((JArray)config["commands"]).Add(setting);

            using (var stream = new StreamWriter(await file.OpenStreamForWriteAsync()))
            {
                await stream.WriteAsync(config.ToString());
            }
        }

        private async void button_Click_1(object sender, RoutedEventArgs e)
        {
            await xmlgen();
        }
    }
}
