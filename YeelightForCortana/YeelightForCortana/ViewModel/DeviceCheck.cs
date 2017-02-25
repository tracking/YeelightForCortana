using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana.ViewModel
{
    public class DeviceCheck : BaseModel
    {
        private bool isChecked;

        public Device Device { get; set; }
        public bool IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                this.EmitPropertyChanged("IsChecked");
            }
        }
    }
}
