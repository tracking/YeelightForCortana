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
        public DeviceCheckList()
        {
        }
        public DeviceCheckList(DeviceList deviceList)
        {
            AddRange(deviceList);
        }

        /// <summary>
        /// 添加多个
        /// </summary>
        /// <param name="deviceList">设备列表</param>
        public void AddRange(DeviceList deviceList)
        {
            foreach (var item in deviceList)
                this.Add(new DeviceCheck() { IsChecked = false, Device = item });
        }
    }
}
