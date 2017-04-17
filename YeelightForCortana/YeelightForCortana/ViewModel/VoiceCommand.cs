using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana.ViewModel
{
    public class VoiceCommand : BaseModel
    {
        private string say;
        private string answer;

        public string Id { get; set; }
        public string Say
        {
            get { return say; }
            set
            {
                this.say = value;
                EmitPropertyChanged("Say");
            }
        }
        public string Answer
        {
            get { return answer; }
            set
            {
                this.answer = value;
                EmitPropertyChanged("Answer");
            }
        }

        public VoiceCommand()
        {

        }
    }
}
