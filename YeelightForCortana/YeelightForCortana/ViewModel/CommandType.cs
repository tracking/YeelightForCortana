using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YeelightAPI;

namespace YeelightForCortana.ViewModel
{
    public class CommandType : BaseModel
    {
        private ActionType? type;
        private string name;

        public ActionType? Type { get { return type; } }
        public string Name { get { return name; } }

        public CommandType()
        {
            this.type = null;
            this.name = "全部";
        }
        public CommandType(ActionType type)
        {
            this.type = type;

            switch (type)
            {
                case ActionType.PowerOn:
                    this.name = "开灯";
                    break;
                case ActionType.PowerOff:
                    this.name = "关灯";
                    break;
                case ActionType.BrightUp:
                    this.name = "增加亮度";
                    break;
                case ActionType.BrightDown:
                    this.name = "减少亮度";
                    break;
                case ActionType.SwitchColor:
                    this.name = "切换颜色";
                    break;
                default:
                    throw new Exception("不受支持的操作类型");
            }
        }
    }
}
