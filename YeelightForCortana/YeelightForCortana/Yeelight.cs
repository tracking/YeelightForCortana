using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace YeelightForCortana
{
    /// <summary>
    /// Yeelight颜色模式
    /// </summary>
    enum YeelightModel
    {
        /// <summary>
        /// 白光
        /// </summary>
        MONO,
        /// <summary>
        /// 彩光
        /// </summary>
        COLOR,
        /// <summary>
        /// LED条
        /// </summary>
        STRIPE
    }
    /// <summary>
    /// Yeelight电源状态
    /// </summary>
    enum YeelightPower
    {
        /// <summary>
        /// 开启
        /// </summary>
        ON,
        /// <summary>
        /// 关闭
        /// </summary>
        OFF
    }
    /// <summary>
    /// 颜色模式
    /// </summary>
    enum YeelightColorMode
    {
        /// <summary>
        /// 彩色
        /// </summary>
        COLOR,
        /// <summary>
        /// 色温
        /// </summary>
        TEMPERATURE,
        /// <summary>
        /// HSV
        /// </summary>
        HSV
    }

    /// <summary>
    /// Yeelight对象
    /// </summary>
    class Yeelight
    {
        // 源设备信息头正则
        private static readonly Regex RAW_DEVICE_INFO_HEADER_REGEX = new Regex(@"^HTTP\/1\.1 200 OK\r\n");
        // 地址信息正则
        private static readonly Regex LOCATION_REGEX = new Regex(@"Location: yeelight://(.+):(.+)\r\n");
        // ID正则
        private static readonly Regex ID_REGEX = new Regex(@"id: (.+)\r\n");
        // 设备类型正则
        private static readonly Regex MODEL_REGEX = new Regex(@"model: (.+)\r\n");
        // 支持函数正则
        private static readonly Regex SUPPORT_REGEX = new Regex(@"support: (.+)\r\n");
        // 电源状态正则
        private static readonly Regex POWER_REGEX = new Regex(@"power: (.+)\r\n");
        // 亮度正则
        private static readonly Regex BRIGHT_REGEX = new Regex(@"bright: (.+)\r\n");
        // 颜色模式正则
        private static readonly Regex COLOR_MODE_REGEX = new Regex(@"color_mode: (.+)\r\n");
        // 色温正则
        private static readonly Regex COLOR_TEMPERATURE_REGEX = new Regex(@"ct: (.+)\r\n");
        // RGB正则
        private static readonly Regex RGB_REGEX = new Regex(@"rgb: (.+)\r\n");
        // 色调正则
        private static readonly Regex HUE_REGEX = new Regex(@"hue: (.+)\r\n");
        // 饱和度正则
        private static readonly Regex SAT_REGEX = new Regex(@"sat: (.+)\r\n");
        // 名字正则
        private static readonly Regex NAME_REGEX = new Regex(@"name: (.+)\r\n");

        // result正则
        private static readonly Regex RESULT_REGEX = new Regex("\"result\":" + @"\[" + "\"(.+)\"" + @"\]");

        // 私有属性
        private string id;
        private string ip;
        private string port;
        private YeelightModel model;
        private List<string> support;
        private YeelightPower power;
        private int bright;
        private YeelightColorMode color_mode;
        private int color_temperature;
        private int rgb;
        private int hue;
        private int sat;
        private string name;
        private StreamSocket tcpClient;

        /// <summary>
        /// 设备ID
        /// </summary>
        public string Id { get { return id; } }
        /// <summary>
        /// 设备IP地址
        /// </summary>
        public string Ip { get { return ip; } }
        /// <summary>
        /// 设备端口
        /// </summary>
        public string Port { get { return port; } }
        /// <summary>
        /// 设备类型（目前只有：mono白光、color彩光、stripeLED条）
        /// </summary>
        internal YeelightModel Model { get { return model; } }
        /// <summary>
        /// 电源状态
        /// </summary>
        internal YeelightPower Power { get { return power; } }
        /// <summary>
        /// 亮度 1-100
        /// </summary>
        public int Bright { get { return bright; } }
        /// <summary>
        /// 颜色模式（1:可设置颜色 2:色温模式 3: HSV模式）
        /// </summary>
        internal YeelightColorMode ColorMode { get { return color_mode; } }
        /// <summary>
        /// 色温（颜色模式为 "色温模式" 时才有效）
        /// </summary>
        public int ColorTemperature { get { return color_temperature; } }
        /// <summary>
        /// RGB
        /// </summary>
        public int RGB { get { return rgb; } }
        /// <summary>
        /// 色调
        /// </summary>
        public int HUE { get { return hue; } }
        /// <summary>
        /// 饱和度
        /// </summary>
        public int SAT { get { return sat; } }
        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get { return name; } }

        // 设备连接对象
        //private TcpClient tcpClient;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rawDevInfo">源设备信息</param>
        public Yeelight(string rawDevInfo)
        {
            Debug.WriteLine(rawDevInfo);

            // 验证合法性
            if (!VerifyRawDevInfo(rawDevInfo))
            {
                throw new Exception("源设备信息不合法");
            }

            // 解析并初始化设备信息
            ParseRawDeviceInfo(rawDevInfo);

            // 创建TCPClient
            this.tcpClient = new StreamSocket();
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
        private void ParseRawDeviceInfo(string rawDevInfo)
        {
            try
            {
                // 解析地址信息
                var matchLocation = LOCATION_REGEX.Match(rawDevInfo);
                this.ip = matchLocation.Groups[1].ToString();
                this.port = matchLocation.Groups[2].ToString();

                // 解析ID
                var matchID = ID_REGEX.Match(rawDevInfo);
                this.id = matchID.Groups[1].ToString();

                // 解析设备类型
                var matchModel = MODEL_REGEX.Match(rawDevInfo);
                string rawModel = matchModel.Groups[1].ToString();
                switch (rawModel)
                {
                    case "mono":
                        this.model = YeelightModel.MONO;
                        break;
                    case "color":
                        this.model = YeelightModel.COLOR;
                        break;
                    case "stripe":
                        this.model = YeelightModel.STRIPE;
                        break;
                }

                // 解析支持函数
                var matchSupport = SUPPORT_REGEX.Match(rawDevInfo);
                string rawSupport = matchSupport.Groups[1].ToString();
                this.support = rawSupport.Split(' ').ToList<string>();

                // 解析电源状态
                var matchPower = POWER_REGEX.Match(rawDevInfo);
                string rawPower = matchPower.Groups[1].ToString();
                this.power = rawPower == "on" ? YeelightPower.ON : YeelightPower.OFF;

                // 解析亮度
                var matchBright = BRIGHT_REGEX.Match(rawDevInfo);
                this.bright = Convert.ToInt32(matchBright.Groups[1].ToString());

                // 解析颜色模式
                var matchColorMode = COLOR_MODE_REGEX.Match(rawDevInfo);
                string rawColorMode = matchColorMode.Groups[1].ToString();
                switch (rawColorMode)
                {
                    case "1":
                        this.color_mode = YeelightColorMode.COLOR;
                        break;
                    case "2":
                        this.color_mode = YeelightColorMode.TEMPERATURE;
                        break;
                    case "3":
                        this.color_mode = YeelightColorMode.HSV;
                        break;
                }

                // 解析色温
                var matchColorTemperature = BRIGHT_REGEX.Match(rawDevInfo);
                this.color_temperature = Convert.ToInt32(matchColorTemperature.Groups[1].ToString());

                // 解析RGB
                var matchRGB = RGB_REGEX.Match(rawDevInfo);
                this.rgb = Convert.ToInt32(matchRGB.Groups[1].ToString());

                // 解析色调
                var matchHue = HUE_REGEX.Match(rawDevInfo);
                this.hue = Convert.ToInt32(matchHue.Groups[1].ToString());

                // 解析饱和度
                var matchSat = SAT_REGEX.Match(rawDevInfo);
                this.sat = Convert.ToInt32(matchSat.Groups[1].ToString());

                // 解析名字
                var matchName = NAME_REGEX.Match(rawDevInfo);
                this.name = matchName.Groups[1].ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private StreamSocket ConnectDevice()
        {

            // 连接
            await tcpClient.ConnectAsync(new HostName(this.ip), this.port);
        }

        public void Toggle()
        {
            // 命令
            byte[] buf = Encoding.ASCII.GetBytes("{\"id\": " + this.id + ", \"method\": \"toggle\", \"params\":[]}\r\n");
            // 发送
            this.tcpClient.Client.Send(buf);
            // 接收回应
            byte[] recv = new byte[1000];
            int len = this.tcpClient.Client.Receive(recv);

            // 转换
            string msg = Encoding.Default.GetString(recv.ToList().GetRange(0, len).ToArray());

            Debug.WriteLine(msg);

            // 处理
            var resultGroups = RESULT_REGEX.Match(msg).Groups;
            string result = resultGroups[1].ToString();

            return result == "ok" ? true : false;
        }
    }
}
