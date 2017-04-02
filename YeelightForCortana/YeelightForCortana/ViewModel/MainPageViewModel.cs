using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana.ViewModel
{
    public class MainPageViewModel : BaseModel
    {
        // 是否显示语音命令集面板
        private bool showVoiceCommandSetGrid;
        // 是否显示设备面板
        private bool showDeviceGrid;

        // 设备列表
        public DeviceList DeviceList { get; set; }
        // 新设备列表
        public DeviceList NewDeviceList { get; set; }
        // 设备选择列表
        public DeviceCheckList DeviceCheckList { get; set; }
        // 设备分组列表
        public DeviceGroupList DeviceGroupList { get; set; }
        // 命令类型列表
        public CommandTypeList CommandTypeList { get; set; }
        // 语音命令集列表
        public VoiceCommandSetList VoiceCommandSetList { get; set; }
        // 语音命令集
        public VoiceCommandSet SelectedVoiceCommandSet { get; set; }

        // 设备分组列表选中索引
        public int DeviceGroupListSelectedIndex { get; set; }
        // 命令类型列表选中索引
        public int CommandTypeListSelectedIndex { get; set; }

        // 是否显示语音命令集面板
        public bool ShowVoiceCommandSetGrid
        {
            get { return this.showVoiceCommandSetGrid; }
            set
            {
                this.showVoiceCommandSetGrid = value;
                this.EmitPropertyChanged("ShowVoiceCommandSetGrid");
            }
        }
        // 是否显示新设备面板
        public bool ShowDeviceGrid
        {
            get { return this.showDeviceGrid; }
            set
            {
                this.showDeviceGrid = value;
                this.EmitPropertyChanged("ShowDeviceGrid");
            }
        }

        // 没有添加任何语音命令
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
