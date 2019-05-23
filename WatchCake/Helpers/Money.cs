using System;
using System.Collections.Generic;
using System.Linq;
using WatchCake.Services.Currencier;

namespace WatchCake.Helpers
{
    /// <summary>
    /// Represents the amount of money of the specific currency.
    /// </summary>
    public class Money : NotifyingObject, IComparable
    {
        private decimal _amount;

        /// <summary>
        /// Amount of money in a corresponding Currency.
        /// </summary>
        public decimal Amount { get => _amount; set { _amount = value; RaisePropertyChanged(); } }

        /// <summary>
        /// Currency of the specified Amount of money.
        /// </summary>
        public Currency Currency { get; set; }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Money(Money original)
        {
            Amount = original.Amount;
            Currency = original.Currency;
        }

        /// <summary>
        /// Parameterless constructor for automation.
        /// </summary>
        public Money()
        {
        }

        /// <summary>
        /// Manual constructing.
        /// </summary>
        public Money(decimal amount, Currency currency)
        {
            Amount = amount;
            Currency = currency;
        }

        /// <summary>
        /// Get another Money object represented in Main currency, retaining the actual value.
        /// </summary>
        public Money AsMainCurrency() => As(Currencier.MainCurrency);

        /// <summary>
        /// Get another Money object with a different currency, retaining the actual value.
        /// </summary>
        public Money As(Currency newCurrency)
        {
            if (this.Currency == newCurrency)
                return this;
            else
                return new Money(
                    amount: this.Amount * (decimal)Currencier.GetRateFor(newCurrency, this.Currency),
                    currency: newCurrency
                    );

        }

        /// <summary>
        /// List of currency symbols.
        /// </summary>
        readonly Dictionary<Currency, string> CurrencyFormatting = new Dictionary<Currency, string>()
        {
            { Currency.USD, "${0:F}" },
            { Currency.EUR, "{0:F} €" },
            { Currency.UAH, "{0:F} грн" },
        };

        /// <summary>
        /// Outputs amount with the currency identifier. E.g.: '345 USD'.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => string.Format(CurrencyFormatting[Currency], Amount);

        /// <summary>
        /// Compares this Money to that Money.
        /// </summary>
        public int CompareTo(object otherObj)
        {
            var other = otherObj as Money;

            if (this.Currency != other.Currency)
                other = other.As(this.Currency);

            return this.Amount.CompareTo(other.Amount);
        }

        /// <summary>
        /// Calculate average money value within the provided set of money instances.
        /// </summary>
        public static Money GetAverage(IEnumerable<Money> list, Currency? outputCurrency = null)
        {
            if (list == null || list.Count() < 1)
                return null;

            Currency currency = outputCurrency ?? Currencier.MainCurrency;

            decimal sum = 0;
            int count = 0;
            foreach (Money entry in list)
            {
                if (entry == null)
                    continue;

                sum += entry.As(currency).Amount;
                count++;
            }

            return new Money(sum / count, currency);
        }

    }
}
