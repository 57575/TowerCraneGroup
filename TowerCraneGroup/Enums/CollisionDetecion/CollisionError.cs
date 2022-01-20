using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.Enums.CollisionDetecion
{
    public enum CollisionError
    {
        塔吊安全距离不足 = 1,
        楼宇安全距离不足 = 2,
        悬出段超限 = 3,
    }
}
