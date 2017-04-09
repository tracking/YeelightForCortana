using System.Collections.Generic;

namespace ConfigStorage.Entiry
{
    /// <summary>
    /// 设置实体
    /// </summary>
    public class Config
    {
        public Config()
        {
            Devices = new List<Device>();
            Groups = new List<Group>();
            VoiceCommandSets = new List<VoiceCommandSet>();
        }

        /// <summary>
        /// 设备列表
        /// </summary>
        public List<Device> Devices { get; set; }
        /// <summary>
        /// 分组列表
        /// </summary>
        public List<Group> Groups { get; set; }
        /// <summary>
        /// 语音命令集列表
        /// </summary>
        public List<VoiceCommandSet> VoiceCommandSets { get; set; }
    }
}
