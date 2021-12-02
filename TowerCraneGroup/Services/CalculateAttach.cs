using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Entities;
using TowerCraneGroup.InputModels;
using TowerCraneGroup.SolutionModels;

namespace TowerCraneGroup.Services
{
    public static class CalculateAttach
    {
        public static List<TowerAttach> CalculateAndPrint(Individual solution, List<TowerCrane> towers, List<TowerChargeHelper> towerChargeHelpers, Dictionary<int, BuildingProcessing> buildingsDic)
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
                result.AttachDetails.AddRange(Calculate(solution, charge, building, towerDic[charge.TowerId].IndependentHeight));
                results.Add(result);
            }

            Print(results);

            return results;
        }

        private static List<TowerAttachDetail> Calculate(Individual solution, TowerChargeHelper charge, BuildingProcessing building, double independentHeight)
        {
            List<TowerAttachDetail> results = new List<TowerAttachDetail>();
            int floorIndex = 0;
            double currentTowerHeight = charge.TowerStartHeight;
            solution.Genes[charge.GeneIndex].ForEach(gene =>
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
