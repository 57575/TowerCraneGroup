using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TowerCraneGroup.ViewModels.Crane;
using TowerCraneGroup.InputModels.Crane;
using TowerCraneGroup.InputModels.Process;
using TowerCraneGroup.SolutionModels;

namespace TowerCraneGroup.Services
{
    internal static class OutPutService
    {
        internal static TowerCraneGroupSolution OutputSolution(List<List<int>> genes, List<TowerCrane> towers, List<TowerChargeHelper> towerChargeHelpers, Dictionary<int, BuildingProcessing> buildingsDic)
        {
            TowerCraneGroupSolution result = new TowerCraneGroupSolution
            {
                LiftConditions = OutputTowerLiftCondition(genes, towers, towerChargeHelpers, buildingsDic),
                TowerAttaches = CalculateAttach.Caleculate(genes, towers, towerChargeHelpers, buildingsDic)
            };
            return result;
        }

        private static List<TowerLiftCondition> OutputTowerLiftCondition(List<List<int>> genes, List<TowerCrane> towers, List<TowerChargeHelper> towerChargeHelpers, Dictionary<int, BuildingProcessing> buildingsDic)
        {
            List<TowerLiftCondition> results = new List<TowerLiftCondition>();
            foreach (TowerCrane tower in towers)
            {
                TowerChargeHelper charge = towerChargeHelpers.FirstOrDefault(x => x.TowerId == tower.Id);
                BuildingProcessing building = buildingsDic[charge.BuildingId];
                List<KeyValuePair<DateTime, double>> process = building.Process.ToList();
                double towerHeight = charge.TowerStartHeight;
                for (int floorIndex = 0; floorIndex < process.Count; floorIndex++)
                {
                    towerHeight += charge.TowerSectionLength * genes[charge.GeneIndex][floorIndex];
                    TowerLiftCondition result = new TowerLiftCondition
                    {
                        BuildingId = building.Id,
                        BuildingCode = building.BuildingCode,
                        BuildingHeight = process[floorIndex].Value,
                        LiftSectionNumber = genes[charge.GeneIndex][floorIndex],
                        Time = process[floorIndex].Key,
                        TowerId = tower.Id,
                        TowerCode = tower.Code,
                        TowerHeight = towerHeight
                    };
                    results.Add(result);
                }
            }

            return results;
        }
    }
}
