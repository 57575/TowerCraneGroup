using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.Entities
{
    public class TowerCrane
    {
        public TowerCrane()
        {
            LiftSectionNumDic = new Dictionary<int, int>();
        }

        public int Id { get; set; }
        /// <summary>
        /// 塔吊型号
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 最大独立高度
        /// </summary>
        public double IndependentHeight { get; set; }
        /// <summary>
        /// 塔吊的初始高度
        /// </summary>
        public double StartHeight { get; set; }
        /// <summary>
        /// 塔吊安装时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 塔吊拆除时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 每节高度
        /// </summary>
        public double SectionHeight { get; set; }
        /// <summary>
        /// 塔吊每次附墙之后的最大提升节数
        /// </summary>
        public Dictionary<int, int> LiftSectionNumDic { get; set; }
        /// <summary>
        /// 塔吊臂架长度
        /// </summary>
        public double JibLength { get; set; }
    }
}
