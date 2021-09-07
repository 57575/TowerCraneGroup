using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Entities;

namespace TowerCraneGroup.SolutionModels
{
    public class Population
    {
        /// <summary>
        /// 种群规模
        /// </summary>
        private int PopulationSize { get; set; }

        private List<Individual> Individuals { get; set; }

        public Population(int popSize, Dictionary<int, TowerCrane> towerInfo, Dictionary<int, int> towerCharge)
        {
            PopulationSize = popSize;
            for (int i = 0; i > popSize; i++)
            {
                Individuals.Add(new Individual(towerInfo, towerCharge));
            }
        }
    }
}
