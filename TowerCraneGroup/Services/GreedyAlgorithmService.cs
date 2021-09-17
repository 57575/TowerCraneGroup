using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Entities;
using TowerCraneGroup.InputModels;
using TowerCraneGroup.SolutionModels;
using Newtonsoft.Json;

namespace TowerCraneGroup.Services
{
    public class GreedyAlgorithmService
    {
        private Dictionary<int, TowerCrane> Towers { get; set; }
        private List<TowerChargeBuilding> TowerCharges { get; set; }
        private Dictionary<int, BuildingProcessing> Buildings { get; set; }
        private Dictionary<int, List<CollisionRelation>> Collision { get; set; }
        private List<FinishTimeHelper> FinishTimeTowers { get; set; }
        private List<TowerChargeHelper> TowerChargeHelpers { get; set; }
        public GreedyAlgorithmService(
            Dictionary<int, TowerCrane> towerInfo,
            List<TowerChargeBuilding> towerCharge,
            Dictionary<int, BuildingProcessing> buildings,
            Dictionary<int, List<CollisionRelation>> collisionsDic
            )
        {
            Towers = towerInfo;
            TowerCharges = towerCharge;
            Buildings = buildings;
            Collision = collisionsDic;
            FinishTimeTowers = new List<FinishTimeHelper>();
            TowerChargeHelpers = new List<TowerChargeHelper>();
            towerCharge.ForEach(x =>
            {
                FinishTimeHelper finishTimeHelper = new FinishTimeHelper
                {
                    TowerId = x.TowerId,
                    Time = Buildings[x.BuildingId].Process.Keys.OrderByDescending(x => x).FirstOrDefault()
                };
                FinishTimeTowers.Add(finishTimeHelper);
            });
        }

        private void Service()
        {
            Individual solution = new Individual();

            int towerId = GetLatestTower(0);
            for (int geneIndex = 0; geneIndex < Towers.Count; geneIndex++)
            {
                TowerCrane towerCrane = Towers[towerId];
                int chargeBuildingId = TowerCharges.Where(x => x.TowerId == towerId).FirstOrDefault().BuildingId;
                BuildingProcessing building = Buildings[chargeBuildingId];

                List<int> gene = new List<int>();

                TowerChargeHelper towerChargeHelper = new TowerChargeHelper(geneIndex, towerId, chargeBuildingId, building.Process.Count, towerCrane.SectionHeight, towerCrane.StartHeight);
                TowerChargeHelpers.Add(towerChargeHelper);

                List<int> collisionableTowerIds = Collision[towerId].Where(x => x.RelationType == CollisionRelationType.塔吊与塔吊).Select(x => x.CollisionId).ToList();
                List<int> existedTowerIds = TowerChargeHelpers.Where(x => collisionableTowerIds.Contains(x.TowerId)).Select(x => x.TowerId).ToList();
                if (existedTowerIds != null && existedTowerIds.Count != 0)
                {

                }
                else
                {
                    gene = GenerateGeneWithoutOtherTower(towerId, chargeBuildingId);
                }




                solution.Genes.Add(gene);
            }
        }
        /// <summary>
        /// <para>若该塔吊范围内暂无其它塔吊方案,则该塔吊仅以建筑为约束进行提升</para>
        /// <para>并生成相关基因</para>        
        /// </summary>
        /// <param name="towerId"></param>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        private List<int> GenerateGeneWithoutOtherTower(int towerId, int buildingId)
        {
            List<int> result = new List<int>();
            double nowHeight = Towers[towerId].StartHeight;
            int liftingNum = 0;
            BuildingProcessing building = Buildings[buildingId];
            building.Process.Values.ToList().ForEach(x =>
            {
                if (x + GetSecurityHeight() > nowHeight)
                {
                    int thisLiftSectionNum = Towers[towerId].LiftSectionNumDic[liftingNum];
                    liftingNum++;
                    nowHeight += thisLiftSectionNum * Towers[towerId].SectionHeight;
                }
                else
                {
                    result.Add(0);
                }
            });
            return result;
        }

        private List<int> GenerateGene(int towerId)
        {
            List<int> result = new List<int>();
            return result;
        }

        /// <summary>
        /// 寻找给定towerId可能碰撞的所有塔吊中，进度最慢的塔吊
        /// </summary>
        /// <param name="towerId"></param>
        /// <returns></returns>
        private int FindLatestWithTowerId(int towerId)
        {
            List<int> towerIds = Collision[towerId]
                .Where(x => x.RelationType == CollisionRelationType.塔吊与塔吊)
                .Select(x => x.CollisionId)
                .ToList();
            return FinishTimeTowers
                .Where(x => towerIds.Contains(x.TowerId))
                .OrderByDescending(x => x.Time)
                .FirstOrDefault().TowerId;
        }
        /// <summary>
        /// 获取第index最晚完成的塔吊Id,从0开始
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int GetLatestTower(int index)
        {
            return FinishTimeTowers.OrderByDescending(x => x.Time).ToList()[index].TowerId;
        }
        /// <summary>
        /// 获取安全距离
        /// </summary>
        /// <returns></returns>
        private double GetSecurityHeight()
        {
            return 8.00;
        }
        private List<int> NewEmptyGene(int geneNum)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < geneNum; i++)
            {
                result.Add(0);
            }
            return result;
        }
        struct FinishTimeHelper
        {
            public DateTime Time { get; set; }
            public int TowerId { get; set; }
        }


    }

}
