using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana.ViewModel
{
    public class MainPageViewModel : BaseModel
    {
        private bool showNewDeviceGrid;

        public DeviceList DeviceList { get; set; }
        public DeviceCheckList DeviceCheckList { get; set; }
        public DeviceGroupList DeviceGroupList { get; set; }
        public CommandTypeList CommandTypeList { get; set; }
        public VoiceCommandSetList VoiceCommandSetList { get; set; }
        public VoiceCommandSet SelectedVoiceCommandSet { get; set; }

        public int DeviceGroupListSelectedIndex { get; set; }
        public int CommandTypeListSelectedIndex { get; set; }

        public bool ShowVoiceCommandSetGrid { get; set; }
        public bool ShowNewDeviceGrid
        {
            get { return this.showNewDeviceGrid; }
            set
            {
                this.showNewDeviceGrid = value;
                this.EmitPropertyChanged("ShowNewDeviceGrid");
            }
        }

        public bool DoesNotExistVoiceCommand
        {
            get
            {
                return !(VoiceCommandSetList.Count > 0);
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
