using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Enums.CollisionDetecion;

namespace TowerCraneGroup.ViewModels.CollisionDetection
{
    public class CollisionDetectionErrorMessage
    {
        public int CraneId { get; set; }
        public string CraneCode { get; set; }
        public DateTime DateTime { get; set; }
        public CollisionError ErrorType { get; set; }
        public int? CollisionId { get; set; }
        public string CollisionCode { get; set; }
    }
}
