using System;
using System.Linq;
using WatchCake.Models;
using WatchCake.Services.Currencier;

namespace WatchCake.Helpers
{
    /// <summary>
    /// Helpers providing various option-price-money calculation methods.
    /// </summary>
    public static class PriceMaths
    {
        /// <summary>
        /// Calculate weighted average price having option price history.
        /// </summary>
        public static Money CalculateWeightedMeanPrice(this Option option, Currency? outputCurrency = null)
        {  
            var snaps = option.Snapshots;

            if (snaps == null || snaps.Count() < 1)
                return null;

            Currency currency = outputCurrency ?? Currencier.MainCurrency;

            if (snaps.Count() == 1)
                return snaps.First().Price.As(currency);

            var snapArray = snaps.OrderBy(ss => ss.Timestamp).ToArray();
            
            decimal weightedSum = 0;
            Snapshot prevSnap = null, thisSnap = null, nextSnap = null;
            for (int i = 0; i < snapArray.Length; i++)
            {
                bool isFirst = i == 0;
                bool isLast = i == snapArray.Length - 1;

                prevSnap = isFirst ? null : snapArray[i - 1];
                thisSnap = snapArray[i];
                nextSnap = isLast ? null : snapArray[i + 1];

                long preTicks = isFirst ? 0 : ( (thisSnap.Timestamp.Ticks - prevSnap.Timestamp.Ticks) / 2 );
                long postTicks = isLast ? 0 : ( (nextSnap.Timestamp.Ticks - thisSnap.Timestamp.Ticks) / 2 );

                long weightTicks = preTicks + postTicks;

                Money price = thisSnap.Price.As(currency);

                weightedSum += weightTicks * price.Amount;
            }

            long totalTicks = snapArray[snapArray.Length-1].Timestamp.Ticks - snapArray[0].Timestamp.Ticks;

            return new Money(weightedSum / totalTicks, currency);
        }

        /// <summary>
        /// Compare prices and return signed deviation.
        /// </summary>
        public static decimal? CalculatePriceShift(Money general, Money concrete)
        {
            decimal? result = null;

            if (general != null && concrete != null)
            {
                decimal todayAmount = concrete.Amount;
                decimal avergeAmount = general.As(concrete.Currency).Amount;

                if (todayAmount != 0 && avergeAmount != 0)
                {
                    decimal tpa = todayAmount / avergeAmount;
                    decimal shift = tpa < 1 ? (-1 + tpa) : (tpa - 1);
                    result = Math.Round(shift, 2);
                }
            }

            return result;
        }
    }
}
