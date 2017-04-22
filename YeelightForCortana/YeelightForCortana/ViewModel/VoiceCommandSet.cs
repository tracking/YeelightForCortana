using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana.ViewModel
{
    public class VoiceCommandSet : BaseModel
    {
        private Device device;
        private DeviceGroup deviceGroup;

        public string Id { get; set; }
        public CommandType CommandType { get; set; }
        public string ActionParams { get; set; }
        public ObservableCollection<VoiceCommand> VoiceCommandList { get; set; }
        public string ObjectName
        {
            get
            {
                return Device == null ? DeviceGroup.Name : Device.Name;
            }
        }
        public string FirstSay
        {
            get
            {
                var first = VoiceCommandList.First();

                return first != null ? first.Say : "";
            }
        }
        public string FirstAnswer
        {
            get
            {
                var first = VoiceCommandList.First();

                return first != null ? first.Answer : "";
            }
        }

        public Device Device { get => device; set => device = value; }
        public DeviceGroup DeviceGroup { get => deviceGroup; set => deviceGroup = value; }

        public VoiceCommandSet()
        {
            this.VoiceCommandList = new ObservableCollection<VoiceCommand>();
        }
        public VoiceCommandSet(Device device)
        {
            this.Device = device;
            this.VoiceCommandList = new ObservableCollection<VoiceCommand>();
        }
        public VoiceCommandSet(DeviceGroup deviceGroup)
        {
            this.DeviceGroup = deviceGroup;
            this.VoiceCommandList = new ObservableCollection<VoiceCommand>();
        }
    }
}
