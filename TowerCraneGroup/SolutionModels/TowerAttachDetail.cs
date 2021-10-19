using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.SolutionModels
{
    /// <summary>
    /// 附墙的详细信息
    /// </summary>
    public class TowerAttachDetail
    {
        /// <summary>
        /// 附墙次数序号
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 附墙位置高度
        /// </summary>
        public double Height { get; set; }
        /// <summary>
        /// 附墙时间
        /// </summary>
        public DateTime DateTime { get; set; }
    }
}
