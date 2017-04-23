using ConfigStorage.Entiry;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigStorage
{
    /// <summary>
    /// 配置存储接口
    /// </summary>
    public interface IConfigStorage
    {
        /// <summary>
        /// 是否存在设备
        /// </summary>
        /// <param name="id">设备编号</param>
        /// <returns>是否存在</returns>
        bool HasDevice(string id);
        /// <summary>
        /// 是否存在分组
        /// </summary>
        /// <param name="id">分组编号</param>
        /// <returns>是否存在</returns>
        bool HasGroup(string id);
        /// <summary>
        /// 是否存在语音命令
        /// </summary>
        /// <param name="id">语音命令编号</param>
        /// <returns>是否存在</returns>
        bool HasVoiceCommandSet(string id);

        /// <summary>
        /// 获取设备
        /// </summary>
        /// <param name="id">设备编号</param>
        /// <returns>设备</returns>
        Device GetDevice(string id);
        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <returns>设备列表</returns>
        List<Device> GetDevices();
        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <param name="name">设备名称 模糊匹配</param>
        /// <returns设备列表></returns>
        List<Device> GetDevices(string name);
        /// <summary>
        /// 获取分组
        /// </summary>
        /// <param name="id">分组编号</param>
        /// <returns>分组</returns>
        Group GetGroup(string id);
        /// <summary>
        /// 获取分组列表
        /// </summary>
        /// <returns>分组列表</returns>
        List<Group> GetGroups();
        /// <summary>
        /// 获取分组列表
        /// </summary>
        /// <param name="name">分组名称 模糊匹配</param>
        /// <returns>分组列表</returns>
        List<Group> GetGroups(string name);
        /// <summary>
        /// 获取语音命令集
        /// </summary>
        /// <param name="id">语音命令集编号</param>
        /// <returns>语音命令集</returns>
        VoiceCommandSet GetVoiceCommandSet(string id);
        /// <summary>
        /// 获取语音命令集列表
        /// </summary>
        /// <returns>语音命令集列表</returns>
        List<VoiceCommandSet> GetVoiceCommandSets();

        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="device">设备实体</param>
        /// <returns>是否成功</returns>
        bool AddDevice(Device device);
        /// <summary>
        /// 添加分组
        /// </summary>
        /// <param name="group">分组实体</param>
        /// <returns>是否成功</returns>
        bool AddGroup(Group group);
        /// <summary>
        /// 添加语音命令集
        /// </summary>
        /// <param name="voiceCommandSet">语音命令实体</param>
        /// <returns>是否成功</returns>
        bool AddVoiceCommandSet(VoiceCommandSet voiceCommandSet);

        /// <summary>
        /// 更新设备
        /// </summary>
        /// <param name="id">设备编号</param>
        /// <param name="device">设备实体</param>
        /// <returns>是否成功</returns>
        bool UpdateDevice(string id, Device device);
        /// <summary>
        /// 更新分组
        /// </summary>
        /// <param name="id">分组编号</param>
        /// <param name="group">分组实体</param>
        /// <returns>是否成功</returns>
        bool UpdateGroup(string id, Group group);
        /// <summary>
        /// 更新语音命令集
        /// </summary>
        /// <param name="id">语音命令集编号</param>
        /// <param name="voiceCommandSet">语音命令集实体</param>
        /// <returns>是否成功</returns>
        bool UpdateVoiceCommandSet(string id, VoiceCommandSet voiceCommandSet);

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="id">设备编号</param>
        /// <returns>是否成功</returns>
        bool DeleteDevice(string id);
        /// <summary>
        /// 删除分组
        /// </summary>
        /// <param name="id">分组编号</param>
        /// <returns>是否成功</returns>
        bool DeleteGroup(string id);
        /// <summary>
        /// 删除语音命令集
        /// </summary>
        /// <param name="id">语音命令集编号</param>
        /// <returns>是否成功</returns>
        bool DeleteVoiceCommandSet(string id);
        /// <summary>
        /// 通过分组编号删除语音命令集
        /// </summary>
        /// <param name="id">分组编号</param>
        /// <returns>是否成功</returns>
        bool DeleteVoiceCommandSetByGroupId(string id);

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns>是否成功</returns>
        Task<bool> LoadAsync();
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <returns>是否成功</returns>
        Task<bool> SaveAsync();
    }
}