﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
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

            IList<Yeelight> yeelightList = await YeelightUtils.SearchDeviceAsync();
            var userMessage = new VoiceCommandUserMessage();
            VoiceCommandResponse response;

            switch (voiceCommand.CommandName)
            {
                case "openPower":
                    foreach (var item in yeelightList)
                    {
                        await item.ToggleAsync();
                    }

                    //var action = voiceCommand.Properties["action"][0];
                    //var location = voiceCommand.Properties["location"][0];
                    //userMessage.DisplayMessage = String.Format("已为你{1}{0}盏电灯", yeelightList.Count, action);
                    userMessage.DisplayMessage = String.Format("已为你打开{0}盏电灯", yeelightList.Count);
                    userMessage.SpokenMessage = userMessage.DisplayMessage;

                    response = VoiceCommandResponse.CreateResponse(userMessage);

                    await voiceServiceConnection.ReportSuccessAsync(response);

                    break;
                case "closePower":
                    foreach (var item in yeelightList)
                    {
                        await item.ToggleAsync();
                    }

                    userMessage.DisplayMessage = String.Format("已为你关闭{0}盏电灯", yeelightList.Count);
                    userMessage.SpokenMessage = userMessage.DisplayMessage;

                    response = VoiceCommandResponse.CreateResponse(userMessage);

                    await voiceServiceConnection.ReportSuccessAsync(response);

                    break;
                default:
                    LaunchAppInForeground();
                    break;

            }
        }

        private async void LaunchAppInForeground()
        {
            var userMessage = new VoiceCommandUserMessage();
            userMessage.SpokenMessage = "Launching Adventure Works";

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            // When launching the app in the foreground, pass an app 
            // specific launch parameter to indicate what page to show.
            response.AppLaunchArgument = "showAllTrips=true";

            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }
    }
}