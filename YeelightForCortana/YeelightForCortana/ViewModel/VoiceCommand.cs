using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana.ViewModel
{
    public class VoiceCommand : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Say { get; set; }
        public string Answer { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
