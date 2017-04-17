namespace ConfigStorage.Entiry
{
    /// <summary>
    /// 语音命令实体
    /// </summary>
    public class VoiceCommand
    {
        /// <summary>
        /// 语音命令集编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 命令
        /// </summary>
        public string Say { get; set; }
        /// <summary>
        /// 回答
        /// </summary>
        public string Answer { get; set; }
    }
}