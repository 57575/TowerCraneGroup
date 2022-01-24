using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TowerCraneGroup.Enums.Crane;
using TowerCraneGroup.InputModels.Crane;
using TowerCraneGroup.InputModels.Process;
using TowerCraneGroup.SolutionModels;
using TowerCraneGroup.ViewModels.CollisionDetection;

namespace TowerCraneGroup.Services.CollisionDetection
{
    public class BuildingCollisionDetectionService
    {
        private double SecurityHeight = 8.00;

        public void SetSecurityHeight(double securityHeight)
        {
            this.SecurityHeight = securityHeight;
        }

        public List<CollisionDetectionErrorMessage> CalculateBuildingCollisionDetection(
            List<List<int>> genes,
            Dictionary<int, List<CollisionRelation>> collisionsDic,
            Dictionary<int, BuildingProcessing> buildingDic,
            Dictionary<int, TowerCrane> towerDic,
            Dictionary<int, TowerChargeHelper> towerChargeDic
            )
        {
            return BuildingCollisionDetection(genes, collisionsDic, buildingDic, towerDic, towerChargeDic);
        }

        private List<CollisionDetectionErrorMessage> BuildingCollisionDetection(
            List<List<int>> Genes,
            Dictionary<int, List<CollisionRelation>> collisionsDic,
            Dictionary<int, BuildingProcessing> buildingDic,
            Dictionary<int, TowerCrane> towerDic,
            Dictionary<int, TowerChargeHelper> towerChargeDic)
        {
            List<CollisionDetectionErrorMessage> results = new List<CollisionDetectionErrorMessage>();
            for (int i = 0; i < towerChargeDic.Count; i++)
            {
                //简单验证
                if (towerChargeDic[i].FloorNumber != Genes[i].Count)
                    throw new Exception("发生未知错误");
                if (collisionsDic.TryGetValue(towerChargeDic[i].TowerId, out List<CollisionRelation> col))
                {
                    List<int> collisionBuildingIds = col.Where(x => x.RelationType == CollisionRelationType.塔吊与楼宇).Select(x => x.CollisionId).ToList();
                    TowerCrane thisTower = towerDic[towerChargeDic[i].TowerId];
                    double thisTowerHeight = thisTower.StartHeight;
                    for (int floorIndex = 0; floorIndex < Genes[i].Count; floorIndex++)
                    {
                        if (Genes[i][floorIndex] != 0)
                        {
                            double beforeLiftHeight = thisTowerHeight;
                            double afterLiftHeight = beforeLiftHeight + thisTower.SectionHeight * Genes[i][floorIndex];
                            //控制楼宇本层完工时间
                            DateTime thisFloorFinish = buildingDic[towerChargeDic[i].BuildingId].Process.Keys.ToList()[floorIndex];
                            //检测塔吊和所负责楼宇的碰撞
                            collisionBuildingIds.ForEach(buil =>
                            {
                                DateTime? lastTime = null;
                                var times = buildingDic[buil].Process.Keys.Where(x => x <= thisFloorFinish);
                                if (times != null && times.Count() != 0)
                                { lastTime = times.OrderBy(x => x).LastOrDefault(); }

                                //lastTime为空，即检测塔吊范围内的楼宇还未开工
                                if (lastTime != null)
                                {
                                    //检测到有碰撞时，碰撞次数加一
                                    if (CalculateBuildingCollision(beforeLiftHeight, buildingDic[buil].Process[lastTime.Value]))
                                    {
                                        results.Add(new CollisionDetectionErrorMessage
                                        {
                                            ErrorType = Enums.CollisionDetecion.CollisionError.楼宇安全距离不足,
                                            CraneId = thisTower.Id,
                                            CraneCode = thisTower.Code,
                                            DateTime = thisFloorFinish,
                                            CollisionId = buil,
                                            CollisionCode = buildingDic[buil].BuildingCode
                                        });
                                    }
                                }
                            });

                            thisTowerHeight = afterLiftHeight;
                        }
                    }
                }
                else
                {
                    throw new Exception("发生未知错误");
                }
            }
            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="beforeLift">塔吊提升前的高度</param>
        /// <param name="buildingHeight">建筑高度</param>
        /// <returns>是否碰撞</returns>
        private bool CalculateBuildingCollision(double beforeLift, double buildingHeight)
        {
            if (buildingHeight + SecurityHeight > beforeLift)
                return true;
            else
                return false;
        }
    }
}
