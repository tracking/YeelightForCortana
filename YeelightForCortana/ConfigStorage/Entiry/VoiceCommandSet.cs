using System.Collections.Generic;

namespace ConfigStorage.Entiry
{
    /// <summary>
    /// 语音命令集实体
    /// </summary>
    public class VoiceCommandSet
    {
        public VoiceCommandSet()
        {
            VoiceCommands = new List<VoiceCommand>();
        }

        /// <summary>
        /// 语音命令集编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 操作
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 操作参数
        /// </summary>
        public string ActionParams { get; set; }
        /// <summary>
        /// 对象是否为全部
        /// </summary>
        public bool IsAll { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 分组编号
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 语音命令列表
        /// </summary>
        public List<VoiceCommand> VoiceCommands { get; set; }
    }
}
