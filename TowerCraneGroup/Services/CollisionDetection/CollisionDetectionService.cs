using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TowerCraneGroup.InputModels.CollisionDetection;
using TowerCraneGroup.InputModels.Crane;
using TowerCraneGroup.InputModels.Process;
using TowerCraneGroup.SolutionModels;
using TowerCraneGroup.ViewModels.CollisionDetection;

namespace TowerCraneGroup.Services.CollisionDetection
{
    public static class CollisionDetectionService
    {
        public static List<CollisionDetectionErrorMessage> CalculateError(
            List<TowerWorkCondition> conditions,
            List<BuildingProcessing> buildingProcessings,
            List<CollisionRelation> collisions,
            List<TowerCrane> towerCranes,
            List<TowerAttachRelation> attaches)
        {
            List<CollisionDetectionErrorMessage> results = new List<CollisionDetectionErrorMessage>();

            Dictionary<int, TowerCrane> towerDic = towerCranes.ToDictionary(x => x.Id);
            Dictionary<int, List<CollisionRelation>> collisionDic = collisions.GroupBy(x => x.TowerId).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<int, BuildingProcessing> buildingDic = buildingProcessings.ToDictionary(x => x.Id);
            List<TowerChargeBuilding> towerChargeBuildings = CalculateTowerCharge.CalculteTowerChargeBuilding(buildingProcessings, collisionDic, towerDic);
            CompletionTowerAttachment.CompletionTower(towerCranes, buildingProcessings, towerChargeBuildings);
            var geneAndCharge = GenerateTowerChargeHelpers(conditions, towerDic, towerChargeBuildings, buildingDic, attaches);

            //计算建筑碰撞
            BuildingCollisionDetectionService buildingCollision = new BuildingCollisionDetectionService();
            List<CollisionDetectionErrorMessage> buildingCollisionErrors = buildingCollision.CalculateBuildingCollisionDetection(geneAndCharge.Item1, collisionDic, buildingDic, towerDic, geneAndCharge.Item2.ToDictionary(x => x.GeneIndex));
            results.AddRange(buildingCollisionErrors);

            //计算塔吊碰撞
            CraneCollisionDetectionService craneCollision = new CraneCollisionDetectionService();
            results.AddRange(craneCollision.CalculateBuildingCollisionDetection(geneAndCharge.Item1, collisionDic, buildingDic, towerDic, geneAndCharge.Item2.ToDictionary(x => x.GeneIndex)));

            return results;
        }



        private static (List<List<int>>, List<TowerChargeHelper>) GenerateTowerChargeHelpers(
            List<TowerWorkCondition> conditions,
            Dictionary<int, TowerCrane> towerDic,
            List<TowerChargeBuilding> towerChargeBuildings,
            Dictionary<int, BuildingProcessing> buildingDic,
            List<TowerAttachRelation> attaches
            )
        {
            List<List<int>> genes = new List<List<int>>();
            List<TowerChargeHelper> chargeHelpers = new List<TowerChargeHelper>();
            for (int i = 0; i < conditions.Count; i++)
            {
                List<int> gene = new List<int>();
                TowerCrane thisTower = towerDic[conditions[i].TowerId];
                TowerChargeBuilding thisCharge = towerChargeBuildings.FirstOrDefault(x => x.TowerId == thisTower.Id);
                BuildingProcessing chargeBuilding = buildingDic[thisCharge.BuildingId];
                TowerAttachRelation attachBuilding = attaches.FirstOrDefault(x => x.TowerId == thisTower.Id);
                conditions[i].WorkConditions.OrderBy(x => x.FloorIndex).ToList().ForEach(x =>
                {
                    gene.Add(x.LiftSectionNumber);
                });
                TowerChargeHelper result = new TowerChargeHelper(i, thisTower.Id, chargeBuilding.Id, chargeBuilding.Process.Count, thisTower.SectionHeight, thisTower.StartHeight, attachBuilding.BuildingId);
                chargeHelpers.Add(result);
                genes.Add(gene);
            }
            return (genes, chargeHelpers);
        }
    }
}
