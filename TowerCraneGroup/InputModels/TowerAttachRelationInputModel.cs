﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.InputModels
{
    /// <summary>
    /// 塔吊和它附墙的楼宇的关系
    /// </summary>
    public class TowerAttachRelationInputModel
    {
        public int TowerId { get; set; }
        public int BuildingId { get; set; }
    }
}
