using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TowerCraneGroup.InputModels.Process;
using TowerCraneGroup.SolutionModels;
using TowerCraneGroup.InputModels.Crane;
using Newtonsoft.Json;
using TowerCraneGroup.ViewModels.Crane;

namespace TowerCraneGroup.Services
{
    public class SolutionService
    {
        private int GenerationCount { get; set; }
        private Population Population { get; set; }
        private int PopSize { get; set; }
        private int GenerationNumber { get; set; }
        private List<int> BestTenInHistory = new List<int>();
        public SolutionService(int popSize, int genrationNum)
        {
            PopSize = popSize;
            GenerationNumber = genrationNum;
            GenerationCount = 0;
        }
        public TowerCraneGroupSolution Excute(List<BuildingProcessing> buildingProcessings, List<CollisionRelation> collisions, List<TowerCrane> towerCranes, List<TowerAttachRelation> attaches, Dictionary<int, int> order = null)
        {
            Dictionary<int, TowerCrane> towerDic = towerCranes.ToDictionary(x => x.Id);
            Dictionary<int, List<CollisionRelation>> collisionDic = collisions.GroupBy(x => x.TowerId).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<int, BuildingProcessing> buildingDic = buildingProcessings.ToDictionary(x => x.Id);
            List<TowerChargeBuilding> TowerChargeBuildings = CalculateTowerCharge.CalculteTowerChargeBuilding(buildingProcessings, collisionDic, towerDic);
            CompletionTowerAttachment.CompletionTower(towerCranes, buildingProcessings, TowerChargeBuildings);

            GreedyAlgorithmService greedyAlgorithmService = new GreedyAlgorithmService(towerDic, TowerChargeBuildings, attaches, buildingDic, collisionDic, order);

            Individual greedyIndividual = greedyAlgorithmService.RunService();
            Population = new Population(PopSize, towerDic, greedyAlgorithmService.GetTowerChargeHelpers(), buildingDic, collisionDic, false);
            Population.AddIndividual(greedyIndividual);

            Dictionary<int, TowerChargeHelper> towerChargeHelper = greedyAlgorithmService.GetTowerChargeHelpers().ToDictionary(x => x.GeneIndex);

            Individual solution = greedyIndividual;
            while (
                GenerationCount < GenerationNumber
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
                if (BestTenInHistory.Count < GenerationNumber)
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
                Population = offspring;
                GenerationCount++;

            }

            return OutPutService.OutputSolution(solution.Genes, towerCranes, greedyAlgorithmService.GetTowerChargeHelpers(), buildingDic);
        }
    }
}
