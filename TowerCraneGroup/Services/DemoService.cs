using Newtonsoft.Json;
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
                StartTime=new DateTime(2020,12,15),
                EndTime=new DateTime(2022,5,1),
                StartHeight=49.8,
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
                StartTime=new DateTime(2021,1,2),
                EndTime=new DateTime(2022,5,15),
                StartHeight=40.8,
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
                StartHeight=31.8,
                StartTime=new DateTime(2021,1,10),
                EndTime=new DateTime(2022,5,26),
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
        private void InitialTowers()
        {
            Towers.ForEach(x =>
            {
                int? buildingId = TowerChargeBuildings.Where(y => y.TowerId == x.Id).FirstOrDefault()?.BuildingId;
                if (buildingId != null)
                {
                    int floorNum = BuildingProcess.Where(x => x.Id == buildingId.Value).First().Process.Keys.Count;
                    if (floorNum > x.LiftSectionNumDic.Count)
                    {
                        int lastLiftSectionNum = x.LiftSectionNumDic.LastOrDefault().Value;
                        int lastListIndex = x.LiftSectionNumDic.LastOrDefault().Key;
                        for (int i = lastListIndex + 1; i <= floorNum; i++)
                            x.LiftSectionNumDic.Add(i, lastLiftSectionNum);
                    }
                }
            });
        }
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
                    TowerId=2,CollisionId=1,RelationType= CollisionRelationType.塔吊与塔吊
                },
                new CollisionRelation
                {
                    TowerId=2,CollisionId=3,RelationType= CollisionRelationType.塔吊与塔吊
                },
                new CollisionRelation
                {
                    TowerId=3,CollisionId=3,RelationType= CollisionRelationType.塔吊与楼宇
                },
                new CollisionRelation
                {
                    TowerId=3,CollisionId=1,RelationType= CollisionRelationType.塔吊与塔吊
                },
                new CollisionRelation
                {
                    TowerId=3,CollisionId=2,RelationType= CollisionRelationType.塔吊与塔吊
                }
            };

            collisionRelations.GroupBy(x => x.TowerId).ToDictionary(x => x.Key, x => x.ToList()).ToList().ForEach(x =>
                {
                    Collisions.Add(x.Key, x.Value);
                });

        }
        private List<TowerChargeBuilding> TowerChargeBuildings { get; set; }

        private int GenerationCount { get; set; }
        private Population Population { get; set; }
        private int PopSize { get; set; }
        private List<int> BestTenInHistory = new List<int>();

        private Dictionary<int, TowerCrane> TowersDic => Towers.ToDictionary(x => x.Id);
        private Dictionary<int, BuildingProcessing> BuildingsDic => BuildingProcess.ToDictionary(x => x.Id);
        private Dictionary<int, TowerChargeHelper> TowerChargeDic { get; set; }

        public DemoService(int popSize)
        {
            AddInitialBuildings(new DateTime(2021, 1, 1, 0, 0, 0), 1, 30);
            AddInitialBuildings(new DateTime(2021, 1, 14, 0, 0, 0), 2, 30);
            AddInitialBuildings(new DateTime(2021, 1, 25, 0, 0, 0), 3, 30);
            InitialCollision();
            this.PopSize = popSize;
            TowerChargeBuildings = CalculateHelper.CalculteTowerChargeBuilding(BuildingProcess, Collisions, Towers.ToDictionary(x => x.Id));
            InitialTowers();
            Population = new Population(PopSize, Towers.ToDictionary(x => x.Id), TowerChargeBuildings, BuildingProcess.ToDictionary(x => x.Id), Collisions, false);
            GenerationCount = 0;
            int geneIndex = 0;
            TowerChargeDic = new Dictionary<int, TowerChargeHelper>();
            foreach (var tower in TowerChargeBuildings)
            {
                int max = TowersDic[tower.TowerId].LiftSectionNumDic.Values.Max();
                int length = BuildingsDic[tower.BuildingId].Process.Count;
                TowerChargeHelper aHelper = new TowerChargeHelper(geneIndex, tower, length, TowersDic[tower.TowerId].SectionHeight, TowersDic[tower.TowerId].StartHeight);
                TowerChargeDic.Add(aHelper.GeneIndex, aHelper);
                geneIndex++;
            }
        }

        public void Run()
        {
            Individual solution = null;
            while (
                GenerationCount < 10000
                || BestTenInHistory.Distinct().Count() != 1
                || solution.CalculateForbidden(Collisions, BuildingsDic, TowersDic, TowerChargeDic)
                )
            {
                Population offspring = new Population(PopSize, Towers.ToDictionary(x => x.Id), TowerChargeBuildings, BuildingProcess.ToDictionary(x => x.Id), Collisions, true);
                Population.CalculateFitness(Collisions, BuildingsDic, TowersDic, TowerChargeDic);
                Individual best = new Individual();
                string bestJson = Population.GetFittest().Serialize();
                best = JsonConvert.DeserializeObject<Individual>(bestJson);
                solution = best;
                if (BestTenInHistory.Count < 10000)
                {
                    BestTenInHistory.Add(Population.GetFittest().Fitness);
                }
                else
                {
                    BestTenInHistory[BestTenInHistory.IndexOf(BestTenInHistory.Max())] = best.Fitness;
                }
                offspring.AddIndividual(best);
                Random random = new Random();
                for (int i = 1; i < PopSize || offspring.GetPopulationSize() < PopSize; i += 2)
                {
                    Individual son1, son2;
                    (son1, son2) = Population.Crossover();
                    if (random.Next(0, PopSize) < Math.Ceiling((PopSize / 1.0)))
                    {
                        son1.Mutation(18, BuildingsDic, TowerChargeDic);
                    }
                    if (random.Next(0, PopSize) < Math.Ceiling((PopSize / 1.0)))
                    {
                        son2.Mutation(18, BuildingsDic, TowerChargeDic);
                    }
                    Individual newSon1 = new Individual();
                    newSon1 = JsonConvert.DeserializeObject<Individual>(son1.Serialize());
                    Individual newSon2 = new Individual();
                    newSon2 = JsonConvert.DeserializeObject<Individual>(son2.Serialize());
                    if (newSon1.CalculateBaseConstraint(BuildingsDic, TowerChargeDic))
                    {
                        offspring.AddIndividual(newSon1);
                    }
                    if (newSon2.CalculateBaseConstraint(BuildingsDic, TowerChargeDic))
                    {
                        offspring.AddIndividual(newSon2);
                    }
                }
                //offspring.CalculateFitness();
                Population = offspring;
                GenerationCount++;
                Console.WriteLine("Generation:" + GenerationCount + "||BestFitness:" + best.Fitness);
            }
            string solutionStr = JsonConvert.SerializeObject(solution);
            Console.WriteLine("Solution:" + solutionStr);
            Console.WriteLine("DONE");
        }

        public void DebugServe()
        {
            Individual individual = new Individual();
            individual.Genes = new List<List<int>>()
            {
                new List<int>
                {
                    //0,0,0,0,0,8,8,0,0,0,0,0,0,0,0,0,8,0,0,0,0,0,1,0,0,0,0,0,0,0
                    0,7,0,0,0,0,0,0,0,0,0,7,0,0,1,4,4,1,1,0,0,0,0,0,0,0,0,0,0,0
                    //0,0,0,0,0,0,0,0,8,0,0,0,0,0,0,0,8,0,0,0,0,0,0,0,5,0,0,0,0,0
                },
                new List<int>
                {
                    //0,0,0,0,8,8,0,0,0,0,0,0,0,0,0,8,0,0,0,0,0,1,0,0,0,0,0,0,0,0
                    0,0,2,0,5,0,0,5,0,0,0,1,1,0,0,0,0,0,7,0,4,0,0,0,0,0,0,0,0,0
                    //0,0,0,0,0,0,8,0,0,0,0,0,0,0,8,0,0,0,0,0,0,0,5,0,0,0,0,5,0,0
                },
                new List<int>
                {
                    //0,0,0,0,12,0,0,0,0,0,0,0,0,0,10,0,0,0,0,0,9,0,0,0,0,0,0,0,0,0
                    0,1,0,1,1,0,2,0,2,0,2,7,1,0,0,0,0,1,5,0,5,0,0,0,2,0,0,0,1,0
                    //0,0,0,0,10,0,0,0,0,0,0,0,10,0,0,0,0,0,0,10,5,0,0,0,0,3,0,0,0,0
                }
            };
            Population offspring = new Population(1, Towers.ToDictionary(x => x.Id), TowerChargeBuildings, BuildingProcess.ToDictionary(x => x.Id), Collisions, true);

            offspring.AddIndividual(individual);

            offspring.CalculateFitness(Collisions, BuildingsDic, TowersDic, TowerChargeDic);

            Console.WriteLine(individual.Serialize());

            Console.ReadLine();
        }

        public void GreedyServe()
        {
            GreedyAlgorithmService greedyAlgorithmService = new GreedyAlgorithmService(TowersDic, TowerChargeBuildings, BuildingsDic, Collisions);

            greedyAlgorithmService.RunService();
        }
    }
}
