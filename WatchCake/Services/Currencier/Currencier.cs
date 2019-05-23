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
        /// The map keeping all intercurrency rates.
        /// </summary>
        static Dictionary<Currency, Dictionary<Currency, double>> currenciesMap;

        /// <summary>
        /// Call for initialization of the service, allowing for lazy loading.
        /// </summary>
        static void Initalize()
        {
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

            var mainCurrency = MainCurrency;
            var allCurrencies = Enum.GetValues(typeof(Currency)).Cast<Currency>();
            var secondaryCurrencies = allCurrencies.Where(kc => kc != mainCurrency);

            //initialize main map
            currenciesMap = new Dictionary<Currency, Dictionary<Currency, double>>()
            {
                { mainCurrency, new Dictionary<Currency, double>()
                    {
                        {  mainCurrency, 1 }
                    }
                }
            };

            //fill main currecnies
            foreach (Currency currency in secondaryCurrencies)
            {
                var rawHtml = fastWeb.Get($"search?hl=en&gl=en&q={currency}+to+{mainCurrency}");

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(rawHtml);
                HtmlNode docNode = document.DocumentNode;

                string exValue = valueExtractor.ExtractSingle(docNode);
                double.TryParse(exValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedDouble);

                currenciesMap[mainCurrency].Add(currency, parsedDouble);                
            }

            //fill reverse values
            foreach (Currency secondaryCurrency in secondaryCurrencies)
            {
                currenciesMap.Add(secondaryCurrency, new Dictionary<Currency, double>());

                foreach (Currency currency in allCurrencies)
                {
                    var rate = currenciesMap[mainCurrency][currency] / currenciesMap[mainCurrency][secondaryCurrency];
                    currenciesMap[secondaryCurrency].Add(currency, rate);
                }
            }
        }

        /// <summary>
        /// Get rate between provided currencies.
        /// Rate = How many [alpha]s in [beta]s.
        /// </summary>
        public static double GetRateFor(Currency alpha, Currency beta)
        {
            if (currenciesMap == null)
                Initalize();

            return currenciesMap[alpha][beta];
        }
    }
}
