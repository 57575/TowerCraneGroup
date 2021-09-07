using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.InputModels
{
    public class BuildingProcessing
    {
        /// <summary>
        /// 楼宇编号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 楼宇的进度计划，即达到X米的时间T
        /// </summary>
        public Dictionary<DateTime, double> Process { get; set; }

        public Dictionary<DateTime, double> OrderProcess()
        {
            return Process.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        public double GetFinalStructureHeighth()
        {
            return Process.Values.Max();
        }
    }
}
