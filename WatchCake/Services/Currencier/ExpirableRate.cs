using System;

namespace WatchCake.Services.Currencier
{
    /// <summary>
    /// Currency rate value with expiration time. Null expiration means no expiration.
    /// </summary>
    public struct ExpirableRate
    {
        /// <summary>
        /// The value of the currency rate.
        /// </summary>
        public decimal Rate;

        /// <summary>
        /// The expiration time of this rate.
        /// </summary>
        public DateTime Expires;

        /// <summary>
        /// Set new rate, default expiration is max date (effectively never).
        /// </summary>
        public ExpirableRate(decimal rate, DateTime? expires = null)
        {
            Rate = rate;
            Expires = expires ?? DateTime.MaxValue;
        }
    }
}
