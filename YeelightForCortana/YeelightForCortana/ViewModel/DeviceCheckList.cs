using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana.ViewModel
{
    public class DeviceCheckList : ObservableCollection<DeviceCheck>
    {
        public DeviceCheckList(DeviceList deviceList)
        {
            foreach (var item in deviceList)
                this.Add(new DeviceCheck() { IsChecked = false, Device = item });
        }
    }
}
