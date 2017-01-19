using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                // 声明用户信息（用于回应）
                var userMessage = new VoiceCommandUserMessage();

                // 命令名
                string commandName = voiceCommand.CommandName;
                // 获取电灯列表
                var lightList = await YeelightUtils.SearchDeviceAsync();

                if (lightList.Count == 0)
                    userMessage.DisplayMessage = userMessage.SpokenMessage = "没有找到可操作的电灯";
                else
                {
                    // 命令索引
                    int index = Convert.ToInt32(voiceCommand.CommandName);
                    // 读取设置
                    JObject config = await SettingHelper.LoadSetting();
                    // 命令列表
                    JArray commands = (JArray)config["commands"];
                    // 当前的命令
                    JObject command = (JObject)commands[index];
                    // 对应的设备ID
                    JArray deviceIds = (JArray)command["deviceIds"];
                    // 动作
                    LightAction action = (LightAction)Enum.Parse(typeof(LightAction), command["action"]["action"].ToString());
                    // 操作设备列表
                    List<Yeelight> deviceList = new List<Yeelight>();

                    // 找到对应设备
                    foreach (string item in deviceIds)
                    {
                        var light = lightList.First(k => item == k.Id);

                        if (light == null)
                            return;

                        deviceList.Add(light);
                    }

                    // 遍历执行操作
                    foreach (var item in deviceList)
                    {
                        int bright;

                        switch (action)
                        {
                            case LightAction.PowerOn:
                                await item.SetPower(YeelightPower.on);
                                break;
                            case LightAction.PowerOff:
                                await item.SetPower(YeelightPower.off);
                                break;
                            case LightAction.BrightUp:
                                bright = Convert.ToInt32(command["action"]["value"].ToString());
                                bright = bright + item.Bright;
                                bright = bright > 100 ? 100 : bright;
                                bright = bright < 1 ? 1 : bright;
                                await item.SetBright(bright);
                                break;
                            case LightAction.BrightDown:
                                bright = Convert.ToInt32(command["action"]["value"].ToString());
                                bright = item.Bright - bright;
                                bright = bright > 100 ? 100 : bright;
                                bright = bright < 1 ? 1 : bright;
                                await item.SetBright(bright);
                                break;
                        }
                    }
                }

                // 声明语音回应
                VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userMessage);
                // 回应小娜
                await voiceServiceConnection.ReportSuccessAsync(response);
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
    }
}