using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using RDotNet;
using StockSharp.Algo.Indicators;
using StockSharp.BusinessEntities;
using StockSharp.Localization;

namespace StatA.Classes
{
    [DisplayName("Arbitrage")]
    [Description("Индикатор арбитражного спреда и его стационарности")]
    class ArbitrageIndicator : BaseComplexIndicator
    {
        public SpreadIndicator SpreadIndicator {get; set; }
        public Stationarity StationarityIndicator { get; set; }
        public ArbitrageIndicator() : this(new SpreadIndicator {Length = 16}, new Stationarity {Length = 16})
        {

        }

        public ArbitrageIndicator(SpreadIndicator spreadIndicator, Stationarity stationarityIndicator)
        {
            InnerIndicators.Add(SpreadIndicator = spreadIndicator);
            InnerIndicators.Add(StationarityIndicator = stationarityIndicator);
            Mode = ComplexIndicatorModes.Sequence;
        }
        /// <summary>
        /// Длина периода.
        /// </summary>
        [DisplayName("Период")]
        [Description("Период индикатора.")]
        [Category("Основные")]
        public virtual int Length
        {
            get { return SpreadIndicator.Length; }
            set
            {
                SpreadIndicator.Length = StationarityIndicator.Length = value;
                Reset();
            }
        }

    }
}
