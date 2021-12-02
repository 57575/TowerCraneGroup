using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TowerCraneGroup.Entities;
using TowerCraneGroup.InputModels;
using TowerCraneGroup.SolutionModels;

namespace TowerCraneGroup.Services
{
    public class CHJDemo
    {
        public void Run()
        {
            List<BuildingProcessing> buildings = ReadBuildingExcel();
            List<TowerCrane> towers = ReadTowers();
            List<CollisionRelation> collision = ReadCollision(buildings.ToDictionary(x => x.BuildingCode, x => x.Id), towers.ToDictionary(x => x.Code, x => x.Id));
            Dictionary<int, List<CollisionRelation>> collisionDic = collision.GroupBy(x => x.TowerId).ToDictionary(x => x.Key, x => x.ToList());
            List<TowerChargeBuilding> TowerChargeBuildings = CalculateHelper.CalculteTowerChargeBuilding(buildings, collisionDic, towers.ToDictionary(x => x.Id));
            InitialTowers(towers, buildings, TowerChargeBuildings);
            Console.WriteLine("完成数据准备");

            GreedyAlgorithmService greedyAlgorithmService = new GreedyAlgorithmService(towers.ToDictionary(x => x.Id), TowerChargeBuildings, buildings.ToDictionary(x => x.Id), collisionDic);

            Individual individual= greedyAlgorithmService.RunService();

            greedyAlgorithmService.PrintSolution();
        }

        private void InitialTowers(List<TowerCrane> towers, List<BuildingProcessing> buildings, List<TowerChargeBuilding> TowerChargeBuildings)
        {
            towers.ForEach(x =>
            {
                if (x.StartHeight < x.IndependentHeight)
                {
                    int sectionNumToIndependent = (int)Math.Floor((x.IndependentHeight - x.StartHeight) / x.SectionHeight);
                    if (sectionNumToIndependent > 0)
                    {
                        x.LiftSectionNumDic.Keys.OrderByDescending(key => key).ToList().ForEach(key =>
                        {
                            int value = x.LiftSectionNumDic[key];
                            x.LiftSectionNumDic.Remove(key);
                            x.LiftSectionNumDic.Add(++key, value);
                        });
                        x.LiftSectionNumDic.Add(1, sectionNumToIndependent);
                    }
                }

                int? buildingId = TowerChargeBuildings.Where(y => y.TowerId == x.Id).FirstOrDefault()?.BuildingId;
                if (buildingId != null)
                {
                    int floorNum = buildings.Where(x => x.Id == buildingId.Value).First().Process.Keys.Count;
                    if (floorNum > x.LiftSectionNumDic.Count)
                    {
                        int lastListIndex = x.LiftSectionNumDic.Keys.OrderBy(key => key).LastOrDefault();
                        int lastLiftSectionNum = x.LiftSectionNumDic[lastListIndex];
                        for (int i = lastListIndex + 1; i <= floorNum; i++)
                            x.LiftSectionNumDic.Add(i, lastLiftSectionNum);
                    }
                }
            });
        }

        private List<BuildingProcessing> ReadBuildingExcel()
        {
            List<BuildingProcessing> results = new List<BuildingProcessing>();

            FileStream stream = new FileStream(@"D:\work\CraneTowerGroup\施工进度信息表(2).xlsx", FileMode.Open);

            var workbook = new XSSFWorkbook(stream);

            var sheet = workbook.GetSheetAt(0);

            IRow header = sheet.GetRow(0);

            int cellCount = header.LastCellNum;

            string buildingCode = "";
            int buildingId = 1;
            BuildingProcessing building = new BuildingProcessing();
            for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                if (row.Cells.All(r => r.CellType == CellType.Blank)) continue;
                if (string.IsNullOrEmpty(buildingCode))
                {
                    buildingCode = row.GetCell(0).StringCellValue;
                    building = new BuildingProcessing
                    {
                        Id = buildingId,
                        BuildingCode = buildingCode
                    };
                    buildingId++;
                }
                if (buildingCode != row.GetCell(0).StringCellValue)
                {
                    results.Add(building);
                    buildingCode = row.GetCell(0).StringCellValue;
                    building = new BuildingProcessing
                    {
                        Id = buildingId,
                        BuildingCode = buildingCode
                    };
                    buildingId++;
                }

                building.Process.Add(row.GetCell(3).DateCellValue, row.GetCell(2).NumericCellValue);

                if (i == sheet.LastRowNum)
                {
                    results.Add(building);
                }
            }

            return results;
        }

        private List<TowerCrane> ReadTowers()
        {
            List<TowerCrane> results = new List<TowerCrane>();
            FileStream stream = new FileStream(@"D:\work\CraneTowerGroup\塔吊信息表格0924.xlsx", FileMode.Open);
            var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);
            for (int rowIndex = sheet.FirstRowNum + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                TowerCrane tower = new TowerCrane
                {
                    Id = rowIndex,
                    Code = row.GetCell(0).StringCellValue,
                    IndependentHeight = row.GetCell(1).NumericCellValue,
                    StartHeight = row.GetCell(2).NumericCellValue,
                    SectionHeight = row.GetCell(3).NumericCellValue,
                    StartTime = row.GetCell(4).DateCellValue,
                    EndTime = row.GetCell(5).DateCellValue,
                    JibLength = 0,
                    LiftSectionNumDic = new Dictionary<int, int>()
                };
                for (int i = 9; i < row.LastCellNum; i++)
                {
                    string str = row.GetCell(i).StringCellValue;
                    int numIndex = int.Parse(str.Split("<->").FirstOrDefault());
                    int height = (int)Math.Floor(double.Parse(str.Split("<->").LastOrDefault()) / tower.SectionHeight);
                    tower.LiftSectionNumDic.Add(numIndex, height);
                }
                results.Add(tower);
            }
            return results;
        }

        private List<CollisionRelation> ReadCollision(Dictionary<string, int> buildings, Dictionary<string, int> towers)
        {
            List<CollisionRelation> results = new List<CollisionRelation>();
            FileStream stream = new FileStream(@"D:\work\CraneTowerGroup\塔吊信息表格0924.xlsx", FileMode.Open);
            XSSFWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(0);
            for (int rowIndex = sheet.FirstRowNum + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                int towerId = towers[row.GetCell(0).StringCellValue];
                row.GetCell(7).StringCellValue.Split("、").ToList().ForEach(x =>
                {
                    if (buildings.TryGetValue(x, out int buildingId))
                    {
                        CollisionRelation collisionRelation = new CollisionRelation
                        {
                            CollisionId = buildingId,
                            RelationType = CollisionRelationType.塔吊与楼宇,
                            TowerId = towerId
                        };
                        results.Add(collisionRelation);
                    }
                    else
                    {
                        throw new ArgumentException("?");
                    }
                });
                row.GetCell(6).StringCellValue.Split("、").ToList().ForEach(x =>
                {
                    if (towers.TryGetValue(x, out int buildingId))
                    {
                        CollisionRelation collisionRelation = new CollisionRelation
                        {
                            CollisionId = buildingId,
                            RelationType = CollisionRelationType.塔吊与塔吊,
                            TowerId = towerId
                        };
                        results.Add(collisionRelation);
                    }
                    else
                    {
                        throw new ArgumentException("?");
                    }
                });
            }
            return results;
        }
    }
}
