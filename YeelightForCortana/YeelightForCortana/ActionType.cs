using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// 开灯
        /// </summary>
        PowerOn,
        /// <summary>
        /// 关灯
        /// </summary>
        PowerOff,
        /// <summary>
        /// 增加亮度
        /// </summary>
        BrightUp,
        /// <summary>
        /// 减少亮度
        /// </summary>
        BrightDown,
        /// <summary>
        /// 切换颜色
        /// </summary>
        SwitchColor
    }
}
