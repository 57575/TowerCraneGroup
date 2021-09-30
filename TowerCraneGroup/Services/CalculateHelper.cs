using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Entities;
using TowerCraneGroup.InputModels;
using TowerCraneGroup.SolutionModels;

namespace TowerCraneGroup.Services
{
    public static class CalculateHelper
    {
        /// <summary>
        /// 计算塔吊负责的关键楼宇
        /// ---------------------
        /// 即计算塔吊负责的所建造的最快的楼宇
        /// </summary>
        /// <returns>（塔吊Id,楼宇Id）</returns>
        public static List<TowerChargeBuilding> CalculteTowerChargeBuilding(List<BuildingProcessing> processings, Dictionary<int, List<CollisionRelation>> collisions, Dictionary<int, TowerCrane> towersDic)
        {
            List<TowerChargeBuilding> results = new List<TowerChargeBuilding>();
            foreach (KeyValuePair<int, List<CollisionRelation>> aTower in collisions)
            {
                List<int> buildingIds = aTower.Value
                    .Where(x => x.RelationType == CollisionRelationType.塔吊与楼宇)
                    .Select(x => x.CollisionId)
                    .ToList();
                int mainBuildingId = processings
                    .Where(x => buildingIds.Contains(x.Id))
                    .Select(x => new { x.Id, StartTime = x.Process.Keys.FirstOrDefault() })
                    .OrderBy(x => x.StartTime)
                    .FirstOrDefault().Id;

                if (towersDic[aTower.Key].StartHeight <= processings.Where(x => x.Id == mainBuildingId).FirstOrDefault().GetFinalStructureHeighth())
                {
                    var result = new TowerChargeBuilding
                    {
                        TowerId = aTower.Key,
                        BuildingId = mainBuildingId
                    };
                    results.Add(result);
                }
            }

            return results;
        }


        public static int CalculateTowerMaxLiftSection() { return 0; }

    }
}
