using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace YeelightAPI
{
    /// <summary>
    /// Yeelight颜色模式
    /// </summary>
    public enum YeelightModel
    {
        /// <summary>
        /// 白光
        /// </summary>
        mono,
        /// <summary>
        /// 彩光
        /// </summary>
        color,
        /// <summary>
        /// 流光
        /// </summary>
        stripe
    }
    /// <summary>
    /// Yeelight电源状态
    /// </summary>
    public enum YeelightPower
    {
        /// <summary>
        /// 开启
        /// </summary>
        on,
        /// <summary>
        /// 关闭
        /// </summary>
        off
    }
    /// <summary>
    /// Yeelight颜色模式
    /// </summary>
    public enum YeelightColorMode
    {
        /// <summary>
        /// 彩色
        /// </summary>
        color,
        /// <summary>
        /// 色温
        /// </summary>
        temperature,
        /// <summary>
        /// HSV
        /// </summary>
        hsv
    }
    /// <summary>
    /// Yeelight函数
    /// </summary>
    public enum YeelightMethod
    {
        /// <summary>
        /// 获取属性值
        /// </summary>
        get_prop,
        /// <summary>
        /// 设置色温
        /// </summary>
        set_ct_abx,
        /// <summary>
        /// 设置RGB颜色
        /// </summary>
        set_rgb,
        /// <summary>
        /// 设置HSV颜色
        /// </summary>
        set_hsv,
        /// <summary>
        /// 设置亮度
        /// </summary>
        set_bright,
        /// <summary>
        /// 设置电源状态
        /// </summary>
        set_power,
        /// <summary>
        /// 开/关电源
        /// </summary>
        toggle,
        /// <summary>
        /// 保存当前状态
        /// </summary>
        set_default,
        /// <summary>
        /// 开始流光模式
        /// </summary>
        start_cf,
        /// <summary>
        /// 停止流光模式
        /// </summary>
        stop_cf,
        /// <summary>
        /// 设置场景
        /// </summary>
        set_scene,
        /// <summary>
        /// 添加定时任务
        /// </summary>
        cron_add,
        /// <summary>
        /// 获取定时任务
        /// </summary>
        cron_get,
        /// <summary>
        /// 删除定时任务
        /// </summary>
        cron_del,
        /// <summary>
        /// 调整？不明函数
        /// </summary>
        set_adjust,
        /// <summary>
        /// 设置音乐模式
        /// </summary>
        set_music,
        /// <summary>
        /// 设置名字
        /// </summary>
        set_name
    }
    /// <summary>
    /// Yeelight效果转变方式
    /// </summary>
    public enum YeelightTransformEffect
    {
        /// <summary>
        /// 突然
        /// </summary>
        sudden,
        /// <summary>
        /// 平滑
        /// </summary>
        smooth
    }

    /// <summary>
    /// Yeelight对象
    /// </summary>
    public sealed class Yeelight
    {
        #region 正则
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

        //// result正则
        //private static readonly Regex RESULT_REGEX = new Regex("\"result\":" + @"\[" + "\"(.+)\"" + @"\]");
        #endregion

        #region 私有属性
        private string id;
        private string ip;
        private string port;
        private YeelightModel model;
        private Dictionary<string, bool> support;
        private YeelightPower power;
        private int bright;
        private YeelightColorMode color_mode;
        private int ct;
        private int rgb;
        private int r;
        private int g;
        private int b;
        private int hue;
        private int sat;
        private string name;
        private StreamSocket tcpClient;
        #endregion

        #region 暴露属性
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
        public YeelightModel Model { get { return model; } }
        /// <summary>
        /// 电源状态
        /// </summary>
        public YeelightPower Power { get { return power; } }
        /// <summary>
        /// 亮度 1-100
        /// </summary>
        public int Bright { get { return bright; } }
        /// <summary>
        /// 颜色模式（1:可设置颜色 2:色温模式 3: HSV模式）
        /// </summary>
        public YeelightColorMode ColorMode { get { return color_mode; } }
        /// <summary>
        /// 色温（颜色模式为 "色温模式" 时才有效）
        /// </summary>
        public int ColorTemperature { get { return ct; } }
        /// <summary>
        /// RGB 十进制
        /// </summary>
        public int RGB { get { return rgb; } }
        /// <summary>
        /// R
        /// </summary>
        public int R { get { return r; } }
        /// <summary>
        /// G
        /// </summary>
        public int G { get { return g; } }
        /// <summary>
        /// B
        /// </summary>
        public int B { get { return b; } }
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
        #endregion

        #region 构造函数
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

            // 解析并初始化设备信息
            ParseRawDeviceInfo(rawDevInfo);
        }
        #endregion

        #region 公共函数
        /// <summary>
        /// 开关灯
        /// </summary>
        /// <returns>是否成功</returns>
        public IAsyncOperation<bool> ToggleAsync()
        {
            return this.ToggleHelper().AsAsyncOperation();
        }
        /// <summary>
        /// 设置色温
        /// </summary>
        /// <param name="ct">色温 1700 - 6500</param>
        /// <returns>是否成功</returns>
        public IAsyncOperation<bool> SetColorTemperatureAsync(int ct)
        {
            return this.SetColorTemperatureHelper(ct).AsAsyncOperation();
        }
        /// <summary>
        /// 设置HSV私有函数
        /// </summary>
        /// <param name="h">色相 0 - 359</param>
        /// <param name="s">饱和度 0 - 100</param>
        /// <param name="v">亮度 1 - 100</param>
        /// <returns>是否成功</returns>
        public IAsyncOperation<bool> SetHSV(int h, int s, int v)
        {
            return this.SetHSVHelper(h, s, v).AsAsyncOperation();
        }
        /// <summary>
        /// 设置亮度
        /// </summary>
        /// <param name="bright">亮度 1 - 100</param>
        /// <returns>是否成功</returns>
        public IAsyncOperation<bool> SetBright(int bright)
        {
            return this.SetBrightHelper(bright).AsAsyncOperation();
        }
        /// <summary>
        /// 设置电源状态
        /// </summary>
        /// <param name="power">电源状态</param>
        /// <returns>是否成功</returns>
        public IAsyncOperation<bool> SetPower(YeelightPower power)
        {
            return this.SetPowerHelper(power).AsAsyncOperation();
        }
        /// <summary>
        /// 设置设备名称
        /// </summary>
        /// <param name="name">设备名称</param>
        /// <returns>是否成功</returns>
        public IAsyncOperation<bool> SetDeviceName(string name)
        {
            return this.SetDeviceNameHelper(name).AsAsyncOperation();
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(this.name) ? this.id : this.name;
        }
        #endregion

        #region 私有函数

        #region 通信相关函数
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
                        this.model = YeelightModel.mono;
                        break;
                    case "color":
                        this.model = YeelightModel.color;
                        break;
                    case "stripe":
                        this.model = YeelightModel.stripe;
                        break;
                }

                // 解析支持函数
                var matchSupport = SUPPORT_REGEX.Match(rawDevInfo);
                string rawSupport = matchSupport.Groups[1].ToString();
                this.support = rawSupport.Split(' ').ToDictionary(k => k, v => true);

                // 解析电源状态
                var matchPower = POWER_REGEX.Match(rawDevInfo);
                string rawPower = matchPower.Groups[1].ToString();
                this.power = rawPower == "on" ? YeelightPower.on : YeelightPower.off;

                // 解析亮度
                var matchBright = BRIGHT_REGEX.Match(rawDevInfo);
                this.bright = Convert.ToInt32(matchBright.Groups[1].ToString());

                // 解析颜色模式
                var matchColorMode = COLOR_MODE_REGEX.Match(rawDevInfo);
                string rawColorMode = matchColorMode.Groups[1].ToString();
                switch (rawColorMode)
                {
                    case "1":
                        this.color_mode = YeelightColorMode.color;
                        break;
                    case "2":
                        this.color_mode = YeelightColorMode.temperature;
                        break;
                    case "3":
                        this.color_mode = YeelightColorMode.hsv;
                        break;
                }

                // 解析色温
                var matchColorTemperature = BRIGHT_REGEX.Match(rawDevInfo);
                this.ct = Convert.ToInt32(matchColorTemperature.Groups[1].ToString());

                // 解析RGB
                var matchRGB = RGB_REGEX.Match(rawDevInfo);
                this.rgb = Convert.ToInt32(matchRGB.Groups[1].ToString());
                // RGB十六进制转换
                var rgb16 = Convert.ToString(this.rgb, 16);
                rgb16 = rgb16 == "0" ? "000000" : rgb16;
                // 单独保存
                this.r = Convert.ToInt32(rgb16.Substring(0, 2), 16);
                this.g = Convert.ToInt32(rgb16.Substring(2, 2), 16);
                this.b = Convert.ToInt32(rgb16.Substring(4, 2), 16);

                // 解析色调
                var matchHue = HUE_REGEX.Match(rawDevInfo);
                this.hue = Convert.ToInt32(matchHue.Groups[1].ToString());

                // 解析饱和度
                var matchSat = SAT_REGEX.Match(rawDevInfo);
                this.sat = Convert.ToInt32(matchSat.Groups[1].ToString());

                // 解析名字
                var matchName = NAME_REGEX.Match(rawDevInfo);

                try
                {
                    this.name = Encoding.UTF8.GetString(Convert.FromBase64String(matchName.Groups[1].ToString()));
                }
                catch (Exception)
                {
                    this.name = null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 连接到设备
        /// </summary>
        /// <returns></returns>
        private async Task ConnectDeviceAsync()
        {
            // 断开连接
            this.DisconnectDevice();

            // 创建TCPClient
            this.tcpClient = new StreamSocket();
            var a = new StreamSocketListener();

            // 连接设备
            await this.tcpClient.ConnectAsync(new HostName(this.ip), this.port);
        }
        /// <summary>
        /// 断开设备连接
        /// </summary>
        /// <returns></returns>
        private void DisconnectDevice()
        {
            if (this.tcpClient != null)
            {
                this.tcpClient.Dispose();
                this.tcpClient = null;
            }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>回应数据</returns>
        private async Task<string> SendDataAsync(string data)
        {
            // 结果
            string result = "";

            // 连接设备
            await this.ConnectDeviceAsync();

            // 创建写入对象
            using (DataWriter dw = new DataWriter(this.tcpClient.OutputStream))
            {
                // 数据处理
                data = data.Replace("\r\n", "") + "\r\n";
                // 写入缓冲区
                dw.WriteString(data);
                // 发送数据
                await dw.StoreAsync();
                // 分离流
                dw.DetachStream();
            }

            // 创建读取对象
            using (DataReader dr = new DataReader(this.tcpClient.InputStream))
            {
                // 设置为有数据可用就往下执行
                dr.InputStreamOptions = InputStreamOptions.Partial;
                // 一次读1024字节
                await dr.LoadAsync(1024);
                // 保存结果
                result = dr.ReadString(dr.UnconsumedBufferLength);

                // 分离流
                dr.DetachStream();
            }

            // 断开连接
            this.DisconnectDevice();

            return result;
        }
        #endregion

        #region 包装设备函数
        /// <summary>
        /// 更新设备信息
        /// </summary>
        /// <returns></returns>
        private async Task UpdateDeviceInfo()
        {
            // 获取设备数据
            Dictionary<string, object> propList = await this.Device_get_prop_Async();

            if (propList["power"] != null)
                this.power = (YeelightPower)propList["power"];
            if (propList["bright"] != null)
                this.bright = (int)propList["bright"];
            if (propList["color_mode"] != null)
                this.color_mode = (YeelightColorMode)propList["color_mode"];
            if (propList["ct"] != null)
                this.ct = (int)propList["ct"];
            if (propList["rgb"] != null)
            {
                this.rgb = (int)propList["rgb"];
                this.r = (int)propList["r"];
                this.g = (int)propList["g"];
                this.b = (int)propList["b"];
            }
            if (propList["hue"] != null)
                this.hue = (int)propList["hue"];
            if (propList["sat"] != null)
                this.sat = (int)propList["sat"];
            if (propList["name"] != null)
                this.name = (string)propList["name"];
        }
        /// <summary>
        /// 开关灯私有函数
        /// </summary>
        /// <returns>是否成功</returns>
        private async Task<bool> ToggleHelper()
        {
            // 是否成功
            bool isSuccess = await this.Device_toggle_Async();
            // 更新设备信息
            await this.UpdateDeviceInfo();

            return isSuccess;
        }
        /// <summary>
        /// 设置色温私有函数
        /// </summary>
        /// <param name="ct">色温 1700 - 6500</param>
        /// <returns>是否成功</returns>
        private async Task<bool> SetColorTemperatureHelper(int ct)
        {
            // 格式处理
            ct = ct > 6500 ? 6500 : ct;
            ct = ct < 1700 ? 1700 : ct;

            // 是否成功
            bool isSuccess = await this.Device_set_ct_abx_Async(ct, YeelightTransformEffect.smooth, 300);
            // 更新设备信息
            await this.UpdateDeviceInfo();

            return isSuccess;
        }
        /// <summary>
        /// 设置HSV私有函数
        /// </summary>
        /// <param name="h">色相 0 - 359</param>
        /// <param name="s">饱和度 0 - 100</param>
        /// <param name="v">亮度 1 - 100</param>
        /// <returns>是否成功</returns>
        private async Task<bool> SetHSVHelper(int h, int s, int v)
        {
            // 格式处理
            h = h > 359 ? 359 : h;
            h = h < 0 ? 0 : h;
            s = s > 100 ? 100 : s;
            s = s < 0 ? 0 : s;
            v = v > 100 ? 100 : v;
            v = v < 1 ? 1 : v;

            // 是否成功
            bool isSuccess = await this.Device_set_hsv_Async(h, s, YeelightTransformEffect.smooth, 150);
            isSuccess = isSuccess ? await this.Device_set_bright_Async(v, YeelightTransformEffect.smooth, 150) : isSuccess;

            // 更新设备信息
            await this.UpdateDeviceInfo();

            return isSuccess;
        }
        /// <summary>
        /// 设置亮度私有函数
        /// </summary>
        /// <param name="bright">亮度 1 - 100</param>
        /// <returns>是否成功</returns>
        private async Task<bool> SetBrightHelper(int bright)
        {
            // 格式处理
            bright = bright > 100 ? 100 : bright;
            bright = bright < 1 ? 1 : bright;

            // 是否成功
            bool isSuccess = await this.Device_set_bright_Async(bright, YeelightTransformEffect.smooth, 300);
            // 更新设备信息
            await this.UpdateDeviceInfo();

            return isSuccess;
        }
        /// <summary>
        /// 设置电源状态私有函数
        /// </summary>
        /// <param name="power">电源状态</param>
        /// <returns>是否成功</returns>
        private async Task<bool> SetPowerHelper(YeelightPower power)
        {
            // 是否成功
            bool isSuccess = await this.Device_set_power_Async(power, YeelightTransformEffect.smooth, 300);
            // 更新设备信息
            await this.UpdateDeviceInfo();

            return isSuccess;
        }
        /// <summary>
        /// 设置设备名称私有函数
        /// </summary>
        /// <param name="name">设备名称</param>
        /// <returns>是否成功</returns>
        private async Task<bool> SetDeviceNameHelper(string name)
        {
            // 是否成功
            bool isSuccess = await this.Device_set_name_Async(name);
            // 更新设备信息
            await this.UpdateDeviceInfo();

            return isSuccess;
        }
        #endregion

        #region 设备函数
        /// <summary>
        /// 设备是否支持该函数
        /// </summary>
        /// <param name="method">函数名</param>
        private void DeviceIsSupportFunc(string method)
        {
            // 判断是否支持该函数
            if (!this.support.ContainsKey(method))
            {
                throw new Exception("不支持该函数");
            }
        }
        /// <summary>
        /// 设备请求构建器
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="param">参数</param>
        /// <returns>请求文本</returns>
        private string DeviceRequestBuildHelper(string method, JArray param)
        {
            JObject data = new JObject();
            data["id"] = this.id;
            data["method"] = method;
            data["params"] = param;

            return data.ToString();
        }
        /// <summary>
        /// 设备回应是否成功
        /// </summary>
        /// <returns>是否成功</returns>
        private bool DeviceResponseIsSuccess(string resp)
        {
            // JSON转换
            JObject json = JObject.Parse(resp);

            if (json["result"] == null || json["result"][0] == null)
            {
                return false;
            }

            return json["result"][0].ToString() == "ok" ? true : false;
        }

        /// <summary>
        /// 获取设备各属性值（不支持属性或未设置属性为空值）
        /// </summary>
        /// <returns>属性与值</returns>
        private async Task<Dictionary<string, object>> Device_get_prop_Async()
        {
            // 函数名
            string method = YeelightMethod.get_prop.ToString();

            // 判断是否支持该函数 不支持直接抛异常
            this.DeviceIsSupportFunc(method);

            // 组建参数
            JArray param = new JArray();
            param.Add("power");
            param.Add("bright");
            param.Add("color_mode");
            param.Add("ct");
            param.Add("rgb");
            param.Add("hue");
            param.Add("sat");
            param.Add("name");

            // 接收回应
            JObject res = JObject.Parse(await this.SendDataAsync(this.DeviceRequestBuildHelper(method, param)));

            // 结果
            Dictionary<string, object> result = new Dictionary<string, object>();

            // 电源状态
            if (!string.IsNullOrEmpty(res["result"][0].ToString()))
                result.Add("power", res["result"][0].ToString() == "on" ? YeelightPower.on : YeelightPower.off);
            else
                result.Add("power", null);

            // 亮度
            if (!string.IsNullOrEmpty(res["result"][1].ToString()))
                result.Add("bright", Convert.ToInt32(res["result"][1].ToString()));
            else
                result.Add("bright", null);

            // 颜色模式
            string colorMode = res["result"][2].ToString();
            switch (colorMode)
            {
                case "1":
                    result.Add("color_mode", YeelightColorMode.color);
                    break;
                case "2":
                    result.Add("color_mode", YeelightColorMode.temperature);
                    break;
                case "3":
                    result.Add("color_mode", YeelightColorMode.hsv);
                    break;
                default:
                    result.Add("color_mode", null);
                    break;
            }

            // 色温
            if (!string.IsNullOrEmpty(res["result"][3].ToString()))
                result.Add("ct", Convert.ToInt32(res["result"][3].ToString()));
            else
                result.Add("ct", null);

            // RGB
            if (!string.IsNullOrEmpty(res["result"][4].ToString()))
            {
                // 解析RGB
                var rgb = Convert.ToInt32(res["result"][4].ToString());
                // RGB十六进制转换
                var rgb16 = Convert.ToString(rgb, 16);
                rgb16 = rgb16 == "0" ? "000000" : rgb16;
                // 单独保存
                result.Add("rgb", rgb);
                result.Add("r", Convert.ToInt32(rgb16.Substring(0, 2), 16));
                result.Add("g", Convert.ToInt32(rgb16.Substring(2, 2), 16));
                result.Add("b", Convert.ToInt32(rgb16.Substring(4, 2), 16));
            }
            else
            {
                result.Add("rgb", null);
                result.Add("r", null);
                result.Add("g", null);
                result.Add("b", null);
            }

            // 色调
            if (!string.IsNullOrEmpty(res["result"][5].ToString()))
                result.Add("hue", Convert.ToInt32(res["result"][5].ToString()));
            else
                result.Add("hue", null);

            // 饱和度
            if (!string.IsNullOrEmpty(res["result"][6].ToString()))
                result.Add("sat", Convert.ToInt32(res["result"][6].ToString()));
            else
                result.Add("sat", null);

            // 名字
            if (!string.IsNullOrEmpty(res["result"][7].ToString()))
            {
                string name;

                try
                {
                    name = res["result"][7].ToString();
                    name = Encoding.UTF8.GetString(Convert.FromBase64String(name));
                }
                catch (Exception)
                {
                    name = null;
                }

                result.Add("name", name);
            }
            else
                result.Add("name", null);

            return result;
        }
        /// <summary>
        /// 设置色温
        /// </summary>
        /// <param name="color_temperature">色温 1700 - 6500</param>
        /// <param name="effect">渐变效果</param>
        /// <param name="duration">渐变时长</param>
        /// <returns>是否成功</returns>
        private async Task<bool> Device_set_ct_abx_Async(int color_temperature, YeelightTransformEffect effect, int duration)
        {
            // 函数名
            string method = YeelightMethod.set_ct_abx.ToString();

            // 判断是否支持该函数 不支持直接抛异常
            this.DeviceIsSupportFunc(method);

            // 参数错误
            if (color_temperature > 6500
                || color_temperature < 1700
                || (effect == YeelightTransformEffect.smooth && duration < 30))
            {
                throw new Exception("参数错误");
            }

            // 效果是突然时不需要渐变时间
            if (effect == YeelightTransformEffect.sudden)
                duration = 0;

            // 组建参数
            JArray param = new JArray();
            param.Add(color_temperature);
            param.Add(effect.ToString());
            param.Add(duration);

            // 接收回应
            string resp = await this.SendDataAsync(this.DeviceRequestBuildHelper(method, param));

            return this.DeviceResponseIsSuccess(resp);
        }
        /// <summary>
        /// 设置RGB RGB不能都为0
        /// </summary>
        /// <param name="r">红色 0 - 255</param>
        /// <param name="g">绿色 0 - 255</param>
        /// <param name="b">蓝色 0 - 255</param>
        /// <param name="effect">渐变效果</param>
        /// <param name="duration">渐变时长</param>
        /// <returns>是否成功</returns>
        private async Task<bool> Device_set_rgb_Async(int r, int g, int b, YeelightTransformEffect effect, int duration)
        {
            // 函数名
            string method = YeelightMethod.set_rgb.ToString();

            // 判断是否支持该函数 不支持直接抛异常
            this.DeviceIsSupportFunc(method);

            // 参数错误
            if (r > 255
                || r < 0
                || g > 255
                || g < 0
                || b > 255
                || b < 0
                || (r == 0 && g == 0 && b == 0)
                || (effect == YeelightTransformEffect.smooth && duration < 30))
            {
                throw new Exception("参数错误");
            }

            // 效果是突然时不需要渐变时间
            if (effect == YeelightTransformEffect.sudden)
                duration = 0;

            // 组建参数
            JArray param = new JArray();
            param.Add((r * 65536) + (g * 256) + b);
            param.Add(effect.ToString());
            param.Add(duration);

            // 接收回应
            string resp = await this.SendDataAsync(this.DeviceRequestBuildHelper(method, param));

            return this.DeviceResponseIsSuccess(resp);
        }
        /// <summary>
        /// 设置HSV
        /// </summary>
        /// <param name="h">色相 0 - 359</param>
        /// <param name="s">饱和度 0 - 100</param>
        /// <param name="effect">渐变效果</param>
        /// <param name="duration">渐变时长</param>
        /// <returns></returns>
        private async Task<bool> Device_set_hsv_Async(int h, int s, YeelightTransformEffect effect, int duration)
        {
            // 函数名
            string method = YeelightMethod.set_hsv.ToString();

            // 判断是否支持该函数 不支持直接抛异常
            this.DeviceIsSupportFunc(method);

            // 参数错误
            if (h > 359
                || h < 0
                || s > 100
                || s < 0
                || (effect == YeelightTransformEffect.smooth && duration < 30))
            {
                throw new Exception("参数错误");
            }

            // 效果是突然时不需要渐变时间
            if (effect == YeelightTransformEffect.sudden)
                duration = 0;

            // 组建参数
            JArray param = new JArray();
            param.Add(h);
            param.Add(s);
            param.Add(effect.ToString());
            param.Add(duration);

            // 接收回应
            string resp = await this.SendDataAsync(this.DeviceRequestBuildHelper(method, param));

            return this.DeviceResponseIsSuccess(resp);
        }
        /// <summary>
        /// 设置亮度
        /// </summary>
        /// <param name="bright">亮度 1 - 100</param>
        /// <param name="effect">渐变效果</param>
        /// <param name="duration">渐变时长</param>
        /// <returns></returns>
        private async Task<bool> Device_set_bright_Async(int bright, YeelightTransformEffect effect, int duration)
        {
            // 函数名
            string method = YeelightMethod.set_bright.ToString();

            // 判断是否支持该函数 不支持直接抛异常
            this.DeviceIsSupportFunc(method);

            // 参数错误
            if (bright > 100
                || bright < 1
                || (effect == YeelightTransformEffect.smooth && duration < 30))
            {
                throw new Exception("参数错误");
            }

            // 效果是突然时不需要渐变时间
            if (effect == YeelightTransformEffect.sudden)
                duration = 0;

            // 组建参数
            JArray param = new JArray();
            param.Add(bright);
            param.Add(effect.ToString());
            param.Add(duration);

            // 接收回应
            string resp = await this.SendDataAsync(this.DeviceRequestBuildHelper(method, param));

            return this.DeviceResponseIsSuccess(resp);
        }
        /// <summary>
        /// 设置电源状态
        /// </summary>
        /// <param name="power">电源状态</param>
        /// <param name="effect">渐变效果</param>
        /// <param name="duration">渐变时长</param>
        /// <returns></returns>
        private async Task<bool> Device_set_power_Async(YeelightPower power, YeelightTransformEffect effect, int duration)
        {
            // 函数名
            string method = YeelightMethod.set_power.ToString();

            // 判断是否支持该函数 不支持直接抛异常
            this.DeviceIsSupportFunc(method);

            // 参数错误
            if (effect == YeelightTransformEffect.smooth && duration < 30)
            {
                throw new Exception("参数错误");
            }

            // 效果是突然时不需要渐变时间
            if (effect == YeelightTransformEffect.sudden)
                duration = 0;

            // 组建参数
            JArray param = new JArray();
            param.Add(power.ToString());
            param.Add(effect.ToString());
            param.Add(duration);

            // 接收回应
            string resp = await this.SendDataAsync(this.DeviceRequestBuildHelper(method, param));

            return this.DeviceResponseIsSuccess(resp);
        }
        /// <summary>
        /// 开/关电源
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Device_toggle_Async()
        {
            // 函数名
            string method = YeelightMethod.toggle.ToString();

            // 判断是否支持该函数 不支持直接抛异常
            this.DeviceIsSupportFunc(method);

            // 组建参数
            JArray param = new JArray();

            // 接收回应
            string resp = await this.SendDataAsync(this.DeviceRequestBuildHelper(method, param));

            return this.DeviceResponseIsSuccess(resp);
        }
        /// <summary>
        /// 设置设备名称
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        private async Task<bool> Device_set_name_Async(string name)
        {
            // 函数名
            string method = YeelightMethod.set_name.ToString();

            // 判断是否支持该函数 不支持直接抛异常
            this.DeviceIsSupportFunc(method);

            // 参数错误
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("参数错误");
            }

            // base64转换
            name = Convert.ToBase64String(Encoding.UTF8.GetBytes(name));

            // 组建参数
            JArray param = new JArray();
            param.Add(name);

            // 接收回应
            string resp = await this.SendDataAsync(this.DeviceRequestBuildHelper(method, param));

            return this.DeviceResponseIsSuccess(resp);
        }
        #endregion
    }
    #endregion
}
