using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Entities;
using TowerCraneGroup.InputModels;
using Newtonsoft.Json.Serialization;

namespace TowerCraneGroup.SolutionModels
{
    public class Individual
    {

        private static readonly double SecurityHeight = 8.00;
        /// <summary>
        /// 基因组
        /// </summary>
        public List<List<int>> Genes { get; set; }
        /// <summary>
        /// 适应度评分
        /// </summary>
        [JsonRequired]
        public int Fitness { get; private set; }

        /// <summary>
        /// 每个塔吊高度的关键楼宇信息、塔吊信息和在genes中的index
        /// dictionary的key是index
        /// </summary>
        //public Dictionary<int, TowerChargeHelper> towerChargeDic { get; set; }
        public Individual()
        {
            Genes = new List<List<int>>();
        }
        public Dictionary<int, int> InitialIndividual(Dictionary<int, TowerCrane> towerInfo, List<TowerChargeBuilding> towerCharge, Dictionary<int, BuildingProcessing> buildings)
        {
            Random random = new Random();
            Genes = new List<List<int>>();

            Dictionary<int, int> results = new Dictionary<int, int>();
            //对每个塔吊生产基因段，组合成染色体
            foreach (var tower in towerCharge)
            {
                int max = towerInfo[tower.TowerId].LiftSectionNumDic.Values.Max();
                int length = buildings[tower.BuildingId].Process.Count;
                int geneIndex = GenerateATowerGene(random, max, length);
                results.Add(tower.TowerId, geneIndex);
            }
            Fitness = 0;
            return results;
        }
        public Individual(Dictionary<int, TowerCrane> towerInfo, List<TowerChargeBuilding> towerCharge, Dictionary<int, BuildingProcessing> buildings)
        {
            Random random = new Random();
            Genes = new List<List<int>>();
            //TowerChargeDic = new Dictionary<int, TowerChargeHelper>();

            //对每个塔吊生产基因段，组合成染色体
            foreach (var tower in towerCharge)
            {
                int max = towerInfo[tower.TowerId].LiftSectionNumDic.Values.Max();
                int length = buildings[tower.BuildingId].Process.Count;
                int geneIndex = GenerateATowerGene(random, max, length);
                //TowerChargeHelper aHelper = new TowerChargeHelper(geneIndex, tower, length, towerInfo[tower.TowerId].SectionHeight, towerInfo[tower.TowerId].StartHeight);
                //TowerChargeDic.Add(aHelper.GeneIndex, aHelper);
            }
            Fitness = 0;
        }
        /// <summary>
        /// 按照给定最大值和基因片段长度生成一个塔吊的基因片段
        /// </summary>
        /// <param name="random"></param>
        /// <param name="max">基因最大值，单次提升最大值（节数）</param>
        /// <param name="length">基因长度，塔吊可提升次数</param>
        private int GenerateATowerGene(Random random, int max, int length)
        {
            List<int> aTowerGene = new List<int>();
            for (int i = 0; i < length; i++)
            {
                if (random.Next(0, 4) == 0)
                {
                    aTowerGene.Add(random.Next(0, max));
                }
                else
                {
                    aTowerGene.Add(0);
                }
            }
            Genes.Add(aTowerGene);
            return Genes.IndexOf(aTowerGene);
        }

        /// <summary>
        /// 计算个体的适应度值
        /// </summary>
        /// <param name="collisionsDic"></param>
        /// <param name="buildingDic"></param>
        /// <param name="towerDic"></param>
        public void CalculateFitness(
            Dictionary<int, List<CollisionRelation>> collisionsDic,
            Dictionary<int, BuildingProcessing> buildingDic,
            Dictionary<int, TowerCrane> towerDic,
            Dictionary<int, TowerChargeHelper> towerChargeDic)
        {
            Fitness = 0;
            Genes.ForEach(x =>
            {
                Fitness += 2 * x.Count(y => y != 0);
            });
            Fitness +=
                  40 * BuildingCollisionNumber(collisionsDic, buildingDic, towerDic, towerChargeDic)
                + 10 * TowerCollisionNumber(collisionsDic, buildingDic, towerDic, towerChargeDic)
                + 10 * LiftingNumber(towerDic, towerChargeDic)
                + 5 * HigherNumber(buildingDic, towerChargeDic)
                + 20 * LowThanNeedNumber(buildingDic, towerChargeDic);
            //var c = HigherNumber(buildingDic, towerChargeDic);
            //var d = LiftingNumber(towerDic, towerChargeDic);
            //var e = LowThanNeedNumber(buildingDic, towerChargeDic);
            //var b = BuildingCollisionNumber(collisionsDic, buildingDic, towerDic, towerChargeDic);
            //var a = TowerCollisionNumber(collisionsDic, buildingDic, towerDic, towerChargeDic);
        }
        /// <summary>
        /// 是否有不允许的情况
        /// </summary>
        /// <param name="collisionsDic"></param>
        /// <param name="buildingDic"></param>
        /// <param name="towerDic"></param>
        /// <param name="towerChargeDic"></param>
        /// <returns></returns>
        public bool CalculateForbidden(Dictionary<int, List<CollisionRelation>> collisionsDic,
            Dictionary<int, BuildingProcessing> buildingDic,
            Dictionary<int, TowerCrane> towerDic,
            Dictionary<int, TowerChargeHelper> towerChargeDic)
        {
            if (TowerCollisionNumber(collisionsDic, buildingDic, towerDic, towerChargeDic) == 0
                && BuildingCollisionNumber(collisionsDic, buildingDic, towerDic, towerChargeDic) == 0
                && LowThanNeedNumber(buildingDic, towerChargeDic) == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        private int BuildingCollisionNumber
            (
            Dictionary<int, List<CollisionRelation>> collisionsDic,
            Dictionary<int, BuildingProcessing> buildingDic,
            Dictionary<int, TowerCrane> towerDic,
            Dictionary<int, TowerChargeHelper> towerChargeDic
            )
        {
            int result = 0;
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
                                        result += 1;
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


            return result;
        }

        /// <summary>
        /// 碰撞次数
        /// </summary>
        /// <param name="collisionsDic"></param>
        /// <param name="buildingDic"></param>
        /// <returns></returns>
        private int TowerCollisionNumber(
            Dictionary<int, List<CollisionRelation>> collisionsDic,
            Dictionary<int, BuildingProcessing> buildingDic,
            Dictionary<int, TowerCrane> towerDic,
            Dictionary<int, TowerChargeHelper> towerChargeDic
            )
        {
            int result = 0;
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
                                    if (CalculateTowerCollision(beforeLiftHeight, afterLiftHeight, thisFloorFinish, towerId, buildingDic, towerChargeDic))
                                    {
                                        result += 1;
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

            return result;
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
        private bool CalculateTowerCollision(double beforeLift, double afterLift, DateTime thisFloorFinish, int towerId, Dictionary<int, BuildingProcessing> buildingDic, Dictionary<int, TowerChargeHelper> towerChargeDic)
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
                //被检测塔吊提升前高于本塔吊                
                if (height > beforeLift)
                {
                    if (height >= afterLift + SecurityHeight)
                        return false;
                    else
                        return true;
                }
                //被检测塔吊低于本塔吊
                else
                {
                    return false;
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

                    //被检测塔吊是高位塔吊
                    if (beijianceqian > beforeLift)
                    {
                        if (beijiancehou >= afterLift + SecurityHeight)
                            return false;
                        else
                            return true;
                    }
                    //被检测塔吊是低位塔吊
                    else//beijianceqian <= beforeLift
                    {
                        if (beijiancehou + SecurityHeight <= afterLift)
                            return false;
                        else
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
                    //被检测塔吊是高位塔吊
                    if (height > beforeLift)
                    {
                        if (height >= afterLift + SecurityHeight)
                            return false;
                        else//height<afterlift+se
                            return true;
                    }
                    //被检测塔吊是低位塔吊
                    else//height<beforeLift
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 提升节数大于其最大提升高度的次数
        /// </summary>
        /// <returns></returns>
        private int LiftingNumber(Dictionary<int, TowerCrane> towerDic, Dictionary<int, TowerChargeHelper> towerChargeDic)
        {
            int result = 0;
            for (int i = 0; i < towerChargeDic.Count; i++)
            {
                if (towerChargeDic[i].FloorNumber != Genes[i].Count)
                    throw new Exception("发生未知错误");

                //单个塔吊的提升次数，初始为第一次提升
                int liftingIndex = 1;
                TowerCrane thisTower = towerDic[towerChargeDic[i].TowerId];
                for (int j = 0; j < Genes[i].Count; j++)
                {
                    if (Genes[i][j] != 0)
                    {
                        int max = 0;
                        //查找当前的提升次数
                        if (thisTower.LiftSectionNumDic.TryGetValue(liftingIndex, out int maxSectionNum))
                        {
                            max = maxSectionNum;
                        }
                        else
                        {
                            max = thisTower.LiftSectionNumDic.OrderBy(x => x.Key).LastOrDefault().Value;
                        }
                        //如果当前提升节数大于允许提升节数，该次数加1
                        if (Genes[i][j] > max)
                        {
                            result++;
                        }
                        liftingIndex++;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 提升后大于塔吊所需最终高度的节数
        /// </summary>
        /// <returns></returns>
        private int HigherNumber(Dictionary<int, BuildingProcessing> buildingDic, Dictionary<int, TowerChargeHelper> towerChargeDic)
        {
            int result = 0;
            for (int i = 0; i < towerChargeDic.Count; i++)
            {
                if (towerChargeDic[i].FloorNumber != Genes[i].Count)
                    throw new Exception("发生未知错误");

                double finalHeighth = (buildingDic[towerChargeDic[i].BuildingId].GetFinalStructureHeighth()) + SecurityHeight;

                //计算出该塔吊提升后大于塔吊所需最终高度的节数
                int needSectionNum = (int)Math.Ceiling(finalHeighth / towerChargeDic[i].TowerSectionLength);

                int thisSectionNum = Genes[i].Where(x => x > 0).Sum();

                if (thisSectionNum > needSectionNum)
                {
                    result += (thisSectionNum - needSectionNum);
                }
            }
            return result;
        }

        private int LowThanNeedNumber(Dictionary<int, BuildingProcessing> buildingDic, Dictionary<int, TowerChargeHelper> towerChargeDic)
        {
            int result = 0;
            for (int i = 0; i < towerChargeDic.Count; i++)
            {
                int sectionNum = Genes[towerChargeDic[i].GeneIndex].Where(x => x > 0).Sum();
                double buildingHeight = buildingDic[towerChargeDic[i].BuildingId].Process.Values.Max();
                double difference = (buildingHeight + SecurityHeight) - sectionNum * towerChargeDic[i].TowerSectionLength;
                if (difference > 0)
                {
                    double percentage = (difference / (buildingHeight + SecurityHeight)) * 100;
                    if (percentage < 20)
                        result += 1;
                    else if (percentage < 50)
                        result += 2;
                    else
                        result += 3;
                }
            }
            return result;
        }

        /// <summary>
        /// 随机变异一个基因片段中的基因
        /// </summary>
        public void Mutation()
        {
            Random random = new Random();
            int mutationGene = random.Next(0, Genes.Count);
            int mutationPoint = random.Next(0, Genes[mutationGene].Count);
            if (random.Next(0, 10) < 5)
            {
                if (Genes[mutationGene][mutationPoint] > 0)
                {
                    Genes[mutationGene][mutationPoint] -= 1;
                }
            }
            else
            {
                Genes[mutationGene][mutationPoint] += 1;
            }
        }
        /// <summary>
        /// 获取指定基因片段的长度
        /// </summary>
        /// <param name="geneIndex"></param>
        /// <returns></returns>
        public int GetGeneLength(int geneIndex)
        {
            return Genes[geneIndex].Count;
        }
        /// <summary>
        /// 获取指定位置的基因
        /// </summary>
        /// <param name="geneIndex">gene片段序号</param>
        /// <param name="point">gene片段中的点位</param>
        /// <returns></returns>
        public int GetGene(int geneIndex, int point)
        {
            return Genes[geneIndex][point];
        }
        /// <summary>
        /// 将指定位置的gene设置为指定值
        /// </summary>
        /// <param name="geneIndex">gene片段序号</param>
        /// <param name="point">gene片段中的点位</param>
        /// <param name="gene">gene值</param>
        public void SetGene(int geneIndex, int point, int gene)
        {
            Genes[geneIndex][point] = gene;
        }
        public int GetGenesNum()
        {
            return Genes.Count;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
