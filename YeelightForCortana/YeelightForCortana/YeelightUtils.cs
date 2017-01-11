using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace YeelightForCortana
{
    /// <summary>
    /// Yeelight工具类
    /// </summary>
    class YeelightUtils
    {
        // 组播地址
        private static HostName MULTICAST_HOST = new HostName("239.255.255.250");
        // 组播端口
        private static string MULTICAST_PORT = "1982";
        // 搜索设备广播内容
        private static string SEARCH_DEVICE_MULTCAST_CONTENT = "M-SEARCH * HTTP/1.1\r\nHOST:239.255.255.250:1982\r\nMAN:\"ssdp:discover\"\r\nST:wifi_bulb\r\n";

        // 搜索超时
        private static int SEARCH_DEVICE_TIMEOUT = 2000;

        /// <summary>
        /// 搜索设备
        /// </summary>
        /// <returns>Yeelight对象</returns>
        public async static Task<List<Yeelight>> SearchDevice()
        {
            // 创建Socket
            DatagramSocket udp = new DatagramSocket();
            // 绑定随机端口
            await udp.BindServiceNameAsync("");
            // 获取输出流
            var outputStream = await udp.GetOutputStreamAsync(MULTICAST_HOST, MULTICAST_PORT);

            // Yeelight列表
            Dictionary<string, Yeelight> yeelightList = new Dictionary<string, Yeelight>();

            // 处理回应
            udp.MessageReceived += (sender, args) =>
            {
                // 读取对象
                var reader = args.GetDataReader();
                // 读取设备信息
                string rawDevInfo = reader.ReadString(reader.UnconsumedBufferLength);

                try
                {
                    // 构造Yeelight对象
                    Yeelight device = new Yeelight(rawDevInfo);

                    // 是否已存在
                    if (yeelightList.ContainsKey(device.Id))
                    {
                        // 替换
                        yeelightList[device.Id] = device;
                    }
                    else
                    {
                        // 新增
                        yeelightList.Add(device.Id, device);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            };

            // 创建数据写入对象
            using (DataWriter writer = new DataWriter(outputStream))
            {
                // 写入缓冲区
                writer.WriteString(SEARCH_DEVICE_MULTCAST_CONTENT);
                // 发送数据
                await writer.StoreAsync();

                // 等待两秒时间
                await Task.Delay(SEARCH_DEVICE_TIMEOUT);
            }

            // 清理资源
            await udp.CancelIOAsync();
            udp.Dispose();

            return yeelightList.Values.ToList<Yeelight>();
        }

        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns>IP地址列表</returns>
        public static List<string> GetLocalIPs()
        {
            // 用于存储结果集合
            List<string> results = new List<string>();
            // 获取HOSTNAMES
            var hostNames = NetworkInformation.GetHostNames();

            // 遍历
            foreach (var item in hostNames)
            {
                // 只取IPv4地址
                if (item.Type == Windows.Networking.HostNameType.Ipv4)
                {
                    results.Add(item.CanonicalName);
                }
            }

            return results;
        }
    }
}
