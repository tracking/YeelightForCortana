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

        public Yeelight RawDevice { get { return rawDevice; } }

        public string Id { get { return rawDevice.Id; } }
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
        public bool Power { get; set; }
        public bool Online { get; set; }

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
