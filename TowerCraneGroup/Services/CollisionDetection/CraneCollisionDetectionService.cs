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
    public class CraneCollisionDetectionService
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
            return TowerCollisionDetection(genes, collisionsDic, buildingDic, towerDic, towerChargeDic);
        }

        /// <summary>
        /// 碰撞次数
        /// </summary>
        /// <param name="collisionsDic"></param>
        /// <param name="buildingDic"></param>
        /// <returns></returns>
        private List<CollisionDetectionErrorMessage> TowerCollisionDetection(
            List<List<int>> Genes,
            Dictionary<int, List<CollisionRelation>> collisionsDic,
            Dictionary<int, BuildingProcessing> buildingDic,
            Dictionary<int, TowerCrane> towerDic,
            Dictionary<int, TowerChargeHelper> towerChargeDic
            )
        {
            List<CollisionDetectionErrorMessage> results = new List<CollisionDetectionErrorMessage>();
            //i is genes index
            for (int i = 0; i < towerChargeDic.Count; i++)
            {
                //简单验证
                if (towerChargeDic[i].FloorNumber != Genes[i].Count)
                    throw new Exception("发生未知错误");
                if (collisionsDic.TryGetValue(towerChargeDic[i].TowerId, out List<CollisionRelation> col))
                {
                    List<int> collisionTowerIds = col.Where(x => x.RelationType == CollisionRelationType.塔吊与塔吊).Select(x => x.CollisionId).ToList();
                    TowerCrane thisTower = towerDic[towerChargeDic[i].TowerId];
                    double thisTowerHeight = thisTower.StartHeight;

                    for (int floorIndex = 0; floorIndex < Genes[i].Count; floorIndex++)
                    {
                        if (Genes[i][floorIndex] != 0)
                        {
                            double beforeLiftHeight = thisTowerHeight;
                            double afterLiftHeight = thisTowerHeight + thisTower.SectionHeight * Genes[i][floorIndex];
                            //控制楼宇本层完工时间
                            DateTime thisFloorFinish = buildingDic[towerChargeDic[i].BuildingId].Process.Keys.ToList()[floorIndex];
                            //检测塔吊和塔吊的碰撞
                            collisionTowerIds.ForEach(towerId =>
                            {
                                //被检测塔吊的安装时间晚于当前时间或是被检测塔吊的拆除时间早于当前时间
                                //即被检测塔吊未安装或是已拆除，则不检测
                                if (!(towerDic[towerId].StartTime >= thisFloorFinish || towerDic[towerId].EndTime <= thisFloorFinish))
                                {
                                    if (CalculateTowerCollision(Genes, beforeLiftHeight, afterLiftHeight, thisFloorFinish, towerId, buildingDic, towerChargeDic))
                                    {
                                        results.Add(new CollisionDetectionErrorMessage
                                        {
                                            ErrorType = Enums.CollisionDetecion.CollisionError.塔吊安全距离不足,
                                            CraneId = thisTower.Id,
                                            CraneCode = thisTower.Code,
                                            DateTime = thisFloorFinish,
                                            CollisionId = towerId,
                                            CollisionCode = towerDic[towerId].Code
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
        /// <param name="startHeight">本塔吊的初始高度</param>
        /// <param name="beforeLift">本塔吊提升前高度</param>
        /// <param name="afterLift">本塔吊提升后高度</param>
        /// <param name="thisFloorFinish">本塔吊提升时的时间</param>
        /// <param name="towerId">被检测是否碰撞的塔吊Id</param>
        /// <param name="buildingDic"></param>
        /// <returns>是否碰撞，是为true</returns>
        private bool CalculateTowerCollision(
            List<List<int>> Genes,
            double beforeLift,
            double afterLift,
            DateTime thisFloorFinish,
            int towerId,
            Dictionary<int, BuildingProcessing> buildingDic,
            Dictionary<int, TowerChargeHelper> towerChargeDic)
        {
            //被检测塔吊
            TowerChargeHelper CollisionTower = towerChargeDic.Values.Where(x => x.TowerId == towerId).FirstOrDefault();
            //被检测塔吊的主要楼宇的Id
            int buildingId = CollisionTower.BuildingId;
            //被检测塔吊在基因组中的Index
            int index = CollisionTower.GeneIndex;
            //被检测塔吊的节长
            double sectionLength = CollisionTower.TowerSectionLength;
            //
            DateTime? time = null;
            var times = buildingDic[buildingId].Process.Keys.Where(x => x <= thisFloorFinish);
            if (times != null && times.Count() != 0)
            { time = times.OrderBy(x => x).LastOrDefault(); }

            //即被检测塔吊的主控楼宇还未开始施工，此时判断提升前后是否与被检测塔吊有碰撞
            //即被检测塔吊不提升
            if (time is null)
            {
                //被检测塔吊的初始高度
                double height = towerChargeDic.Values.Where(x => x.TowerId == towerId).FirstOrDefault().TowerStartHeight;
                if (afterLift >= height + SecurityHeight || afterLift <= height - SecurityHeight)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            //被检测塔吊当天有可能有提升
            else
            {
                int floorIndex = buildingDic[buildingId].Process.Keys.OrderBy(x => x).ToList().IndexOf(time.Value);
                //当日被检测塔吊也会提升
                if (time == thisFloorFinish && Genes[index][floorIndex] != 0)
                {
                    //被检测塔吊提升前
                    double beijianceqian = CollisionTower.TowerStartHeight;
                    for (int i = 0; i < floorIndex; i++)
                    {
                        if (Genes[index][i] != 0)
                        {
                            beijianceqian += Genes[index][i] * sectionLength;
                        }
                    }
                    //被检测塔吊提升后
                    double beijiancehou = beijianceqian + Genes[index][floorIndex] * sectionLength;

                    if (afterLift >= beijiancehou + SecurityHeight || afterLift <= beijiancehou - SecurityHeight)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                //当日被检测塔吊不提升
                else
                {
                    double height = CollisionTower.TowerStartHeight;
                    for (int i = 0; i <= floorIndex; i++)
                    {
                        if (Genes[index][i] != 0)
                        {
                            height += Genes[index][i] * sectionLength;
                        }
                    }

                    if (afterLift >= height + SecurityHeight || afterLift <= height - SecurityHeight)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

    }
}
