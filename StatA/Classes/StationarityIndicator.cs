using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using DevExpress.Mvvm.Native;
using DevExpress.Xpf.Editors.Helpers;
using RDotNet;
using StockSharp.Algo.Indicators;
using StockSharp.BusinessEntities;
using StockSharp.Localization;

namespace StatA.Classes
{
    
    [DisplayName("Stationarity")]
    [Description("Индикатор стационарности временного ряда")]
    public class Stationarity : LengthIndicator<decimal>
    {
        REngine engine { get; set; }
        public Stationarity()
        {
            Length = 25;
            engine = REngine.GetInstance();
            //if (!engine.IsRunning)
            //    engine.Initialize();
            engine.Evaluate("library(tseries)"); //необходимо для aft.test()
        }
        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var newValue = input.GetValue<decimal>();

            if (input.IsFinal)
            {
                Buffer.Add(newValue);

                if (Buffer.Count > Length)
                    Buffer.RemoveAt(0);
            }

            if (input.IsFinal)
                return new DecimalIndicatorValue(this, ADFTest(Buffer));

            return new DecimalIndicatorValue(this, ADFTest(Buffer.Skip(1)));
        }

        private decimal ADFTest(IEnumerable<decimal> timeSeries)
        {
            if (Buffer.Count < Length) return 1M;
            var doubleList = timeSeries.Select(item => Convert.ToDouble(item));
            NumericVector numericVector = engine.CreateNumericVector(doubleList); 
            engine.SetSymbol("buffer", numericVector);
            GenericVector testResult = engine.Evaluate("adf.test(buffer, k=0)").AsList();
            var pVal = testResult["p.value"].AsNumeric().First();
            engine.Evaluate("rm(list=ls()); gc();");
            return Convert.ToDecimal(pVal);
        }
        public void Dispose(bool disposing)
        {
                engine.Dispose();
        }

    }
}
