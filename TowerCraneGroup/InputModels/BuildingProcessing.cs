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

        public double GetHeightByFloorIndex(int floorIndex)
        {
            if (floorIndex >= this.Process.Values.Count)
                return this.Process.Values.ToList()[floorIndex - 1];
            else
                return Process.Values.ToList()[floorIndex];
        }

        public DateTime GetDateTimeByFloorIndex(int floorIndex)
        {
            return Process.Keys.ToList()[floorIndex];
        }

        public double GetFinalStructureHeighth()
        {
            return Process.Values.Max();
        }
    }
}
