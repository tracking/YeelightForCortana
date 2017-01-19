using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace CortanaService
{
    /// <summary>
    /// 设置类
    /// </summary>
    public static class SettingHelper
    {
        // 设置文件名称
        private static readonly string CONFIG_FILE_NAME = "setting.json";

        /// <summary>
        /// 添加命令并保存
        /// </summary>
        /// <param name="command">命令的JSON对象</param>
        /// <returns></returns>
        public static async Task AddCommand(JObject command)
        {
            // 本地文件夹
            var localFolder = ApplicationData.Current.LocalFolder;
            // 设置文件
            var settingFile = await localFolder.CreateFileAsync(CONFIG_FILE_NAME, CreationCollisionOption.OpenIfExists);
            // 读取设置
            JObject config = await LoadSetting(settingFile);

            // 添加当前命令
            ((JArray)config["commands"]).Add(command);

            // 保存
            await SaveSetting(config);
        }
        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="config">设置的JSON对象</param>
        /// <returns></returns>
        public static async Task SaveSetting(JObject config)
        {
            // 本地文件夹
            var localFolder = ApplicationData.Current.LocalFolder;
            // 设置文件
            var settingFile = await localFolder.CreateFileAsync(CONFIG_FILE_NAME, CreationCollisionOption.OpenIfExists);

            // 创建写入流
            using (var stream = new StreamWriter(await settingFile.OpenStreamForWriteAsync()))
            {
                // 写入数据
                await stream.WriteAsync(config.ToString());
            }
        }
        /// <summary>
        /// 读取设置
        /// </summary>
        /// <returns>设置的JSON对象</returns>
        public static async Task<JObject> LoadSetting()
        {
            // 本地文件夹
            var localFolder = ApplicationData.Current.LocalFolder;
            // 设置文件
            var settingFile = await localFolder.CreateFileAsync(CONFIG_FILE_NAME, CreationCollisionOption.OpenIfExists);

            JObject config;

            // 创建读取流
            using (var stream = new StreamReader(await settingFile.OpenStreamForReadAsync()))
            {
                // 读取所有数据
                string data = await stream.ReadToEndAsync();

                // 空文件
                if (string.IsNullOrEmpty(data))
                {
                    // 初始化设置数据
                    config = new JObject();
                    config["commands"] = new JArray();
                }
                else
                {
                    // 转换成JSON对象
                    config = JObject.Parse(data);
                }
            }

            return config;
        }
        /// <summary>
        /// 读取设置
        /// </summary>
        /// <param name="settingFile">文件对象</param>
        /// <returns>设置的JSON对象</returns>
        public static async Task<JObject> LoadSetting(StorageFile settingFile)
        {
            JObject config;

            // 创建读取流
            using (var stream = new StreamReader(await settingFile.OpenStreamForReadAsync()))
            {
                // 读取所有数据
                string data = await stream.ReadToEndAsync();

                // 空文件
                if (string.IsNullOrEmpty(data))
                {
                    // 初始化设置数据
                    config = new JObject();
                    config["commands"] = new JArray();
                }
                else
                {
                    // 转换成JSON对象
                    config = JObject.Parse(data);
                }
            }

            return config;
        }
        /// <summary>
        /// 小娜配置
        /// </summary>
        /// <returns></returns>
        public static async Task CortanaSetting()
        {
            // 读取设置
            var config = await LoadSetting();
            var commands = (JArray)config["commands"];

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
            CommandSet.SetAttributeValue("Name", "YeelightVoiceCommandSet_zh-cn");
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
    }
}
