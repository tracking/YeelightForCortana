using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana.ViewModel
{
    public class MainPageViewModel
    {
        public DeviceGroupList DeviceGroupList { get; set; }
        public CommandTypeList CommandTypeList { get; set; }
        public VoiceCommandSetList VoiceCommandSetList { get; set; }

        public int DeviceGroupListSelectedIndex { get; set; }
        public int CommandTypeListSelectedIndex { get; set; }
        public bool IsHasVoiceCommand
        {
            get
            {
                return VoiceCommandSetList.Count > 0;
            }
        }

        public MainPageViewModel()
        {
            // 默认选择第一个
            this.DeviceGroupListSelectedIndex = 0;
            this.CommandTypeListSelectedIndex = 0;
        }
    }
}
