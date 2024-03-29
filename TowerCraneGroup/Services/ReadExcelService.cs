﻿using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Enums.Crane;
using TowerCraneGroup.InputModels;
using TowerCraneGroup.InputModels.Crane;
using TowerCraneGroup.InputModels.Process;
using TowerCraneGroup.SolutionModels;

namespace TowerCraneGroup.Services
{
    internal static class ReadExcelService
    {
        static public void InitialTowers(List<TowerCrane> towers, List<BuildingProcessing> buildings, List<TowerChargeBuilding> TowerChargeBuildings)
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
        static public List<BuildingProcessing> ReadBuilding(string path)
        {
            List<BuildingProcessing> results = new List<BuildingProcessing>();

            FileStream stream = new FileStream(path, FileMode.Open);

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
        static public List<TowerCrane> ReadTowers(string path, string attachPath)
        {
            List<TowerCrane> results = new List<TowerCrane>();
            FileStream stream = new FileStream(path, FileMode.Open);
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
                    LiftSectionNumDic = new Dictionary<int, int>()
                };
                for (int i = 10; i < row.LastCellNum; i++)
                {
                    string str = row.GetCell(i).StringCellValue;
                    int numIndex = int.Parse(str.Split("<->").FirstOrDefault());
                    int height = (int)Math.Floor(double.Parse(str.Split("<->").LastOrDefault()) / tower.SectionHeight);
                    tower.LiftSectionNumDic.Add(numIndex, height);
                }
                results.Add(tower);
            }
            //ReadTowerAttach(attachPath, results);
            return results;
        }
        static public List<CollisionRelation> ReadCollision(string path, Dictionary<string, int> buildings, Dictionary<string, int> towers)
        {
            List<CollisionRelation> results = new List<CollisionRelation>();
            FileStream stream = new FileStream(path, FileMode.Open);
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
        static public void ReadTowerAttach(string path, List<TowerCrane> towers)
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            XSSFWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(0);
            for (int rowIndex = sheet.FirstRowNum + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                string towerCode = row.GetCell(0).StringCellValue;
                TowerCrane thisTower = towers.FirstOrDefault(x => x.Code == towerCode);
                thisTower.AttachSegments = new List<CraneAttachSegment>();
                for (int cellNum = 1; cellNum < row.LastCellNum; cellNum++)
                {
                    string str = row.GetCell(cellNum).StringCellValue;
                    double lowerBound = double.Parse(str.Split("、").First());
                    double upperBound = double.Parse(str.Split("、").Last());
                    thisTower.AttachSegments.Add(new CraneAttachSegment
                    {
                        Index = cellNum,
                        LowerBound = lowerBound,
                        UpperBound = upperBound,
                        CantileverSectionNum = thisTower.LiftSectionNumDic[cellNum]
                    });
                }
                if (thisTower.LiftSectionNumDic.Count > row.LastCellNum)
                {
                    for (int i = row.LastCellNum; i <= thisTower.LiftSectionNumDic.Count; i++)
                    {
                        thisTower.AttachSegments.Add(new CraneAttachSegment
                        {
                            Index = i,
                            CantileverSectionNum = thisTower.LiftSectionNumDic[i]
                        });
                    };
                }
            }
        }
        static public List<TowerAttachRelation> ReadAttachesInput(string path, List<TowerCrane> towers, List<BuildingProcessing> buildings)
        {
            List<TowerAttachRelation> results = new List<TowerAttachRelation>();
            var towerDic = towers.ToDictionary(x => x.Code, x => x.Id);
            var buildingDic = buildings.ToDictionary(x => x.BuildingCode, x => x.Id);
            FileStream stream = new FileStream(path, FileMode.Open);
            XSSFWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(0);
            for (int rowIndex = sheet.FirstRowNum + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                string towerCode = row.GetCell(0).StringCellValue;
                string buildingCode = row.GetCell(8).StringCellValue;
                TowerAttachRelation result = new TowerAttachRelation()
                {
                    BuildingId = buildingDic[buildingCode],
                    TowerId = towerDic[towerCode]
                };
                results.Add(result);
            }
            return results;
        }
    }
}
