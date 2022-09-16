using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecng.Common;
using StockSharp.Algo.Indicators;

namespace StatA.Classes
{
    /// <summary>
    /// Диапазон спреда для арбитражной стратегии
    /// </summary>
    public class SpreadRange
    {
        public decimal Max { get; set; }
        public decimal Min { get; set; }
        public SpreadRange()
        {
            Max = 0;
            Min = 0;
        }

        public SpreadRange(decimal min, decimal max)
        {
            Min = min;
            Max = max;
        }

        public decimal GetProcentile(decimal proc)
        {
            if (proc > 1) throw new Exception("Аргумент должен быть в пределах от 0 до 1");
            if (Max < Min) throw new Exception("Max < Min");
            decimal channel;
            if (Max.Sign() != Min.Sign())
                channel = Max.Abs() + Min.Abs();
            else channel = Max.Abs() - Min.Abs();
            return Min + channel*proc;
        }
    }
}
