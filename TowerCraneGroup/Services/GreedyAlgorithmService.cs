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
        private Individual Solution { get; set; }
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
            Solution = new Individual();
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

        public Individual RunService()
        {
            int towerId = GetLatestTower(0);
            for (int geneIndex = 0; geneIndex < Towers.Count; geneIndex++)
            {
                TowerCrane towerCrane = Towers[towerId];
                int chargeBuildingId = TowerCharges.Where(x => x.TowerId == towerId).FirstOrDefault().BuildingId;
                BuildingProcessing building = Buildings[chargeBuildingId];

                List<int> gene = new List<int>();

                List<int> collisionableTowerIds = Collision[towerId].Where(x => x.RelationType == CollisionRelationType.塔吊与塔吊).Select(x => x.CollisionId).ToList();
                List<int> existedTowerIds = TowerChargeHelpers.Where(x => collisionableTowerIds.Contains(x.TowerId)).Select(x => x.TowerId).ToList();
                if (existedTowerIds != null && existedTowerIds.Count != 0)
                {
                    gene = GenerateGene(towerId, building.Id, existedTowerIds);
                }
                else
                {
                    gene = GenerateGeneWithoutOtherTower(towerId, chargeBuildingId);
                }
                TowerChargeHelper towerChargeHelper = new TowerChargeHelper(geneIndex, towerId, chargeBuildingId, building.Process.Count, towerCrane.SectionHeight, towerCrane.StartHeight);
                TowerChargeHelpers.Add(towerChargeHelper);
                Solution.Genes.Add(gene);
                towerId = GetLatestTower(collisionableTowerIds);
                if (towerId == 0 && geneIndex < Towers.Count - 1)
                {
                    throw new ArgumentException("error");
                }
            }
            //Solution.CalculateFitness(Collision, Buildings, Towers, TowerChargeHelpers.ToDictionary(x => x.GeneIndex));
            Console.WriteLine(Solution.Serialize());
            Solution.CalculateFitness(Collision, Buildings, Towers, TowerChargeHelpers.ToDictionary(x => x.GeneIndex));
            return Solution;
        }

        public List<TowerChargeHelper> GetTowerChargeHelpers()
        {
            return TowerChargeHelpers;
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
            int liftingNum = 1;
            BuildingProcessing building = Buildings[buildingId];
            for (int i = 0; i < building.Process.Values.Count; i++)
            {
                if (building.Process.Values.ToList()[i] + GetSecurityHeight() > nowHeight)
                {
                    int remainSectionNum = (int)Math.Ceiling((building.GetFinalStructureHeighth() + GetSecurityHeight() - nowHeight) / Towers[towerId].SectionHeight);
                    int thisLiftSectionNum = Towers[towerId].LiftSectionNumDic[liftingNum];
                    if (thisLiftSectionNum > remainSectionNum)
                    {
                        thisLiftSectionNum = remainSectionNum;
                    }
                    liftingNum++;
                    nowHeight += thisLiftSectionNum * Towers[towerId].SectionHeight;
                    result[i - 1] = thisLiftSectionNum;
                    result.Add(0);
                }
                else
                {
                    result.Add(0);
                }
            }
            return result;
        }
        /// <summary>
        /// 若该塔吊范围内其它塔吊提升方案已生成，则该塔吊以建筑和塔吊为约束进行提升，并生成相关基因
        /// </summary>
        /// <param name="towerId"></param>
        /// <param name="buildingId"></param>
        /// <param name="exisedTowerIds"></param>
        /// <returns></returns>
        private List<int> GenerateGene(int towerId, int buildingId, List<int> exisedTowerIds)
        {
            List<int> result = new List<int>();
            //当前塔吊范围内已确定方案的最高塔吊
            double highestTower = FindHighest(exisedTowerIds) + GetSecurityHeight();

            //当前塔吊
            TowerCrane thisTower = Towers[towerId];
            //当前塔吊的主控建筑
            BuildingProcessing thisBuilding = Buildings[buildingId];

            //塔吊的当前高度
            double height = thisTower.StartHeight;
            //塔吊当前提升次数
            int liftNumIndex = 1;


            for (int geneIndex = 0; geneIndex < thisBuilding.Process.Count; geneIndex++)
            {
                double buildingHeight = thisBuilding.GetHeightByFloorIndex(geneIndex + 1) + GetSecurityHeight();

                var before = LiftBefore(exisedTowerIds, buildingId, height, thisBuilding.GetDateTimeByFloorIndex(geneIndex));

                if (buildingHeight > height || (before.Item1 && before.Item2 == thisBuilding.GetDateTimeByFloorIndex(geneIndex)))
                {
                    DateTime thisLiftingTime = thisBuilding.GetDateTimeByFloorIndex(geneIndex);

                    //按高度检测时，当前楼层高度与塔吊不满足安全要求，则应在上一层进行提升
                    //按其它塔吊提前提升时，则应在本层提升
                    //统一二者，将提升的序号加1


                    //查找剩余需要提升的节数，如果小于当前附墙次数最大提升节数，则替代
                    int remainSectionNum = (int)Math.Ceiling((Math.Max(highestTower, thisBuilding.GetFinalStructureHeighth() + GetSecurityHeight()) - height) / thisTower.SectionHeight);
                    int liftSectionNum = thisTower.LiftSectionNumDic[liftNumIndex];
                    if (remainSectionNum < liftSectionNum)
                    {
                        liftSectionNum = remainSectionNum;
                    }

                    for (int i = liftSectionNum; i > 0; i--)
                    {
                        double afterLiftingHeight = height + liftSectionNum * thisTower.SectionHeight;
                        if (TryLifting(exisedTowerIds, thisLiftingTime, afterLiftingHeight))
                        {
                            result.Add(liftSectionNum);
                            height = afterLiftingHeight;
                            liftNumIndex++;
                            break;
                        }
                        else
                        {
                            liftSectionNum--;
                        }
                    }
                    if (liftSectionNum == 0)
                    {
                        int doubleLiftSectionNum = thisTower.LiftSectionNumDic[liftNumIndex] + thisTower.LiftSectionNumDic[liftNumIndex + 1];
                        liftSectionNum = thisTower.LiftSectionNumDic[liftNumIndex];

                        for (int i = liftSectionNum; i < doubleLiftSectionNum; i++)
                        {
                            double afterLiftingHeight = height + liftSectionNum * thisTower.SectionHeight;
                            if (TryLifting(exisedTowerIds, thisLiftingTime, afterLiftingHeight))
                            {
                                result.Add(liftSectionNum);
                                height = afterLiftingHeight;
                                liftNumIndex++;
                                break;
                            }
                            else
                            {
                                liftSectionNum++;
                            }
                        }
                        if (liftSectionNum > doubleLiftSectionNum)
                        {
                            double needHeight = GetHeightLiftingOverMax(exisedTowerIds, thisLiftingTime) + GetSecurityHeight() - height;
                            int sectionNum = (int)Math.Ceiling(needHeight / thisTower.SectionHeight);
                            double afterLiftingHeight = height + sectionNum * thisTower.SectionHeight;
                            if (TryLifting(exisedTowerIds, thisLiftingTime, afterLiftingHeight))
                            {
                                result.Add(sectionNum);
                                height = afterLiftingHeight;
                                liftNumIndex++;
                            }
                            else
                            {
                                throw new ArgumentException("无法提升");
                            }
                        }
                    }
                }
                else
                {
                    result.Add(0);
                }
            }
            return result;
        }

        /// <summary>
        /// 是否需要提前提升，提前提升的最后时间点
        /// </summary>
        /// <param name="collisionIds">碰撞范围内已生成方案的塔吊</param>
        /// <param name="buildingId">正在生成方案的塔吊的主控楼宇</param>
        /// <param name="height">正在生成方案的塔吊的高度</param>
        /// <param name="liftTime">正在生成方案的塔吊的当前时间</param>
        /// <returns>true，需要提前提升</returns>
        private (bool, DateTime) LiftBefore(List<int> collisionIds, int buildingId, double height, DateTime liftTime)
        {
            DateTime? earliest = null;
            var towerChargeDic = TowerChargeHelpers.ToDictionary(x => x.TowerId);
            foreach (int collision in collisionIds)
            {
                if (towerChargeDic.TryGetValue(collision, out var towerInfo))
                {
                    double currentHeight = towerInfo.TowerStartHeight;
                    for (int floorFinishIndex = 0; floorFinishIndex < Solution.Genes[towerInfo.GeneIndex].Count; floorFinishIndex++)
                    {
                        int liftSectionNum = Solution.Genes[towerInfo.GeneIndex][floorFinishIndex];
                        if (liftSectionNum != 0)
                        {
                            currentHeight += liftSectionNum * towerInfo.TowerSectionLength;
                            if (BetweenSecrity(height, currentHeight))
                            {
                                var date = Buildings[towerInfo.BuildingId].Process.Keys.ToList()[floorFinishIndex];
                                if (date > liftTime)
                                {
                                    if (earliest is null || earliest > date)
                                        earliest = date;
                                }
                                break;
                            }
                        }
                    }

                }

            }
            if (earliest is null)
                return (false, new DateTime());
            else
            {
                DateTime result = Buildings[buildingId].Process.Keys.OrderBy(x => x).LastOrDefault(x => x < earliest.Value);
                return (true, result);
            }
        }

        private double GetHeightLiftingOverMax(List<int> collisionIds, DateTime liftTime)
        {
            var towerChargeDic = TowerChargeHelpers.ToDictionary(x => x.TowerId);

            //找到被检测塔吊范围内，下一次提升后最高的一个塔吊的高度
            double result = 0;
            foreach (int collision in collisionIds)
            {
                if (towerChargeDic.TryGetValue(collision, out var towerInfo))
                {
                    BuildingProcessing thisBuilding = Buildings[towerInfo.BuildingId];
                    TowerCrane thisTower = Towers[towerInfo.TowerId];
                    DateTime temTime = thisBuilding.Process.Keys.LastOrDefault(x => x <= liftTime);
                    int timeIndex = thisBuilding.Process.Keys.ToList().IndexOf(temTime);
                    if (!(thisTower.StartTime > liftTime || thisTower.EndTime < liftTime))
                    {
                        double lastHeight = towerInfo.TowerStartHeight;
                        for (int i = 0; i < Solution.Genes[towerInfo.GeneIndex].Count; i++)
                        {
                            if (Solution.Genes[towerInfo.GeneIndex][i] > 0)
                            {
                                lastHeight += towerInfo.TowerSectionLength * Solution.Genes[towerInfo.GeneIndex][i];
                                if (i > timeIndex)
                                { break; }
                            }
                        }
                        if (lastHeight > result)
                            result = lastHeight;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// <para>尝试提升塔吊</para>
        /// </summary>
        /// <param name="towerId">尝试提升的塔吊</param>
        /// <param name="collisionIds">可能与之发生碰撞的塔吊Id</param>
        /// <param name="liftTime">提升时间</param>
        /// <param name="afterLift">提升后的高度</param>
        /// <returns>true 可以提升，false 提升失败</returns>
        private bool TryLifting(List<int> collisionIds, DateTime liftTime, double afterLift)
        {
            var towerChargeDic = TowerChargeHelpers.ToDictionary(x => x.TowerId);
            foreach (int collision in collisionIds)
            {
                towerChargeDic.TryGetValue(collision, out var towerInfo);
                BuildingProcessing thisBuilding = Buildings[towerInfo.BuildingId];
                TowerCrane thisTower = Towers[towerInfo.TowerId];
                DateTime temTime = thisBuilding.Process.Keys.LastOrDefault(x => x <= liftTime);
                int timeIndex = thisBuilding.Process.Keys.ToList().IndexOf(temTime);
                if (!(thisTower.StartTime > liftTime || thisTower.EndTime < liftTime))
                {
                    //寻找当前提升时间，被检测塔吊的上一次提升后高度和下一次提升后高度
                    double lastHeight = towerInfo.TowerStartHeight;
                    for (int i = 0; i <= timeIndex; i++)
                    {
                        lastHeight += towerInfo.TowerSectionLength * Solution.Genes[towerInfo.GeneIndex][i];
                    }
                    double nextHeight = lastHeight;
                    for (int i = timeIndex + 1; i < Solution.Genes[towerInfo.GeneIndex].Count; i++)
                    {
                        int num = Solution.Genes[towerInfo.GeneIndex][i];
                        if (num != 0)
                        {
                            nextHeight += num * towerInfo.TowerSectionLength;
                            break;
                        }
                    }
                    if (BetweenSecrity(lastHeight, afterLift) || BetweenSecrity(nextHeight, afterLift))
                    {
                        return false;
                    }
                }
            }
            return true;

        }

        /// <summary>
        /// 检测给定高度B是否在给定高度A的安全范围内
        /// </summary>
        /// <param name="heightA"></param>
        /// <param name="heightB"></param>
        /// <returns>
        /// <para>true B在A的安全范围内</para>
        /// <para>false B不在A的安全范围内</para>
        /// </returns>
        private bool BetweenSecrity(double heightA, double heightB)
        {
            var a = heightA + GetSecurityHeight();
            var b = heightA - GetSecurityHeight();
            if (heightB > b && heightB < a)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 找到给定塔吊Id中，最高的一个塔吊的高度
        /// </summary>
        /// <param name="towerIds"></param>
        /// <returns></returns>
        private double FindHighest(List<int> towerIds)
        {
            double result = 0;
            towerIds.ForEach(x =>
            {
                if (TowerChargeHelpers.ToDictionary(x => x.TowerId).TryGetValue(x, out var towerChargeHelper))
                {
                    double data = 0;
                    data = towerChargeHelper.TowerStartHeight;
                    Solution.Genes[towerChargeHelper.GeneIndex].ForEach(g =>
                    {
                        if (g != 0)
                        {
                            data += g * towerChargeHelper.TowerSectionLength;
                        }
                    });
                    if (data > result)
                        result = data;
                }
            });
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
        /// 从给定的塔吊中，找出最早完成的一个
        /// </summary>
        /// <param name="towerIds"></param>
        /// <returns></returns>
        private int GetLatestTower(List<int> towerIds)
        {
            List<int> ids = towerIds.Except(TowerChargeHelpers.Select(x => x.TowerId)).ToList();
            int result = 0;
            if (ids != null && ids.Count != 0)
            {
                result = FinishTimeTowers.Where(x => ids.Contains(x.TowerId)).OrderByDescending(x => x.Time).ToList().FirstOrDefault().TowerId;
            }
            else
            {
                List<int> existedTowerIds = TowerChargeHelpers.Select(x => x.TowerId).ToList();
                result = FinishTimeTowers.Where(x => !existedTowerIds.Contains(x.TowerId)).OrderByDescending(x => x.Time).ToList().FirstOrDefault().TowerId;
            }
            return result;
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


        public void PrintSolution()
        {
            Towers.ToList().ForEach(tower =>
            {
                var charge = TowerChargeHelpers.Where(x => x.TowerId == tower.Value.Id).FirstOrDefault();
                var building = Buildings[charge.BuildingId];
                Console.WriteLine("塔吊:" + tower.Value.Code + "\t" + "初始高度:" + charge.TowerStartHeight + "\t" + "塔吊主控楼宇:" + building.BuildingCode);

                var process = building.Process.ToList();
                for (int floorId = 0; floorId < process.Count; floorId++)
                {
                    double towerHeight = charge.TowerStartHeight;
                    for (int geneIndex = 0; geneIndex <= floorId; geneIndex++)
                    {
                        towerHeight += charge.TowerSectionLength * Solution.Genes[charge.GeneIndex][geneIndex];
                    }
                    Console.WriteLine("时间:" + process[floorId].Key.ToString("yyyyMMdd") + "\t" + "楼宇高度:" + process[floorId].Value + "\t" + "塔吊高度:" + towerHeight);
                }
            });
        }

    }

}
