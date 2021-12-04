using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.InputModels;
using TowerCraneGroup.InputModels.Crane;
using TowerCraneGroup.InputModels.Process;
using TowerCraneGroup.SolutionModels;
using TowerCraneGroup.ViewModels.Crane;

namespace TowerCraneGroup.Services
{
    internal static class CalculateAttach
    {
        internal static List<TowerAttach> Caleculate(List<List<int>> genes, List<TowerCrane> towers, List<TowerChargeHelper> towerChargeHelpers, Dictionary<int, BuildingProcessing> buildingsDic)
        {
            List<TowerAttach> results = new List<TowerAttach>();

            Dictionary<int, TowerCrane> towerDic = towers.ToDictionary(x => x.Id);
            foreach (TowerCrane tower in towers)
            {
                TowerChargeHelper charge = towerChargeHelpers.FirstOrDefault(x => x.TowerId == tower.Id);
                double currentTowerHeight = charge.TowerStartHeight;
                BuildingProcessing building = buildingsDic[charge.BuildingId];

                TowerAttach result = new TowerAttach()
                {
                    TowerId = tower.Id,
                    TowerCode = tower.Code,
                    BuildingId = charge.BuildingId,
                    BuildingCode = building.BuildingCode
                };
                result.AttachDetails.AddRange(Calculate(genes, charge, building, towerDic[charge.TowerId].IndependentHeight));
                results.Add(result);
            }
            return results;
        }

        private static List<TowerAttachDetail> Calculate(List<List<int>> genes, TowerChargeHelper charge, BuildingProcessing building, double independentHeight)
        {
            List<TowerAttachDetail> results = new List<TowerAttachDetail>();
            int floorIndex = 0;
            double currentTowerHeight = charge.TowerStartHeight;
            genes[charge.GeneIndex].ForEach(gene =>
            {
                floorIndex++;
                currentTowerHeight += gene * charge.TowerSectionLength;
                if (gene != 0 && currentTowerHeight > independentHeight)
                {
                    if (floorIndex > 3)
                    {
                        TowerAttachDetail detail = new TowerAttachDetail()
                        {
                            Index = floorIndex - 3,
                            DateTime = building.Process.ToList()[floorIndex - 3].Key,
                            Height = building.Process.ToList()[floorIndex - 3].Value
                        };
                        results.Add(detail);
                    }
                }
            });
            return results;
        }

        private static void Print(List<TowerAttach> attaches)
        {
            foreach (var attach in attaches)
            {
                Console.WriteLine("塔吊编号:" + attach.TowerCode + "\t" + "主控楼宇:" + attach.BuildingCode);
                for (int i = 0; i < attach.AttachDetails.Count; i++)
                {
                    Console.WriteLine($"第{i + 1}次附墙,附墙高度{attach.AttachDetails[i].Height},附墙时间:{attach.AttachDetails[i].DateTime.ToString("yyyyMMdd")}");
                }
                if (attach.AttachDetails.Count == 0)
                {
                    Console.WriteLine("塔吊最终高度小于独立高度,无需附墙");
                }
            }
        }
    }
}
