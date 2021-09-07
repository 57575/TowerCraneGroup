using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.SolutionModels
{
    public class CollisionRelation
    {
        //塔吊Id
        public int TowerId { get; set; }
        //与该塔吊碰撞的物体Id
        public int CollisionId { get; set; }
        //碰撞类型
        public CollisionRelationType RelationType { get; set; }
    }
    public enum CollisionRelationType
    {
        塔吊与楼宇 = 1,
        塔吊与塔吊 = 2
    }
}
