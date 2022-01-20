using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.InputModels.CollisionDetection
{
    /// <summary>
    /// 一个塔吊的所有工况
    /// </summary>
    public class TowerWorkCondition
    {
        public int TowerId { get; set; }
        public List<WorkCondition> WorkConditions { get; set; }
    }
    /// <summary>
    /// 一个塔吊一层楼的工况
    /// </summary>
    public class WorkCondition
    {
        /// <summary>
        /// 该层楼属于第Index层
        /// </summary>
        public int FloorIndex { get; set; }
        public int FloorId { get; set; }
        public int LiftSectionNumber { get; set; }
    }
}
