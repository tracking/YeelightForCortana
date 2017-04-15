using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YeelightAPI;

namespace YeelightForCortana.ViewModel
{
    public class Device : BaseModel
    {
        private Yeelight rawDevice;
        private string name;
        private bool power;
        private bool online;
        private bool isBusy;

        /// <summary>
        /// 源设备
        /// </summary>
        public Yeelight RawDevice { get { return rawDevice; } }

        /// <summary>
        /// 设备ID 只读
        /// </summary>
        public string Id { get { return rawDevice.Id; } }
        /// <summary>
        /// 设备名
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                this.EmitPropertyChanged("Name");
            }
        }
        /// <summary>
        /// 电源状态
        /// </summary>
        public bool Power
        {
            get
            {
                return power;
            }
            set
            {
                power = value;
                this.EmitPropertyChanged("Power");
            }
        }
        /// <summary>
        /// 是否在线
        /// </summary>
        public bool Online
        {
            get
            {
                return online;
            }
            set
            {
                online = value;
                this.EmitPropertyChanged("Online");
                this.EmitPropertyChanged("IsEnabled");
            }
        }
        /// <summary>
        /// 是否正忙
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                isBusy = value;
                this.EmitPropertyChanged("IsBusy");
                this.EmitPropertyChanged("IsEnabled");
            }
        }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return online && !isBusy;
            }
        }


        public Device(string rawDeviceInfo)
        {
            rawDevice = new Yeelight(rawDeviceInfo);
        }
        public Device(Yeelight rawDevice)
        {
            this.rawDevice = rawDevice;
        }
    }
}
