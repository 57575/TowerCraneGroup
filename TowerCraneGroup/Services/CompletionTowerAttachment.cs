using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TowerCraneGroup.InputModels.Crane;
using TowerCraneGroup.InputModels.Process;
using TowerCraneGroup.SolutionModels;

namespace TowerCraneGroup.Services
{
    internal static class CompletionTowerAttachment
    {
        internal static void CompletionTower(List<TowerCrane> towers, List<BuildingProcessing> buildings, List<TowerChargeBuilding> TowerChargeBuildings)
        {
            foreach (TowerCrane tower in towers)
            {
                int sectionNumToIndependent = (int)Math.Floor((tower.IndependentHeight - tower.StartHeight) / tower.SectionHeight);
                if (sectionNumToIndependent > 0)
                {
                    tower.LiftSectionNumDic.Keys.OrderByDescending(key => key).ToList().ForEach(key =>
                    {
                        int value = tower.LiftSectionNumDic[key];
                        tower.LiftSectionNumDic.Remove(key);
                        tower.LiftSectionNumDic.Add(++key, value);
                    });
                    tower.LiftSectionNumDic.Add(1, sectionNumToIndependent);
                }
                int? buildingId = TowerChargeBuildings.FirstOrDefault(y => y.TowerId == tower.Id)?.BuildingId;
                if (buildingId != null)
                {
                    int floorNum = buildings.FirstOrDefault(tower => tower.Id == buildingId.Value).Process.Keys.Count;
                    if (floorNum > tower.LiftSectionNumDic.Count)
                    {
                        int lastListIndetower = tower.LiftSectionNumDic.Keys.OrderBy(key => key).LastOrDefault();
                        int lastLiftSectionNum = tower.LiftSectionNumDic[lastListIndetower];
                        for (int i = lastListIndetower + 1; i <= floorNum; i++)
                            tower.LiftSectionNumDic.Add(i, lastLiftSectionNum);
                    }
                }


            }
        }
    }
}
