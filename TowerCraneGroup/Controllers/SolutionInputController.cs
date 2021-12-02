using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Services;

namespace TowerCraneGroup.Controllers
{
    [Route("api/tower/solution")]
    public class SolutionInputController : ControllerBase
    {
        public SolutionInputController()
        {
        }

        [HttpPost]
        public async Task Configure([FromBody] List<int> order)
        {
            Dictionary<int, int> genOrder = new Dictionary<int, int>();
            for (int i = 0; i < order.Count; i++)
            {
                genOrder.Add(i, order[i]);
            }

            TrueServe trueServe = new TrueServe(21);
            trueServe.RunServe(
                @"G:\成果\群塔施工\漕河泾\施工进度信息表(2).xlsx",
                @"G:\成果\群塔施工\漕河泾\塔吊信息表格1018-原始.xlsx",
                @"G:\成果\群塔施工\漕河泾\塔吊附着信息.xlsx",
                genOrder);
        }
    }
}
