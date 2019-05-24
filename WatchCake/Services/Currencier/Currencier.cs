using HtmlAgilityPack;
using System.Globalization;
using WatchCake.Parsers;
using WatchCake.Parsers.Enums;
using WatchCake.Services.Molder;
using WatchCake.Services.FastWebNS;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WatchCake.Services.Currencier
{
    /// <summary>
    /// Infrastructure-level service dealing with currency.
    /// Is lazy-loaded in terms of each currency rate request.
    /// Has backup from-file functionality.
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
            { Currency.USD, new ExpirableRate(1) }
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
        /// Dynamic expiration time from current moment.
        /// </summary>
        static DateTime DefaultExpiration => DateTime.Now + TimeSpan.FromHours(6);

        /// <summary>
        /// Delegate of decision making logic about reading rates from the file.
        /// </summary>
        public static Func<string, bool> IsFileReadAllowed = (c) => 
        {
            Logger.Log("Reading rates from file is not allowed.");
            return false;
        };

        #region Storage
        static readonly string filePath = "rates.cfg";
        static readonly CultureInfo fileCulture = CultureInfo.GetCultureInfo("en-GB");

        /// <summary>
        /// Save all rates to file.
        /// </summary>
        static void WriteAllRates()
        {
            StringBuilder content = new StringBuilder();

            foreach (var entry in Rates)
                content.Append($"{entry.Key} to {MainCurrency}       {entry.Value.Rate.ToString("F4", fileCulture).PadRight(10, ' ')}   ({entry.Value.Expires.ToString(fileCulture)})\n");

            File.WriteAllText(filePath, content.ToString());
        }

        /// <summary>
        /// Create template file.
        /// </summary>
        static void WriteAllRatesTemplate()
        {
            StringBuilder content = new StringBuilder();

            var currencies = Enum.GetValues(typeof(Currency)).Cast<Currency>().Select(c=>c.ToString());

            foreach (var currency in currencies)
                content.Append($"{currency} to {MainCurrency}       xxx   ({DefaultExpiration})\n");

            File.WriteAllText(filePath, content.ToString());
        }

        /// <summary>
        /// Set all rates according to file content.
        /// </summary>
        static void ReadAllRates()
        {
            Rates.Clear();

            //saving with empty Rates would create empty file template.
            if (!File.Exists(filePath))
                WriteAllRatesTemplate();

            foreach (string entry in File.ReadAllText(filePath).Split('\n'))
            {
                if (string.IsNullOrEmpty(entry))
                    continue;

                // Importing example 
                // EUR to USD     1.1200       (25/05/2019 00:49:29)
                var whole = Regex.Replace(entry, @"\s+", " ");
                var bits = whole.Split(' ');
                var currencyBit = bits[0];
                var valueBit = bits[3].Replace(',', '.');
                var expiresBit = $"{bits[4].Substring(1)} {bits[5].Substring(0, bits[5].Length - 1)}";

                Enum.TryParse(currencyBit, out Currency currency);
                decimal.TryParse(valueBit, NumberStyles.Any, fileCulture, out decimal value);
                DateTime.TryParse(expiresBit, fileCulture, DateTimeStyles.None, out DateTime expires);

                if (currency.ToString() != bits[0] || value <= 0 || expires <= DateTime.Now)
                    continue;

                SetRate(currency, new ExpirableRate(value, expires));                
            }
        }
        #endregion Storage

        /// <summary>
        /// Sets the rate for the specified currency.
        /// </summary>
        static void SetRate(Currency currency, ExpirableRate exrate)
        {
            Rates[currency] = exrate;
            Logger.Log(currency + $" rate set: {exrate.Rate} until {exrate.Expires} @ Currencier");
        }

        /// <summary>
        /// Prepare currencier for retrieving rates from all possible sources.
        /// </summary>
        static void EnsureImportingMeansSetup()
        {
            if (fastWeb != null)
                return;

            fastWeb = new FastWeb2("https://www.google.com/") { WebAccessRetries = 0 };

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
            //Check if there is data in memory
            if (Rates.ContainsKey(currency))
            {
                if (Rates[currency].Expires > DateTime.Now)
                    return;
                else
                {
                    Logger.Log(currency + $" rate has expired since {Rates[currency].Expires} @ Currencier");
                    Rates.Remove(currency);
                }
            }
            else
                Logger.Log(currency + " rate is not defined @ Currencier");

            EnsureImportingMeansSetup();

            //Extraction
            string rawHtml = null;
            try
            {
                //Extraction from Web
                rawHtml = fastWeb.Get($"search?hl=en&gl=en&q={currency}+to+{MainCurrency}");
            }
            catch(FastWeb2Exception)
            {
                Logger.Log($"Cannot retrieve remote rate for {currency}. Reading all rates from file [{filePath}]");

                //Extraction from file
                AttemptReadRatesFromFile(currency);
                return;
            }

            //Having remote page pulled down, continue extraction
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(rawHtml);
            HtmlNode docNode = document.DocumentNode;

            string exValue = rateParser.ExtractSingle(docNode);
            decimal.TryParse(exValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value);


            //Saving
            SetRate(currency, new ExpirableRate(value, DefaultExpiration));

            //Storing
            WriteAllRates();
        }

        /// <summary>
        /// Try to get rates from file, relying to file read decision making delegate.
        /// </summary>
        static void AttemptReadRatesFromFile(Currency currency)
        {
            while (!Rates.ContainsKey(currency) || Rates[currency].Expires <= DateTime.Now)
            {
                var reason = $"The necessary currency {currency} rate is not available.";
                Logger.Log(reason);

                if (IsFileReadAllowed(currency + " to " + MainCurrency))
                    ReadAllRates();
                else
                    throw new CurrencierException(reason);
            }
        }

        /// <summary>
        /// Get rate between provided currencies.
        /// Rate = How many [alpha]s in [beta]s.
        /// </summary>
        public static decimal GetRateFor(Currency alpha, Currency beta)
        {
            EnsureValidRate(alpha);
            EnsureValidRate(beta);

            return Rates[beta].Rate / Rates[alpha].Rate;
        }
    }
}
