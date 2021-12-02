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
        /// 塔吊编号
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
        /// 塔吊每一道附墙与前一道附墙的间隔，对第一道附墙为和±0的距离
        /// </summary>
        public List<AttachSegment> AttachSegments { get; set; }
        /// <summary>
        /// 塔吊臂架长度
        /// </summary>
        public double JibLength { get; set; }
    }

    public class AttachSegment
    {
        public int Index { get; set; }
        public double UpperBound { get; set; }
        public double LowerBound { get; set; }
        public override string ToString()
        {
            return "[" + UpperBound + "," + LowerBound + "]";
        }
    }
}
