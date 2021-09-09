using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Entities;
using TowerCraneGroup.InputModels;

namespace TowerCraneGroup.SolutionModels
{
    public class Population
    {
        /// <summary>
        /// 种群规模
        /// </summary>
        private int PopulationSize { get; set; }
        private List<Individual> Individuals { get; set; }
        private Dictionary<int, TowerCrane> Towers { get; set; }
        private List<TowerChargeBuilding> TowerCharges { get; set; }
        private Dictionary<int, BuildingProcessing> Buildings { get; set; }
        Dictionary<int, List<CollisionRelation>> Collision { get; set; }

        public Population(int popSize, Dictionary<int, TowerCrane> towerInfo, List<TowerChargeBuilding> towerCharge, Dictionary<int, BuildingProcessing> buildings, Dictionary<int, List<CollisionRelation>> collisionsDic, bool empty)
        {
            this.Collision = collisionsDic;
            this.Towers = towerInfo;
            this.TowerCharges = towerCharge;
            this.Buildings = buildings;
            PopulationSize = popSize;
            Individuals = new List<Individual>();
            if (!empty)
            {
                for (int i = 0; i < popSize; i++)
                {
                    Individuals.Add(new Individual(Towers, TowerCharges, Buildings));
                }
            }
        }
        public void CalculateFitness()
        {
            Individuals.ForEach(x =>
            {
                x.CalculateFitness(Collision, Buildings, Towers);
            });
        }
        public Individual GetFittest()
        {
            var result = Individuals.OrderBy(x => x.Fitness).FirstOrDefault();
            return result;
        }
        public void AddIndividual(Individual individual)
        { Individuals.Add(individual); }
        public int GetPopulationSize()
        { return Individuals.Count; }
        /// <summary>
        /// 使用锦标赛策略随机选择两个individual，然后使用两点策略交叉
        /// </summary>
        /// <returns></returns>
        public (Individual, Individual) Crossover()
        {
            Random random = new Random();
            Individual parent1 = TournamentSection();
            Individual parent2 = TournamentSection();

            int crossoverGene = random.Next(0, parent1.GeneNum);
            int crossoverPointStart = random.Next(0, parent1.GetGeneLength(crossoverGene));
            int crossoverPointEnd = random.Next(crossoverPointStart, parent1.GetGeneLength(crossoverGene));
            for (int i = crossoverPointStart; i <= crossoverPointEnd; i++)
            {
                int tem = parent1.GetGene(crossoverGene, i);
                parent1.SetGene(crossoverGene, i, parent2.GetGene(crossoverGene, i));
                parent2.SetGene(crossoverGene, i, tem);
            }
            return (parent1, parent2);
        }
        /// <summary>
        /// 锦标赛选择
        /// </summary>
        /// <returns></returns>
        private Individual TournamentSection()
        {
            Random random = new Random();
            Individual best = null;
            for (int i = 0; i < 2; i++)
            {
                var tem = Individuals[random.Next(0, Individuals.Count)];
                if (best is null || tem.Fitness < best.Fitness)
                    best = tem;
            }
            return best;
        }
    }
}
