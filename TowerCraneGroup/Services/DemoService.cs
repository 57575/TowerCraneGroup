using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Entities;
using TowerCraneGroup.InputModels;
using TowerCraneGroup.SolutionModels;

namespace TowerCraneGroup.Services
{
    public class DemoService
    {
        //初始化塔吊信息
        private readonly List<TowerCrane> Towers = new List<TowerCrane>
        {
            //TC7530-16H
            new TowerCrane
            {
                Id=1,
                SectionHeight=4.15,
                IndependentHeight=61.5,
                JibLength=78.28,
                LiftSectionNumDic=new Dictionary<int, int>
                {
                    {1,8},
                    {2,8},
                    {3,8},
                    {4,8},
                    {5,7},
                    {6,7}
                }
            },
            //TC7530-16H
            new TowerCrane
            {
                Id=2,
                SectionHeight=4.15,
                IndependentHeight=61.5,
                JibLength=78.28,
                LiftSectionNumDic=new Dictionary<int, int>
                {
                    {1,8},
                    {2,8},
                    {3,8},
                    {4,8},
                    {5,7},
                    {6,7}
                }
            },
            //XGT7532-20S
            new TowerCrane
            {
                Id=3,
                SectionHeight=3.3,
                IndependentHeight=62.0,
                JibLength=77.2,
                LiftSectionNumDic=new Dictionary<int, int>
                {
                    {1,12},
                    {2,10},
                    {3,10},
                    {4,10},
                    {5,9},
                    {6,9},
                    {7,9},
                    {8,9},
                    {9,5}
                }
            }
        };
        //初始化楼宇进度信息
        private readonly List<BuildingProcessing> BuildingProcess = new List<BuildingProcessing>();
        private void AddInitialBuildings(DateTime start, int Id, int floorNum)
        {
            BuildingProcessing buildingProcessing = new BuildingProcessing
            {
                Id = Id,
                Process = new Dictionary<DateTime, double>()
            };
            for (int i = 0; i < floorNum; i++)
            {
                buildingProcessing.Process.Add(start.AddDays(i * 7), (i + 1) * 4.2);
            }
            BuildingProcess.Add(buildingProcessing);
        }
        //初始化碰撞信息
        private readonly Dictionary<int, List<CollisionRelation>> Collisions = new Dictionary<int, List<CollisionRelation>>();
        private void InitialCollision()
        {
            List<CollisionRelation> collisionRelations = new List<CollisionRelation>
            {
                new CollisionRelation
                {
                    TowerId=1,CollisionId=1, RelationType= CollisionRelationType.塔吊与楼宇
                },
                new CollisionRelation
                {
                    TowerId=1,CollisionId=2,RelationType= CollisionRelationType.塔吊与楼宇
                },
                new CollisionRelation
                {
                    TowerId=1,CollisionId=2,RelationType= CollisionRelationType.塔吊与塔吊
                },
                new CollisionRelation
                {
                    TowerId=1,CollisionId=3,RelationType= CollisionRelationType.塔吊与塔吊
                },
                new CollisionRelation
                {
                    TowerId=2,CollisionId=2,RelationType = CollisionRelationType.塔吊与楼宇
                },
                new CollisionRelation
                {
                    TowerId=2,CollisionId=3,RelationType= CollisionRelationType.塔吊与塔吊
                },
                new CollisionRelation
                {
                    TowerId=3,CollisionId=3,RelationType= CollisionRelationType.塔吊与楼宇
                }
            };

            collisionRelations.GroupBy(x => x.TowerId).ToDictionary(x => x.Key, x => x.ToList()).ToList().ForEach(x =>
                {
                    Collisions.Add(x.Key, x.Value);
                });

        }

        public DemoService()
        {
            AddInitialBuildings(new DateTime(2021, 1, 1, 0, 0, 0), 1, 70);
            AddInitialBuildings(new DateTime(2021, 1, 14, 0, 0, 0), 2, 70);
            AddInitialBuildings(new DateTime(2021, 1, 25, 0, 0, 0), 3, 70);
            InitialCollision();
        }

        public void Run()
        {
            //
            Dictionary<int, int> towerChargeDic = CalculateHelper.CalculteTowerChargeBuilding(BuildingProcess, Collisions, Towers.ToDictionary(x => x.Id));


            Console.WriteLine("DONE");
        }




    }
}
