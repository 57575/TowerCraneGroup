using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.SolutionModels;

namespace TowerCraneGroup.ViewModels.Crane
{
    public class TowerCraneGroupSolution
    {
        public List<TowerLiftCondition> LiftConditions { get; set; }
        public List<TowerAttach> TowerAttaches { get; set; }
    }
}
