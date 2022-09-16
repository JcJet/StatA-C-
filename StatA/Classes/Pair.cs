using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSharp.Algo;
using StockSharp.BusinessEntities;

namespace StatA.Classes
{
    public class Pair
    {
        public string Name { get; set; }
        public WeightedIndexSecurity FirstBasket { get; set; }
        public WeightedIndexSecurity SecondBasket { get; set; }

        public Pair()
        {
            Name = "";
            FirstBasket = new WeightedIndexSecurity();
            SecondBasket = new WeightedIndexSecurity();
        }
        public Pair(WeightedIndexSecurity firstBasket, WeightedIndexSecurity secondBasket)
        {
            Name = "";
            FirstBasket = firstBasket;
            SecondBasket = secondBasket;
        }
        //public class Basket
        //{
        //    public string Name { get; set; }
        //    public List<BasketEntry> Securities { get; set; }

        //    public Basket()
        //    {
        //        Name = "";
        //        Securities = new List<BasketEntry>();
        //    }

        //    public Basket(List<BasketEntry> entries)
        //    {
        //        Name = "";
        //        Securities = entries;
        //    }

        //    public Basket(Basket BasketObj)
        //    {
        //        Name = BasketObj.Name;
        //        Securities = new List<BasketEntry>();
        //        foreach (var sec in BasketObj.Securities)
        //        {
        //            Securities.Add(new BasketEntry(sec));
        //        }
        //    }
        //    public void AddSecurity(Security sec, double weight = 1)
        //    {
        //        Securities.Add(new BasketEntry(sec, weight));
        //    }

        //    public bool FindSecurity(Security sec)
        //    {
        //        foreach (var entry in Securities)
        //        {
        //            if ((sec.Code == entry.Security.Code) && (sec.Board == entry.Security.Board))
        //                return true;
        //        }
        //        return false;
        //    }

        //    public void RemoveSecurity(Security sec)
        //    {
        //        BasketEntry foundEntry = new BasketEntry();
        //        foreach (var entry in Securities)
        //        {
        //            if ((sec.Code == entry.Security.Code) && (sec.Board == entry.Security.Board))
        //            {
        //                foundEntry = entry;
        //                break;
        //            }
        //        }
        //        Securities.Remove(foundEntry);

        //    }

        //    public double GetWeight(Security sec)
        //    {
        //        foreach (var entry in Securities)
        //        {
        //            if ((sec.Code == entry.Security.Code) && (sec.Board == entry.Security.Board))
        //                return entry.Weight;
        //        }
        //        return 0;
        //    }

        //    public void UpdateWeight(Security sec, double weight)
        //    {
        //        foreach (var entry in Securities)
        //        {
        //            if ((sec.Code == entry.Security.Code) && (sec.Board == entry.Security.Board))
        //            {
        //                entry.Weight = weight;
        //                break;
        //            }
        //        }
        //    }
        //    public class BasketEntry
        //    { 
        //        public Security Security { get; set; }
        //        public double Weight { get; set; }

        //        public BasketEntry()
        //        {
        //            Security = new Security();
        //            Weight = 1;
        //        }

        //        public BasketEntry(Security security, double weight = 1)
        //        {
        //            Security = security;
        //            Weight = weight;
        //        }

        //        public BasketEntry(BasketEntry BasketEntryObj)
        //        {
        //            Security = BasketEntryObj.Security;
        //            Weight = BasketEntryObj.Weight;
        //        }
        //    }
        //}
    }
}
