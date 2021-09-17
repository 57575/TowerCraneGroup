using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.SolutionModels
{
    /// <summary>
    /// 在一个个体中
    /// 控制塔吊的高度的楼宇
    /// 
    /// </summary>
    public class TowerChargeHelper
    {
        public TowerChargeHelper()
        {

        }

        public TowerChargeHelper(int index, int towerId, int buildingId, int floorNum, double sectionHeight, double startHeight)
        {
            GeneIndex = index;
            TowerId = towerId;
            BuildingId = buildingId;
            TowerSectionLength = sectionHeight;
            TowerStartHeight = startHeight;
            FloorNumber = floorNum;
        }

        public TowerChargeHelper(int index, TowerChargeBuilding towerChargeBuilding, int floorNum, double sectionHeight, double startHeight)
        {
            GeneIndex = index;
            TowerId = towerChargeBuilding.TowerId;
            BuildingId = towerChargeBuilding.BuildingId;
            TowerSectionLength = sectionHeight;
            TowerStartHeight = startHeight;
            FloorNumber = floorNum;
        }
        /// <summary>
        /// 在genes中的index
        /// </summary>
        public int GeneIndex { get; private set; }
        /// <summary>
        /// 塔吊Id
        /// </summary>
        public int TowerId { get; private set; }
        /// <summary>
        /// 塔吊每节的长度
        /// </summary>
        public double TowerSectionLength { get; private set; }
        /// <summary>
        /// 塔吊开始高度
        /// </summary>
        public double TowerStartHeight { get; set; }
        /// <summary>
        /// 建筑Id
        /// </summary>
        public int BuildingId { get; private set; }
        /// <summary>
        /// 建筑的楼层数
        /// </summary>
        public int FloorNumber { get; private set; }
    }
}
