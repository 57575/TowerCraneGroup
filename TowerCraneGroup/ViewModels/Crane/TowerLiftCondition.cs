using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerCraneGroup.ViewModels.Crane
{
    /// <summary>
    /// <para>塔吊在其主控楼宇某一层的结束日期的状态</para>
    /// <para>主控楼宇可能为虚拟楼宇</para>
    /// </summary>
    public class TowerLiftCondition
    {
        /// <summary>
        /// 当前状态时间
        /// </summary>
        public DateTime Time { get; internal set; }
        /// <summary>
        /// 塔吊Id
        /// </summary>
        public int TowerId { get; internal set; }
        /// <summary>
        /// 塔吊编号
        /// </summary>
        public string TowerCode { get; internal set; }
        /// <summary>
        /// 塔吊当前高度
        /// </summary>
        public double TowerHeight { get; internal set; }
        /// <summary>
        /// 当前时间塔吊提升的节数
        /// </summary>
        public int LiftSectionNumber { get; internal set; }
        /// <summary>
        /// 当前楼宇Id
        /// </summary>
        public int BuildingId { get; internal set; }
        /// <summary>
        /// 当前楼宇编号
        /// </summary>
        public string BuildingCode { get; internal set; }
        /// <summary>
        /// 当前楼宇结构高度
        /// </summary>
        public double BuildingHeight { get; internal set; }
    }
}
