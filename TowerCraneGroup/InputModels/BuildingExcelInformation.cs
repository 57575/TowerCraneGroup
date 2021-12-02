using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.InputModels
{
    public class BuildingExcelInformation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public DateTime DateTime { get; set; }
    }
}
