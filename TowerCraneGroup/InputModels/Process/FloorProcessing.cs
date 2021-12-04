using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.InputModels.Process
{
    public class FloorProcessing
    {
        public FloorProcessing(int id, string code, double elevation, DateTime start, DateTime end)
        {
            Id = id;
            FloorCode = code;
            Elevation = elevation;
            StartTime = start;
            EndTime = end;
        }
        internal int Id { get; private set; }

        internal string FloorCode { get; private set; }

        internal double Elevation { get; private set; }

        internal DateTime StartTime { get; private set; }

        internal DateTime EndTime { get; private set; }
    }
}
