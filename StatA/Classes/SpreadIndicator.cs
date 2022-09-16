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

    [DisplayName("Spread")]
    [Description("Индикатор спреда (арбитраж)")]
    public class SpreadIndicator : LengthIndicator<decimal>
    {
        IList<Tuple<decimal, decimal>> historyBuffer { get; set; }
        REngine engine { get; set; }
        REnvironment enviroment { get; set; }
        //TODO: индикатор стационарности используется в этом классе?
        public bool AlwaysRecalc { get; set; } //Следует ли пересчитывать индикатор при поступлении новых данных, или только при завершении свечей.
        public decimal b { get; private set; }
        public SpreadIndicator()
        {
            /*Length: возможно, при бОльшем периоде индикатора значение b-коэффициента будет более значимым. Однако при этом спред появляется слишком поздно. 
            Стоит рассмотреть реализацию рассчета b-коэффициента на исторических данных при запуске индикатора*/
            Length = 25;
            historyBuffer = new List<Tuple<decimal, decimal>>();
            engine = REngine.GetInstance();
            enviroment = engine.CreateIsolatedEnvironment();
            AlwaysRecalc = false;
            b = 1;
        }


        public void ResetBuffer()
        {
            historyBuffer.Clear();
        }
        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            PairIndicatorValue<decimal> value = input as PairIndicatorValue<decimal>;
            var newValue = value.GetValue<Tuple<decimal, decimal>>();
            if (input.IsFinal || AlwaysRecalc)
            {
                historyBuffer.Add(value.GetValue<Tuple<decimal, decimal>>());
                if (historyBuffer.Count > Length)
                    historyBuffer.RemoveAt(0);

                var spread = CalcSpread(newValue);
                Buffer.Add(spread);
                if (Buffer.Count > Length)
                    Buffer.RemoveAt(0);
                if (historyBuffer.Count > 1)
                    return new DecimalIndicatorValue(this, spread); 
            }
            //if (previousIndicatorValue == null)
            //    previousIndicatorValue = new DecimalIndicatorValue(this, CalcSpread(newValue));
            return new DecimalIndicatorValue(this, 0);
        }

        decimal CalcSpread(Tuple<decimal,decimal> currentPrices)
        {
            //var indicatorValue = new DecimalIndicatorValue(this);
            //return indicatorValue;
            /*
            Спред можно вычислить по формуле S=Y1-bY2, 
            где Y1, Y2 – сравниваемые временные ряды(абсолютные значения), 
            b – коэффициент. 
            Значение коэффициента можно вычислить, построив линейную регрессию Y1 ~ Y2
            (см. текст диссертации, "Элементы статистического арбитража)
            */
            b = CalcLinearRegressionCoefficient();
            var Y1 = currentPrices.Item1;
            var Y2 = currentPrices.Item2;
            var S = Y1 - b*Y2;
            return S;
        }

        decimal CalcLinearRegressionCoefficient()
        {
            /*
            Расчет коэффициента линейной регрессии производится с применением среды статистического анализа R (среда должна быть установлена в системе!)
            В среду передается таблица данных с двумя столбцами - историей цен первой и второй корзины(или актива) соответственно.
            Столбцы представляют собой числовые вектора, т.е. не OHLC, а одна цена. 
            То, какие значения свечей учитывать в расчете, определяется свободными параметрами стратегии. Логичнее всего использовать цены закрытия. 
            Возможно, стоит рассмотреть вариант со средним значением по OHLC и медианой свечи на основе тиков(во втором случае могут возникнуть проблемы производительности).
            lm() - функция базового пакета R, предназначенная для построения линейных моделей, coef - коэффициенты полученной модели.
            rm(list=ls()); gc(); - очистка памяти.
            С целью минимизации издержек производительности при операциях со строками, данные операции сведены к минимуму в ущерб читаемости кода.
            */
            //if (!IsFormed) return 1;
            var secVector1 = historyBuffer.Select(item => Convert.ToDouble(item.Item1));
            var secVector2 = historyBuffer.Select(item => Convert.ToDouble(item.Item2));
            var secDataFrame = new IEnumerable[] {secVector1, secVector2};
            var names = new string[] {"sec1", "sec2"};
            DataFrame data = engine.CreateDataFrame(secDataFrame, names);
            engine.SetSymbol("data",data);
            var coeff = engine.Evaluate("coef(lm(sec1~sec2+0, data))[1]").AsNumeric().First();
            engine.Evaluate("rm(list=ls()); gc();");
            return Convert.ToDecimal(coeff);
        }


    }
}
