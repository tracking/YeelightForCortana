using System;
using System.Collections.Generic;
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

        public int Id { get; set; }
        public CommandType CommandType { get; set; }
        public List<VoiceCommand> VoiceCommandList { get; set; }
        public string ObjectName
        {
            get
            {
                return device == null ? deviceGroup.Name : device.Name;
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
        
        public VoiceCommandSet(Device device)
        {
            this.device = device;
        }
        public VoiceCommandSet(DeviceGroup deviceGroup)
        {
            this.deviceGroup = deviceGroup;
        }
    }
}
