using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.SolutionModels
{
    /// <summary>
    /// 控制塔吊高度的楼宇
    /// </summary>
    public class TowerChargeBuilding
    {
        /// <summary>
        /// 塔吊Id
        /// </summary>
        public int TowerId { get; set; }
        /// <summary>
        /// 建筑Id
        /// </summary>
        public int BuildingId { get; set; }
        /// <summary>
        /// 是否需要提升
        /// </summary>
        public bool NeedRaise { get; private set; }

        public void SetRaise(bool needRaise)
        {
            this.NeedRaise = needRaise;
        }
    }
}
