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
        // 是否显示新设备面板
        private bool showNewDeviceGrid;
        // 语音命令集详情
        private VoiceCommandSet voiceCommandSetDetail;
        // 是否显示设置亮度面板
        private bool showSetBrightGrid;
        // 是否显示切换颜色面板
        private bool showSwitchColorGrid;
        // 语音命令详情是否已修改
        private bool voiceCommandSetDetailIsEdit;
        // 是否显示语音详情中Say面板
        private bool showVoiceCommandSetDetailSayGrid;
        // 是否显示语音详情中Answer面板
        private bool showVoiceCommandSetDetailAnswerGrid;

        // 设备列表
        public DeviceList DeviceList { get; set; }
        // 新设备列表
        public DeviceList NewDeviceList { get; set; }
        // 设备选择列表
        public DeviceCheckList DeviceCheckList { get; set; }
        // 设备分组列表
        public DeviceGroupList DeviceGroupList { get; set; }
        // 命令类型列表(显示用)
        public CommandTypeList CommandTypeDisplayList { get; set; }
        // 命令类型列表
        public CommandTypeList CommandTypeList { get; set; }
        // 语音命令集列表
        public VoiceCommandSetList VoiceCommandSetList { get; set; }
        // 语音命令集详情
        public VoiceCommandSet VoiceCommandSetDetail
        {
            get
            {
                return voiceCommandSetDetail;
            }
            set
            {
                voiceCommandSetDetail = value;
                EmitPropertyChanged("VoiceCommandSetDetail");
                EmitPropertyChanged("ShowVoiceCommandSetDetailGrid");
            }
        }
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
                if (value)
                {
                    // 隐藏其他面板
                    this.ShowDeviceGrid = false;
                }

                this.showVoiceCommandSetGrid = value;
                this.EmitPropertyChanged("ShowVoiceCommandSetGrid");
            }
        }
        // 是否显示设备面板
        public bool ShowDeviceGrid
        {
            get { return this.showDeviceGrid; }
            set
            {
                if (value)
                {
                    // 隐藏其他面板
                    this.ShowVoiceCommandSetGrid = false;
                }

                this.showDeviceGrid = value;
                this.EmitPropertyChanged("ShowDeviceGrid");
            }
        }
        // 是否显示新设备面板
        public bool ShowNewDeviceGrid
        {
            get { return this.showNewDeviceGrid; }
            set
            {
                this.showNewDeviceGrid = value;
                this.EmitPropertyChanged("ShowNewDeviceGrid");
            }
        }
        // 是否显示语音命令集详情面板
        public bool ShowVoiceCommandSetDetailGrid
        {
            get
            {
                return this.voiceCommandSetDetail != null;
            }
        }
        // 语音命令详情是否已修改
        public bool VoiceCommandSetDetailIsEdit
        {
            get
            {
                return voiceCommandSetDetailIsEdit;
            }
            set
            {
                voiceCommandSetDetailIsEdit = value;
                EmitPropertyChanged("VoiceCommandSetDetailIsEdit");
            }
        }
        // 是否显示设置亮度面板
        public bool ShowSetBrightGrid
        {
            get { return this.showSetBrightGrid; }
            set
            {
                if (value)
                {
                    // 隐藏其他面板
                    this.ShowSwitchColorGrid = false;
                }

                this.showSetBrightGrid = value;
                this.EmitPropertyChanged("ShowSetBrightGrid");
            }
        }
        // 是否显示切换颜色面板
        public bool ShowSwitchColorGrid
        {
            get { return this.showSwitchColorGrid; }
            set
            {
                if (value)
                {
                    // 隐藏其他面板
                    this.ShowSetBrightGrid = false;
                }

                this.showSwitchColorGrid = value;
                this.EmitPropertyChanged("ShowSwitchColorGrid");
            }
        }
        // 是否显示语音详情中Say面板
        public bool ShowVoiceCommandSetDetailSayGrid
        {
            get { return this.showVoiceCommandSetDetailSayGrid; }
            set
            {
                if (value)
                {
                    // 隐藏其他面板
                    this.ShowVoiceCommandSetDetailAnswerGrid = false;
                }

                this.showVoiceCommandSetDetailSayGrid = value;
                this.EmitPropertyChanged("ShowVoiceCommandSetDetailSayGrid");
            }
        }   
        // 是否显示语音详情中Answer面板
        public bool ShowVoiceCommandSetDetailAnswerGrid
        {
            get { return this.showVoiceCommandSetDetailAnswerGrid; }
            set
            {
                if (value)
                {
                    // 隐藏其他面板
                    this.ShowVoiceCommandSetDetailSayGrid = false;
                }

                this.showVoiceCommandSetDetailAnswerGrid = value;
                this.EmitPropertyChanged("ShowVoiceCommandSetDetailAnswerGrid");
            }
        }

        // 没有添加任何语音命令
        public bool DoesNotExistVoiceCommand
        {
            get
            {
                return VoiceCommandSetList == null || VoiceCommandSetList.Count == 0;
            }
        }

        public MainPageViewModel()
        {
            // 默认数据
            this.DeviceList = new DeviceList();
            this.NewDeviceList = new DeviceList();
            this.DeviceGroupList = new DeviceGroupList();
            this.DeviceCheckList = new DeviceCheckList();
            this.CommandTypeDisplayList = new CommandTypeList();
            this.CommandTypeList = new CommandTypeList();
            this.VoiceCommandSetList = new VoiceCommandSetList();

            // 默认选择第一个
            this.DeviceGroupListSelectedIndex = 0;
            this.CommandTypeListSelectedIndex = 0;
        }
    }
}
