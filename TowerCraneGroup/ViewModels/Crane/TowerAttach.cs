using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.ViewModels.Crane
{
    /// <summary>
    /// 单个塔吊附墙方案
    /// </summary>
    public class TowerAttach
    {
        public TowerAttach()
        {
            AttachDetails = new List<TowerAttachDetail>();
        }
        public int TowerId { get; set; }
        public string TowerCode { get; set; }
        public int BuildingId { get; set; }
        public string BuildingCode { get; set; }
        public List<TowerAttachDetail> AttachDetails { get; set; }
    }
}
