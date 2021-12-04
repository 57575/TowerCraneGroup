using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.InputModels.Process
{
    public class BuildingProcessing
    {
        /// <summary>
        /// 楼宇编号
        /// </summary>
        public int Id { get; set; }

        public string BuildingCode { get; set; }

        public List<FloorProcessing> FloorProcesses { get; set; }
        /// <summary>
        /// 楼宇的进度计划，即达到X米的时间T
        /// </summary>
        internal Dictionary<DateTime, double> Process { get; set; }

        internal BuildingProcessing()
        {
            Process = new Dictionary<DateTime, double>();
        }

        public BuildingProcessing(int id, string code, List<FloorProcessing> floors)
        {
            this.Id = id;
            this.BuildingCode = code;
            this.FloorProcesses = floors;
            this.Process = new Dictionary<DateTime, double>();
            FloorProcesses.OrderBy(x => x.Elevation).ToList().ForEach(x =>
            {
                this.Process.Add(x.EndTime, x.Elevation);
            });
        }

        internal double GetHeightByFloorIndex(int floorIndex)
        {
            if (floorIndex >= this.Process.Values.Count)
                return this.Process.Values.ToList()[floorIndex - 1];
            else
                return Process.Values.ToList()[floorIndex];
        }

        internal DateTime GetDateTimeByFloorIndex(int floorIndex)
        {
            return Process.Keys.ToList()[floorIndex];
        }

        internal double GetFinalStructureHeighth()
        {
            return Process.Values.Max();
        }
    }
}
