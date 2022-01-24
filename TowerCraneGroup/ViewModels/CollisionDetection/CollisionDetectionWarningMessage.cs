using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Enums.CollisionDetecion;

namespace TowerCraneGroup.ViewModels.CollisionDetection
{
    public class CollisionDetectionWarningMessage
    {
        public int CraneId { get; set; }

        public string CraneCode { get; set; }

        public CollisionWarningType CollisionType { get; set; }

        public string Warning { get; set; }
    }
}
