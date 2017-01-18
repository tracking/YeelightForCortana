using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortanaService
{
    /// <summary>
    /// 电灯操作
    /// </summary>
    enum LightAction
    {
        /// <summary>
        /// 关闭电源
        /// </summary>
        PowerOff,
        /// <summary>
        /// 打开电源
        /// </summary>
        PowerOn,
        /// <summary>
        /// 切换电源
        /// </summary>
        PowerToggle,
        /// <summary>
        /// 增加亮度
        /// </summary>
        BrightUp,
        /// <summary>
        /// 减小亮度
        /// </summary>
        BrightDown,
    }
}
