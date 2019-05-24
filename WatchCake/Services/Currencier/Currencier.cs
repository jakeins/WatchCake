using HtmlAgilityPack;
using System.Collections.Generic;
using System.Globalization;
using WatchCake.Parsers;
using WatchCake.Parsers.Enums;
using WatchCake.Services.Molder;
using WatchCake.Services.FastWebNS;
using System;
using ExpirableRate = System.Tuple<decimal, System.DateTime?>;

namespace WatchCake.Services.Currencier
{
    /// <summary>
    /// Infrastructure-level service dealing with currency.
    /// Is lazy-loaded in terms of each currency rate request.
    /// </summary>
    public static class Currencier
    {        
        /// <summary>
        /// Main currency. Defined by the app domain.
        /// </summary>
        public static Currency MainCurrency { get; set; } = Currency.USD;

        /// <summary>
        /// The map keeping all rates relative to the main currency. Tuple [rate],[expires (null = never)]
        /// </summary>
        static Dictionary<Currency, ExpirableRate> Rates = new Dictionary<Currency, ExpirableRate>()
        {
            { MainCurrency, new ExpirableRate(1,null) }
        };

        /// <summary>
        /// Web-pulling service instance.
        /// </summary>
        static FastWeb2 fastWeb;

        /// <summary>
        /// Rate parsing rules container. 
        /// </summary>
        static BitParser rateParser;

        /// <summary>
        /// Prepare currencier for retrieving rates from all possible sources.
        /// </summary>
        static void EnsureImportingMeansSetup()
        {
            if (fastWeb != null)
                return;

            fastWeb = new FastWeb2("https://www.google.com/");

            rateParser = new BitParser()
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
        }

        /// <summary>
        /// Make sure the rate for provided currency is known.
        /// </summary>
        static void EnsureValidRate(Currency currency)
        {
            if (Rates.ContainsKey(currency))
            {
                if (Rates[currency].Item2 is null || Rates[currency].Item2 > DateTime.Now)
                    return;
                else
                {
                    Logger.Log(currency + $" rate has expired since {Rates[currency].Item2} @ Currencier");
                    Rates.Remove(currency);
                }
            }
            else
                Logger.Log(currency + " rate is not defined @ Currencier");

            EnsureImportingMeansSetup();
            
            //Extraction from Web
            var rawHtml = fastWeb.Get($"search?hl=en&gl=en&q={currency}+to+{MainCurrency}");

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(rawHtml);
            HtmlNode docNode = document.DocumentNode;

            string exValue = rateParser.ExtractSingle(docNode);
            decimal.TryParse(exValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed);


            //Storing
            var expires = DateTime.Now + TimeSpan.FromHours(6);
            Rates.Add(currency, new ExpirableRate(parsed, expires));
            Logger.Log(currency + $" rate set: {parsed} @ Currencier");
        }

        /// <summary>
        /// Get rate between provided currencies.
        /// Rate = How many [alpha]s in [beta]s.
        /// </summary>
        public static decimal GetRateFor(Currency alpha, Currency beta)
        {
            EnsureValidRate(alpha);
            EnsureValidRate(beta);

            return Rates[beta].Item1 / Rates[alpha].Item1;
        }
    }
}
