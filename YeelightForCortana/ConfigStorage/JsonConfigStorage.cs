using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigStorage.Entiry;
using Newtonsoft.Json.Linq;
using Windows.Storage;
using System.IO;
using Newtonsoft.Json;

namespace ConfigStorage
{
    public class JsonConfigStorage : IConfigStorage
    {
        // 设置文件名称
        private readonly string CONFIG_FILE_NAME = "config.json";

        // 配置实体
        private Config config;

        public JsonConfigStorage()
        {
        }

        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="device">设备实体</param>
        /// <returns>是否成功</returns>
        public bool AddDevice(Device device)
        {
            config.Devices.Add(device);
            return true;
        }
        /// <summary>
        /// 添加分组
        /// </summary>
        /// <param name="group">分组实体</param>
        /// <returns>是否成功</returns>
        public bool AddGroup(Group group)
        {
            config.Groups.Add(group);
            return true;
        }
        /// <summary>
        /// 添加语音命令集
        /// </summary>
        /// <param name="voiceCommandSet">语音命令实体</param>
        /// <returns>是否成功</returns>
        public bool AddVoiceCommandSet(VoiceCommandSet voiceCommandSet)
        {
            config.VoiceCommandSets.Add(voiceCommandSet);
            return true;
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="id">设备编号</param>
        /// <returns>是否成功</returns>
        public bool DeleteDevice(string id)
        {
            var result = config.Devices.Remove(GetDevice(id));

            return result;
        }
        /// <summary>
        /// 删除分组
        /// </summary>
        /// <param name="id">分组编号</param>
        /// <returns>是否成功</returns>
        public bool DeleteGroup(string id)
        {
            var result = config.Groups.Remove(GetGroup(id));

            return result;
        }
        /// <summary>
        /// 删除语音命令集
        /// </summary>
        /// <param name="id">语音命令集编号</param>
        /// <returns>是否成功</returns>
        public bool DeleteVoiceCommandSet(string id)
        {
            var result = config.VoiceCommandSets.Remove(GetVoiceCommandSet(id));

            return result;
        }

        /// <summary>
        /// 获取设备
        /// </summary>
        /// <param name="id">设备编号</param>
        /// <returns>设备</returns>
        public Device GetDevice(string id)
        {
            return (from item in config.Devices where item.Id == id select item).First();
        }
        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <returns>设备列表</returns>
        public List<Device> GetDevices()
        {
            return config.Devices;
        }
        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <param name="name">设备名称 模糊匹配</param>
        /// <returns设备列表></returns>
        public List<Device> GetDevices(string name)
        {
            return (from item in config.Devices where item.Name.IndexOf(name) != -1 select item).ToList<Device>();
        }
        /// <summary>
        /// 获取分组
        /// </summary>
        /// <param name="id">分组编号</param>
        /// <returns>分组</returns>
        public Group GetGroup(string id)
        {
            return (from item in config.Groups where item.Id == id select item).First();
        }
        /// <summary>
        /// 获取分组列表
        /// </summary>
        /// <returns>分组列表</returns>
        public List<Group> GetGroups()
        {
            return config.Groups;
        }
        /// <summary>
        /// 获取分组列表
        /// </summary>
        /// <param name="name">分组名称 模糊匹配</param>
        /// <returns>分组列表</returns>
        public List<Group> GetGroups(string name)
        {
            return (from item in config.Groups where item.Name.IndexOf(name) != -1 select item).ToList<Group>();
        }
        /// <summary>
        /// 获取语音命令集
        /// </summary>
        /// <param name="id">语音命令集编号</param>
        /// <returns>语音命令集</returns>
        public VoiceCommandSet GetVoiceCommandSet(string id)
        {
            return (from item in config.VoiceCommandSets where item.Id == id select item).First();
        }
        /// <summary>
        /// 获取语音命令集列表
        /// </summary>
        /// <returns>语音命令集列表</returns>
        public List<VoiceCommandSet> GetVoiceCommandSets()
        {
            return config.VoiceCommandSets;
        }

        /// <summary>
        /// 是否存在设备
        /// </summary>
        /// <param name="id">设备编号</param>
        /// <returns>是否存在</returns>
        public bool HasDevice(string id)
        {
            return config.Devices.Exists(item => item.Id == id);
        }
        /// <summary>
        /// 是否存在分组
        /// </summary>
        /// <param name="id">分组编号</param>
        /// <returns>是否存在</returns>
        public bool HasGroup(string id)
        {
            return config.Groups.Exists(item => item.Id == id);
        }
        /// <summary>
        /// 是否存在语音命令
        /// </summary>
        /// <param name="id">语音命令编号</param>
        /// <returns>是否存在</returns>
        public bool HasVoiceCommandSet(string id)
        {
            return config.VoiceCommandSets.Exists(item => item.Id == id);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> LoadAsync()
        {
            // 本地文件夹
            var localFolder = ApplicationData.Current.LocalFolder;
            // 设置文件
            var settingFile = await localFolder.CreateFileAsync(CONFIG_FILE_NAME, CreationCollisionOption.OpenIfExists);

            // 创建读取流
            using (var stream = new StreamReader(await settingFile.OpenStreamForReadAsync()))
            {
                // 读取所有数据
                string data = await stream.ReadToEndAsync();

                // 空文件
                if (string.IsNullOrEmpty(data))
                {
                    // 初始化设置数据
                    config = new Config();
                }
                else
                {
                    try
                    {
                        // 转换成JSON对象
                        config = JsonConvert.DeserializeObject<Config>(data);
                    }
                    catch (Exception)
                    {
                        // 初始化设置数据
                        config = new Config();
                    }
                }
            }

            // 数据完整性验证
            if (config.Devices == null)
            {
                config.Devices = new List<Device>();
            }
            if (config.Groups == null)
            {
                config.Groups = new List<Group>();
            }
            if (config.VoiceCommandSets == null)
            {
                config.VoiceCommandSets = new List<VoiceCommandSet>();
            }

            return true;
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> SaveAsync()
        {
            // 本地文件夹
            var localFolder = ApplicationData.Current.LocalFolder;
            // 设置文件
            var settingFile = await localFolder.CreateFileAsync(CONFIG_FILE_NAME, CreationCollisionOption.ReplaceExisting);

            // 创建写入流
            using (var stream = new StreamWriter(await settingFile.OpenStreamForWriteAsync()))
            {
                // 写入数据
                await stream.WriteAsync(JsonConvert.SerializeObject(config));
            }

            return true;
        }

        /// <summary>
        /// 更新设备
        /// </summary>
        /// <param name="id">设备编号</param>
        /// <param name="device">设备实体</param>
        /// <returns>是否成功</returns>
        public bool UpdateDevice(string id, Device device)
        {
            var old = GetDevice(id);

            if (old != null)
            {
                old.Id = device.Id;
                old.Name = device.Name;

                return true;
            }

            return false;
        }
        /// <summary>
        /// 更新分组
        /// </summary>
        /// <param name="id">分组编号</param>
        /// <param name="group">分组实体</param>
        /// <returns>是否成功</returns>
        public bool UpdateGroup(string id, Group group)
        {
            var old = GetGroup(id);

            if (old != null)
            {
                old.Id = group.Id;
                old.Name = group.Name;
                old.Devices = group.Devices;

                return true;
            }

            return false;
        }
        /// <summary>
        /// 更新语音命令集
        /// </summary>
        /// <param name="id">语音命令集编号</param>
        /// <param name="voiceCommandSet">语音命令集实体</param>
        /// <returns>是否成功</returns>
        public bool UpdateVoiceCommandSet(string id, VoiceCommandSet voiceCommandSet)
        {
            var old = GetVoiceCommandSet(id);

            if (old != null)
            {
                old.Id = voiceCommandSet.Id;
                old.DeviceId = voiceCommandSet.DeviceId;
                old.GroupId = voiceCommandSet.GroupId;
                old.Action = voiceCommandSet.Action;
                old.VoiceCommands = voiceCommandSet.VoiceCommands;

                return true;
            }

            return false;
        }
    }
}
