using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YeelightForCortana
{
    /// <summary>
    /// Yeelight对象
    /// </summary>
    class Yeelight
    {
        // 源设备信息头
        private static Regex RAW_DEVICE_INFO_HEADER_REGEX = new Regex(@"^HTTP\/1\.1 200 OK\r\n");

        // result正则
        private static Regex RESULT_REGEX = new Regex("\"result\":" + @"\[" + "\"(.+)\"" + @"\]");

        // 设备ID
        private string id;
        // 设备IP地址
        private string ip;
        // 设备端口
        private string port;
        // 设备类型（目前只有：mono白光、color彩光、stripeLED条）
        private string model;
        // 支持函数列表
        private List<string> support;
        // 电源状态
        private bool power;
        // 亮度 1-100
        private int bright;
        // 颜色模式（1:可设置颜色 2:色温模式 3: HSV模式）
        private int color_mode;
        // 色温（颜色模式为2时才有效）
        private int color_temperature;
        // RGB
        private int rgb;
        // 色调
        private int hue;
        // 饱和度
        private int sat;
        // 名字
        private string name;


        // 设备连接对象
        //private TcpClient tcpClient;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rawDevInfo">源设备信息</param>
        public Yeelight(string rawDevInfo)
        {
            // 验证合法性
            if (!VerifyRawDevInfo(rawDevInfo))
            {
                throw new Exception("源设备信息不合法");
            }

            // 初始化
            ParseDeviceInfo(rawDevInfo);
        }

        /// <summary>
        /// 验证源设备信息合法性
        /// </summary>
        /// <param name="rawDevInfo">源设备信息</param>
        /// <returns>是否合法</returns>
        private bool VerifyRawDevInfo(string rawDevInfo)
        {
            return RAW_DEVICE_INFO_HEADER_REGEX.IsMatch(rawDevInfo);
        }
        /// <summary>
        /// 解析并初始化设备信息
        /// </summary>
        private void ParseDeviceInfo(string rawDevInfo)
        {

        }

        //public bool Toggle()
        //{
        //    // 命令
        //    byte[] buf = Encoding.Default.GetBytes("{\"id\": " + this.id + ", \"method\": \"toggle\", \"params\":[]}\r\n");
        //    // 发送
        //    this.tcpClient.Client.Send(buf);
        //    // 接收回应
        //    byte[] recv = new byte[1000];
        //    int len = this.tcpClient.Client.Receive(recv);

        //    // 转换
        //    string msg = Encoding.Default.GetString(recv.ToList().GetRange(0, len).ToArray());

        //    Debug.WriteLine(msg);

        //    // 处理
        //    var resultGroups = RESULT_REGEX.Match(msg).Groups;
        //    string result = resultGroups[1].ToString();

        //    return result == "ok" ? true : false;
        //}
    }
}
