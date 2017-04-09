namespace ConfigStorage.Entiry
{
    /// <summary>
    /// 设备实体
    /// </summary>
    public class Device
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 设备名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 源设备信息
        /// </summary>
        public string RawDeviceInfo { get; set; }
    }
}
