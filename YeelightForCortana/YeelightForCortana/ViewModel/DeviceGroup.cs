using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana.ViewModel
{
    public class DeviceGroup : BaseModel
    {
        private string name;

        public string Id { get; set; }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
                EmitPropertyChanged("Name");
            }
        }
        public List<Device> DeviceList { get; set; }

        public DeviceGroup()
        {
            this.DeviceList = new List<Device>();
        }
    }
}
