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
        private BackgroundTaskDeferral serviceDeferral;
        VoiceCommandServiceConnection voiceServiceConnection;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            this.serviceDeferral = taskInstance.GetDeferral();

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);

            VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

            IList<Yeelight> yeelightList = await YeelightUtils.SearchDeviceAsync(2000);
            var userMessage = new VoiceCommandUserMessage();
            VoiceCommandResponse response;

            int index = Convert.ToInt32(voiceCommand.CommandName);

            var localFolder = ApplicationData.Current.LocalFolder;
            var configFile = await localFolder.CreateFileAsync(@"setting.json", CreationCollisionOption.OpenIfExists);
            JArray commands;

            using (var stream = new StreamReader(await configFile.OpenStreamForReadAsync()))
            {
                string data = await stream.ReadToEndAsync();
                commands = (JArray)JObject.Parse(data)["commands"];
            }


            JObject command = (JObject)commands[index];
            JArray deviceIds = (JArray)command["deviceIds"];
            LightAction action = (LightAction)Enum.Parse(typeof(LightAction), command["action"]["action"].ToString());
            List<Yeelight> deviceList = new List<Yeelight>();

            foreach (string item in deviceIds)
            {
                var light = yeelightList.First(k => item == k.Id);

                if (light == null)
                    return;

                deviceList.Add(light);
            }

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

            //userMessage.DisplayMessage = String.Format("已为你打开{0}盏电灯", yeelightList.Count);
            //userMessage.SpokenMessage = userMessage.DisplayMessage;

            response = VoiceCommandResponse.CreateResponse(userMessage);

            await voiceServiceConnection.ReportSuccessAsync(response);
            response = VoiceCommandResponse.CreateResponse(userMessage);

            await voiceServiceConnection.ReportSuccessAsync(response);
        }
    }
}