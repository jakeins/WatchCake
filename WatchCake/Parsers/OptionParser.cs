using HtmlAgilityPack;
using System;
using System.Globalization;
using WatchCake.Helpers;
using WatchCake.Models;
using WatchCake.Parsers.Models;
using WatchCake.Services;
using WatchCake.Services.Currencier;

namespace WatchCake.Parsers
{
    /// <summary>
    /// Instruction on how to extract option and its current snapshot.
    /// </summary>
    public class OptionParser
    {
        //References to the crude parsing result of the corresponding model properties.
        public BitParser Code { get; set; }
        public BitParser PropertyA { get; set; }
        public BitParser PropertyB { get; set; }
        public BitParser PropertyC { get; set; }
        public BitParser Stock { get; set; }        
        public BitParser PriceAmount { get; set; }
        public Currency PriceCurrency { get; set; }

        /// <summary>
        /// Call for starting parsing by the contained rules, using provided crude information object.
        /// </summary>
        public OptionParseResult Parse(object obj)
        {
            if (obj is HtmlNode node)
                return Parse(node);
            else if (obj is string str)
                return Parse(str);
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// Call for starting parsing by the contained rules, using provided crude information string.
        /// </summary>
        public OptionParseResult Parse(string str) => Parse(str, null);

        /// <summary>
        /// Call for starting parsing by the contained rules, using provided crude information HtmlNode.
        /// </summary>
        public OptionParseResult Parse(HtmlNode node) => Parse(null, node);

        /// <summary>
        /// Parse option having crude option, string or HtmlMode. Is a variable-argument method.
        /// </summary>
        public OptionParseResult Parse(string str, HtmlNode node)
        {
            bool strFormalMode = str != null;
            bool nodeFormalMode = node != null;

            if (strFormalMode == nodeFormalMode)
                throw new InvalidOperationException("This is a two-mode method, please provide only single desired parameter.");

            if(PriceAmount == null)
                throw new NullReferenceException($"Price extraction is not defined.");

            #region Option Info Parsing

            var newOptionParse = new OptionParseResult();

            // Details
            try
            {
                if (Code != null)
                    newOptionParse.Code = Code.ExtractSingle(str, node);
            }
            catch (NullReferenceException nrex) { Logger.Log(nrex.Message + " @ Option Code Parsing"); }

            try
            {
                if (PropertyA != null)
                    newOptionParse.PropertyA = PropertyA.ExtractSingle(str, node);
            }
            catch (NullReferenceException nrex) { Logger.Log(nrex.Message + " @ Option PropertyA Parsing"); }

            try
            {
                if (PropertyB != null)
                    newOptionParse.PropertyB = PropertyB.ExtractSingle(str, node);
            }
            catch (NullReferenceException nrex) { Logger.Log(nrex.Message + " @ Option PropertyB Parsing"); }

            try
            {
                if (PropertyC != null)
                    newOptionParse.PropertyC = PropertyC.ExtractSingle(str, node);
            }
            catch (NullReferenceException nrex) { Logger.Log(nrex.Message + " @ Option PropertyC Parsing"); }

            #endregion Option Info Parsing

            #region Snapshot Info Parsing
            var snapshot = new Snapshot(newOptionParse)
            {
                Timestamp = DateTime.Now
            };

            try
            {
                if (Stock != null)
                    newOptionParse.StockStatus = Stock.ExtractSingle(str, node);
            }
            catch (NullReferenceException nrex) { Logger.Log(nrex.Message + " @ Option Snapshot Stock Parsing"); }

            ///Price
            var extracted = PriceAmount.ExtractSingle(str, node);
            double.TryParse(extracted, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedDouble);

            if (parsedDouble <= 0)
                throw new NullReferenceException($"Zero or negative price [{parsedDouble}] is not permitted.");

            newOptionParse.Price = new Money((decimal)parsedDouble, PriceCurrency);
            #endregion Snapshot Info Parsing

            return newOptionParse;
        }
    }
}