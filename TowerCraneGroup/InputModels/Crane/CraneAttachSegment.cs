using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.InputModels.Crane
{
    /// <summary>
    /// 塔吊每道附墙的实体
    /// </summary>
    public class CraneAttachSegment
    {
        /// <summary>
        /// 附墙序号
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 附墙范围上限
        /// </summary>
        public double UpperBound { get; set; }
        /// <summary>
        /// 附墙范围下限
        /// </summary>
        public double LowerBound { get; set; }
        /// <summary>
        /// 最大悬臂高度
        /// </summary>
        public double CantileverHeight { get; set; }
        /// <summary>
        /// 最大悬臂高度对应的标准节数量
        /// </summary>
        public int CantileverSectionNum { get; set; }
    }
}
