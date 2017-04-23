using ConfigStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;
using YeelightAPI;

namespace CortanaService
{
    /// <summary>
    /// Yeelight灯泡语音命令服务
    /// </summary>
    public sealed class YeelightVoiceCommandService : IBackgroundTask
    {
        // 后台任务名
        private readonly string TRIGGER_DETAIL_NAME = "YeelightVoiceCommandService";

        // 后台任务延迟
        private BackgroundTaskDeferral serviceDeferral;
        // 语音服务连接
        private VoiceCommandServiceConnection voiceServiceConnection;

        // 后台任务入口
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // 获取后台任务相关的详细信息
            var triggerDetails = (AppServiceTriggerDetails)taskInstance.TriggerDetails;

            // 保存后台任务延迟实例
            this.serviceDeferral = taskInstance.GetDeferral();

            // 无法获取后台任务或非本后台任务
            if (triggerDetails == null || triggerDetails.Name != TRIGGER_DETAIL_NAME)
            {
                return;
            }

            try
            {
                // 获取语音服务连接
                this.voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                // 语音命令完成事件
                this.voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                // 获取当前的语音命令
                VoiceCommand voiceCommand = await this.voiceServiceConnection.GetVoiceCommandAsync();

                // 命令名
                string commandName = voiceCommand.CommandName;
                // 指令
                string say = voiceCommand.Properties.ContainsKey("Say") ? voiceCommand.Properties["Say"][0] : "";
                // 回答
                string answer = "已为您执行";

                if (commandName != "Action" || string.IsNullOrEmpty(say))
                {
                    await Response("小娜听不懂");
                    return;
                }

                // 读取配置
                var configStorage = new JsonConfigStorage();
                // 加载配置
                await configStorage.LoadAsync();

                // 初始化设备
                var devices = configStorage.GetDevices();
                var deviceDict = new Dictionary<string, Yeelight>();
                foreach (var item in devices)
                    deviceDict.Add(item.Id, new Yeelight(item.RawDeviceInfo));

                // 初始化分组
                var groups = configStorage.GetGroups();
                var groupDict = new Dictionary<string, List<Yeelight>>();
                foreach (var item in groups)
                {
                    // 组建设备列表
                    var deviceList = new List<Yeelight>();

                    foreach (var deviceId in item.Devices)
                        if (deviceDict.ContainsKey(deviceId))
                            deviceList.Add(deviceDict[deviceId]);

                    groupDict.Add(item.Id, deviceList);
                }

                // 获取相关命令指向
                var vcss = configStorage.GetVoiceCommandSets();
                var vcsList = new List<ConfigStorage.Entiry.VoiceCommandSet>();

                foreach (var vcs in vcss)
                {
                    foreach (var vc in vcs.VoiceCommands)
                    {
                        if (vc.Say == say)
                        {
                            vcsList.Add(vcs);

                            // 设置回答
                            answer = vc.Answer;

                            break;
                        }
                    }
                }

                if (vcsList.Count == 0)
                {
                    await Response("小娜听不懂");
                    return;
                }

                // 执行命令
                List<Task> taskList = new List<Task>();
                foreach (var vcs in vcsList)
                {
                    // 全部
                    if (vcs.IsAll)
                    {
                        foreach (var device in deviceDict.Values)
                            taskList.Add(DeviceAction(vcs.Action, vcs.ActionParams, device));
                    }
                    // device
                    if (!string.IsNullOrEmpty(vcs.DeviceId))
                    {
                        if (deviceDict.ContainsKey(vcs.DeviceId))
                        {
                            taskList.Add(DeviceAction(vcs.Action, vcs.ActionParams, deviceDict[vcs.DeviceId]));
                        }
                    }
                    // group
                    if (!string.IsNullOrEmpty(vcs.GroupId))
                    {
                        if (groupDict.ContainsKey(vcs.GroupId))
                        {
                            foreach (var device in groupDict[vcs.GroupId])
                                taskList.Add(DeviceAction(vcs.Action, vcs.ActionParams, device));
                        }
                    }
                }

                // 等待所有完成
                await Task.WhenAll(taskList.ToArray());
                // 回应
                await Response(answer);
            }
            catch (Exception ex)
            {
                await Log(string.Format("{0}\n{1}\n\n", ex.Message, ex.StackTrace));
            }
            finally
            {
                if (this.serviceDeferral != null)
                    this.serviceDeferral.Complete();
            }
        }

        // 语音命令完成事件
        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
                this.serviceDeferral.Complete();
        }

        /// <summary>
        /// 回应
        /// </summary>
        private async Task Response(string msg)
        {
            // 声明用户消息
            var userMessage = new VoiceCommandUserMessage();
            userMessage.DisplayMessage = userMessage.SpokenMessage = msg;

            // 声明语音回应
            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userMessage);

            // 回应小娜
            await voiceServiceConnection.ReportSuccessAsync(response);
        }
        /// <summary>
        /// 对设备进行操作
        /// </summary>
        /// <param name="type">操作类型</param>
        /// <param name="param">参数</param>
        /// <param name="device">设备</param>
        private async Task DeviceAction(string type, string param, Yeelight device)
        {
            try
            {
                switch (Enum.Parse(typeof(ActionType), type))
                {
                    case ActionType.PowerOn:
                        await device.SetPower(YeelightPower.on);
                        break;
                    case ActionType.PowerOff:
                        await device.SetPower(YeelightPower.off);
                        break;
                    case ActionType.BrightUp:
                        await device.SetBright(Math.Min(100, device.Bright + Convert.ToInt32(param)));
                        break;
                    case ActionType.BrightDown:
                        await device.SetBright(Math.Max(1, device.Bright - Convert.ToInt32(param)));
                        break;
                    case ActionType.SwitchColor:
                        var hsv = param.Split(',');
                        await device.SetHSV(Convert.ToInt32(Convert.ToDouble(hsv[0])), Convert.ToInt32(Convert.ToDouble(hsv[1]) * 100));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await Log(string.Format("{0}\n{1}\n\n", ex.Message, ex.StackTrace));
            }
        }

        private async Task Log(string msg)
        {
            // 本地文件夹
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            // 设置文件
            var settingFile = await tempFolder.CreateFileAsync("log.json", CreationCollisionOption.OpenIfExists);
            await FileIO.AppendTextAsync(settingFile, msg);
        }
    }
}