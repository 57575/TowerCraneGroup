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
        /// <summary>
        /// chj:9,10,8,13,2,7,11,12,4,15,16,14,17,5,1,3,6
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task Configure([FromBody] List<int> order)
        {
            List<int> a = new List<int> { 9, 10, 8, 13, 2, 7, 11, 12, 4, 15, 16, 14, 17, 5, 1, 3, 6 };
            Dictionary<int, int> genOrder = new Dictionary<int, int>();
            for (int i = 0; i < a.Count; i++)
            {
                genOrder.Add(i, a[i]);
            }

            TrueServe trueServe = new TrueServe(21);
            trueServe.RunServe(
                @"G:\课题\群塔施工\漕河泾\施工进度信息表(2).xlsx",
                @"G:\课题\群塔施工\漕河泾\塔吊信息表格1018-原始.xlsx",
                @"G:\课题\群塔施工\漕河泾\塔吊附着信息.xlsx",
                genOrder);
        }
    }
}
