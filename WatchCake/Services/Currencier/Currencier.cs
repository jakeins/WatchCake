using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WatchCake.Parsers;
using WatchCake.Parsers.Enums;
using WatchCake.Services.Molder;
using WatchCake.Services.FastWebNS;

namespace WatchCake.Services.Currencier
{
    /// <summary>
    /// Infrastructure-level service dealing with currency.
    /// </summary>
    public static class Currencier
    {        
        /// <summary>
        /// Main currency. Defined by the app domain.
        /// </summary>
        public static Currency MainCurrency { get; set; } = Currency.USD;

        /// <summary>
        /// The map keeping all rates relative to the main currency
        /// </summary>
        static Dictionary<Currency, decimal> Rates;

        /// <summary>
        /// Call for initialization of the service, allowing for lazy loading.
        /// </summary>
        static void Initalize()
        {
            #region Google currency parser setup
            FastWeb2 fastWeb = new FastWeb2("https://www.google.com/");

            var valueExtractor = new BitParser()
            {
                SelectMethod = SelectMethod.XPath,
                DetailType = SelectDetailType.Property,
                SelectQuery = "//*[contains(text(),' United States Dollar')]",
                Detail = "InnerText",
                PostProcess = new[]
                {
                    new Mold(MoldType.Before, " United States Dollar"),
                    new Mold(MoldType.Substr,(-5).ToString()),
                    new Mold(MoldType.OnlyFloatChars),
                }
            };
            #endregion Google currency parser setup

            //initialize rates map
            Rates = new Dictionary<Currency, decimal>()
            {
                { MainCurrency, 1 }
            };

            //fill main currecnies
            var secondaryCurrencies = Enum.GetValues(typeof(Currency)).Cast<Currency>().Where(kc => kc != MainCurrency);
            foreach (Currency currency in secondaryCurrencies)
            {
                var rawHtml = fastWeb.Get($"search?hl=en&gl=en&q={currency}+to+{MainCurrency}");

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(rawHtml);
                HtmlNode docNode = document.DocumentNode;

                string exValue = valueExtractor.ExtractSingle(docNode);
                decimal.TryParse(exValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed);

                Rates.Add(currency, parsed);                
            }
        }

        /// <summary>
        /// Get rate between provided currencies.
        /// Rate = How many [alpha]s in [beta]s.
        /// </summary>
        public static decimal GetRateFor(Currency alpha, Currency beta)
        {
            if (Rates == null)
                Initalize();

            return Rates[beta] / Rates[alpha];
        }
    }
}
