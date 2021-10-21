using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TowerCraneGroup.Entities;
using TowerCraneGroup.InputModels;
using TowerCraneGroup.SolutionModels;

namespace TowerCraneGroup.Services
{
    public class TrueServe
    {
        private int GenerationCount { get; set; }
        private Population Population { get; set; }
        private int PopSize { get; set; }
        private List<int> BestTenInHistory = new List<int>();

        public TrueServe(int popSize)
        {
            PopSize = popSize;
            GenerationCount = 0;
        }
        public void RunServe(string buildingPath, string towerPath, string attachPath)
        {
            RunGreedyServe(buildingPath, towerPath, attachPath);
        }

        private void RunGreedyServe(string buildingPath, string towerPath, string attachPath)
        {
            List<BuildingProcessing> buildings = ReadExcelService.ReadBuilding(buildingPath);
            var buildingDic = buildings.ToDictionary(x => x.Id);
            List<TowerCrane> towers = ReadExcelService.ReadTowers(towerPath, attachPath);
            var towerDic = towers.ToDictionary(x => x.Id);
            List<CollisionRelation> collision = ReadExcelService.ReadCollision(towerPath, buildings.ToDictionary(x => x.BuildingCode, x => x.Id), towers.ToDictionary(x => x.Code, x => x.Id));
            Dictionary<int, List<CollisionRelation>> collisionDic = collision.GroupBy(x => x.TowerId).ToDictionary(x => x.Key, x => x.ToList());
            List<TowerChargeBuilding> TowerChargeBuildings = CalculateHelper.CalculteTowerChargeBuilding(buildings, collisionDic, towers.ToDictionary(x => x.Id));
            ReadExcelService.InitialTowers(towers, buildings, TowerChargeBuildings);
            Console.WriteLine("完成数据准备");

            GreedyAlgorithmService greedyAlgorithmService = new GreedyAlgorithmService(towerDic, TowerChargeBuildings, buildingDic, collisionDic);

            Individual greedyIndividual = greedyAlgorithmService.RunService();
            Population = new Population(PopSize, towerDic, greedyAlgorithmService.GetTowerChargeHelpers(), buildingDic, collisionDic, false);
            Population.AddIndividual(greedyIndividual);

            var towerChargeHelper = greedyAlgorithmService.GetTowerChargeHelpers().ToDictionary(x => x.GeneIndex);

            Individual solution = greedyIndividual;
            while (
                GenerationCount < 10000
                || BestTenInHistory.Distinct().Count() != 1
                || solution.CalculateForbidden(collisionDic, buildingDic, towerDic, towerChargeHelper)
                )
            {
                Population offspring = new Population(PopSize, towerDic, greedyAlgorithmService.GetTowerChargeHelpers(), buildingDic, collisionDic, true);
                Population.CalculateFitness(collisionDic, buildingDic, towerDic, towerChargeHelper);
                Individual best = new Individual();
                string bestJson = Population.GetFittest().Serialize();
                best = JsonConvert.DeserializeObject<Individual>(bestJson);
                solution = best;
                if (BestTenInHistory.Count < 10000)
                {
                    BestTenInHistory.Add(Population.GetFittest().Fitness);
                }
                else
                {
                    BestTenInHistory[BestTenInHistory.IndexOf(BestTenInHistory.Max())] = best.Fitness;
                }
                offspring.AddIndividual(best);
                Random random = new Random();
                for (int i = 1; i < PopSize || offspring.GetPopulationSize() < PopSize; i += 2)
                {
                    Individual son1, son2;
                    (son1, son2) = Population.Crossover();

                    int mutationNum = (int)Math.Floor(son1.Genes.Sum(x => x.Count) * 0.1);

                    if (random.Next(0, PopSize) < Math.Ceiling((PopSize / 1.0)))
                    {
                        son1.Mutation(mutationNum, buildingDic, towerChargeHelper);
                    }
                    if (random.Next(0, PopSize) < Math.Ceiling((PopSize / 1.0)))
                    {
                        son2.Mutation(mutationNum, buildingDic, towerChargeHelper);
                    }
                    Individual newSon1 = new Individual();
                    newSon1 = JsonConvert.DeserializeObject<Individual>(son1.Serialize());
                    Individual newSon2 = new Individual();
                    newSon2 = JsonConvert.DeserializeObject<Individual>(son2.Serialize());
                    if (newSon1.CalculateBaseConstraint(buildingDic, towerChargeHelper))
                    {
                        offspring.AddIndividual(newSon1);
                    }
                    if (newSon2.CalculateBaseConstraint(buildingDic, towerChargeHelper))
                    {
                        offspring.AddIndividual(newSon2);
                    }
                }
                //offspring.CalculateFitness();
                Population = offspring;
                GenerationCount++;
                Console.WriteLine("Generation:" + GenerationCount + "||BestFitness:" + best.Fitness);
            }
            string solutionStr = JsonConvert.SerializeObject(solution);
            Console.WriteLine("Solution:" + solutionStr);
            solution.Print(towers, greedyAlgorithmService.GetTowerChargeHelpers(), buildingDic);
            CalculateAttach.CalculateAndPrint(solution, towers, greedyAlgorithmService.GetTowerChargeHelpers(), buildingDic);
            Console.WriteLine("DONE");

        }
    }
}
