using System.Collections.Generic;

namespace ConfigStorage.Entiry
{
    /// <summary>
    /// 分组实体
    /// </summary>
    public class Group
    {
        public Group()
        {
            Devices = new List<string>();
        }

        /// <summary>
        /// 分组编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 分组名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 设备编号列表
        /// </summary>
        public List<string> Devices { get; set; }
    }
}
