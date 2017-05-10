using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using YeelightAPI;

namespace YeelightForCortana.ViewModel
{
    public class CommandType : BaseModel
    {
        // 资源
        private ResourceLoader rl;

        private ActionType? type;
        private string name;

        public ActionType? Type { get { return type; } }
        public string Name { get { return name; } }

        public CommandType()
        {
            this.rl = new ResourceLoader();
            this.type = null;
            this.name = rl.GetString("ActionType_All");
        }
        public CommandType(ActionType type)
        {
            this.rl = new ResourceLoader();
            this.type = type;

            switch (type)
            {
                case ActionType.PowerOn:
                    this.name = rl.GetString("ActionType_PowerOn");
                    break;
                case ActionType.PowerOff:
                    this.name = rl.GetString("ActionType_PowerOff");
                    break;
                case ActionType.BrightUp:
                    this.name = rl.GetString("ActionType_BrightUp");
                    break;
                case ActionType.BrightDown:
                    this.name = rl.GetString("ActionType_BrightDown");
                    break;
                case ActionType.SwitchColor:
                    this.name = rl.GetString("ActionType_SwitchColor");
                    break;
                default:
                    throw new Exception("不受支持的操作类型");
            }
        }
    }
}
