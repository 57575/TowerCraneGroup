using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Entities;
using TowerCraneGroup.InputModels;

namespace TowerCraneGroup.SolutionModels
{
    public class Individual
    {
        /// <summary>
        /// 基因组
        /// </summary>
        private List<List<int>> Genes { get; set; }

        /// <summary>
        /// 基因长度
        /// </summary>
        private int GenLength { get; set; }
        /// <summary>
        /// 适应度评分
        /// </summary>
        private int Fitness { get; set; }

        /// <summary>
        /// 每个塔吊高度的关键楼宇信息、塔吊信息和在genes中的index
        /// dictionary的key是index
        /// </summary>
        private Dictionary<int, TowerChargeHelper> TowerChargeDic { get; set; }

        public Individual(Dictionary<int, TowerCrane> towerInfo, List<TowerChargeBuilding> towerCharge, Dictionary<int, BuildingProcessing> buildings)
        {
            Random random = new Random();
            Genes = new List<List<int>>();

            //对每个塔吊生产基因段，组合成染色体
            foreach (var tower in towerCharge)
            {
                int max = towerInfo[tower.TowerId].LiftSectionNumDic.Values.Max();
                int length = buildings[tower.BuildingId].Process.Count;
                int geneIndex = GenerateATowerGene(random, max, length);
                TowerChargeHelper aHelper = new TowerChargeHelper(geneIndex, tower, length, towerInfo[tower.TowerId].SectionHeight);
                TowerChargeDic.Add(aHelper.GeneIndex, aHelper);
            }
            GenLength = Genes.Sum(x => x.Count);
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


        public int GetFitness()
        { return Fitness; }
        public void CalculateFitness()
        {
            Fitness = 0;
            Genes.ForEach(x =>
            {
                Fitness = +x.Count(y => y != 0);
            });

        }
        /// <summary>
        /// 碰撞次数
        /// </summary>
        /// <param name="collisionsDic"></param>
        /// <param name="buildingDic"></param>
        /// <returns></returns>
        private int CollisionNumber(
            Dictionary<int, List<CollisionRelation>> collisionsDic,
            Dictionary<int, BuildingProcessing> buildingDic,
            Dictionary<int, TowerCrane> towerDic
            )
        {
            int result = 0;
            //i is genes index
            for (int i = 0; i < TowerChargeDic.Count; i++)
            {
                //简单验证
                if (TowerChargeDic[i].FloorNumber != Genes[i].Count)
                    throw new Exception("发生未知错误");
                if (collisionsDic.TryGetValue(TowerChargeDic[i].TowerId, out List<CollisionRelation> col))
                {
                    List<int> collisionBuildingIds = col.Where(x => x.RelationType == CollisionRelationType.塔吊与楼宇).Select(x => x.CollisionId).ToList();
                    List<int> collisionTowerIds = col.Where(x => x.RelationType == CollisionRelationType.塔吊与塔吊).Select(x => x.CollisionId).ToList();
                    TowerCrane thisTower = towerDic[TowerChargeDic[i].TowerId];
                    double thisTowerHeight = thisTower.StartHeight;
                    Genes[i].ForEach(x =>
                    {
                        if (x != 0)
                        {
                            double beforeLiftHeight = thisTowerHeight;
                            double afterLiftHeight = thisTowerHeight + thisTower.SectionHeight * x;
                            //检测塔吊和所负责楼宇的碰撞
                            collisionBuildingIds.ForEach(buil =>
                            {

                            });
                            //检测塔吊和塔吊的碰撞
                            collisionTowerIds.ForEach(tower =>
                            {

                            });
                        }
                    });
                }
                else
                {
                    throw new Exception("发生未知错误");
                }

            }

            return result;
        }

        //private int CalculateCollision(CollisionRelationType relationType)
        //{

        //}


        /// <summary>
        /// 提升节数大于其最大提升高度的次数
        /// </summary>
        /// <returns></returns>
        private int LiftingNumber(Dictionary<int, TowerCrane> towerDic)
        {
            int result = 0;
            for (int i = 0; i < TowerChargeDic.Count; i++)
            {
                if (TowerChargeDic[i].FloorNumber != Genes[i].Count)
                    throw new Exception("发生未知错误");

                //单个塔吊的提升次数，初始为第一次提升
                int liftingIndex = 1;
                TowerCrane thisTower = towerDic[TowerChargeDic[i].TowerId];
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
        private int HigherNumber(Dictionary<int, BuildingProcessing> buildingDic)
        {
            int result = 0;
            for (int i = 0; i < TowerChargeDic.Count; i++)
            {
                if (TowerChargeDic[i].FloorNumber != Genes[i].Count)
                    throw new Exception("发生未知错误");

                double finalHeighth = buildingDic[TowerChargeDic[i].BuildingId].GetFinalStructureHeighth();

                //计算出该塔吊提升后大于塔吊所需最终高度的节数
                int needSectionNum = (int)Math.Ceiling(finalHeighth / TowerChargeDic[i].TowerSectionLength);

                int thisSectionNum = Genes[i].Where(x => x > 0).Sum();

                if (thisSectionNum > needSectionNum)
                {
                    result += (thisSectionNum - needSectionNum);
                }
            }
            return result;
        }
    }
}
