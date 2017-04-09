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
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> DeviceList { get; set; }

        public DeviceGroup()
        {
            this.DeviceList = new List<string>();
        }
    }
}
