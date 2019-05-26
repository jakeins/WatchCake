using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using WatchCake.Parsers.Enums;
using WatchCake.Services;
using WatchCake.Services.Molder;
using WatchCake.Parsers.Models;
using WatchCake.Services.FastWebNS;
using WatchCake.Helpers;
using WatchCake.Models.Bases;

namespace WatchCake.Parsers
{
    /// <summary>
    /// Shop page parser and shop options.
    /// </summary>
    public class PageParser : IdObject
    {
        /// <summary>
        /// Name of the current parser, for display purposes.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Domain at which this parser is based on.
        /// </summary>
        public Uri Domain { get; set; }        

        /// <summary>
        /// Preprocessing conveyor for the entire page html content.
        /// </summary>
        public IEnumerable<Mold> PagePreprocess { get; set; }

        //References to the crude parsing result of the corresponding model properties.
        public BitParser Title { get; set; }
        public BitParser Subtitle { get; set; }

        public BitParser Thumbnail { get; set; }

        public DefaultOptionMode DefaultOptionMode { get; set; }
        public OptionParser DefaultOption { get; set; }

        public BitParser OptionsList { get; set; }
        public OptionParser ActualOption { get; set; }
        public PriceMode PriceMode { get; set; }

        #region Shop Ordering Rules
        /// <summary>
        /// The share of price deduction by discounts, tax deductions etc.
        /// </summary>
        public decimal OrderDeduction { get; set; }

        /// <summary>
        /// The limit of order price. Useful for avoiding customs fees.
        /// </summary>
        public Money OrderLimit { get; set; }

        /// <summary>
        /// The additional price charged for handling, shipping, fees etc.
        /// </summary>
        public Money OrderExtra  { get; set; }
        #endregion Shop Ordering Rules

        FastWeb2 fastWeb;

        //Memberwise copy-constructor to produce another parsers based on current, having another name and domain.
        public PageParser ProduceTwin(int? id, string name, string domain)
        {
            var fresh = (PageParser)MemberwiseClone();
            fresh.ID = id;
            fresh.Name = name;
            fresh.Domain = new Uri(domain);
            return fresh;
        }

        /// <summary>
        /// Call for starting parsing by the contained rules, using provided crude information html content string.
        /// </summary>
        public PageParseResult Parse(string relativeUri = null)
        {
            if(fastWeb == null)
                fastWeb = new FastWeb2(Domain);

            var rawHtml = fastWeb.Get(relativeUri);

            //PreprocessPlan
            string processedHtml = PagePreprocess == null ? rawHtml : rawHtml.Mold(PagePreprocess);

            //interpret as document node
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(processedHtml);
            HtmlNode entireDocumentNode = document.DocumentNode;

            var pageParseResult = new PageParseResult
            {
                Title = Title.ExtractSingle(node: entireDocumentNode)
            };

            if (Subtitle != null)
                pageParseResult.Title += " " + Subtitle.ExtractSingleFromNode(node: entireDocumentNode);

            if(Thumbnail != null)
                pageParseResult.ThumbPath = Thumbnail.ExtractSingleFromNode(node: entireDocumentNode);

            //Parsing Actual Options
            if(DefaultOptionMode != DefaultOptionMode.Single)
            {
                try
                {
                    bool isRegex = OptionsList.SelectMethod == SelectMethod.Regex;

                    IEnumerable<object> crudeOptions;

                    if (isRegex)
                        crudeOptions = OptionsList.ExtractList(entireDocumentNode.InnerHtml);
                    else
                        crudeOptions = OptionsList.ExtractList(entireDocumentNode);

                    foreach (var entry in crudeOptions)
                    {
                        var optionParseResult = isRegex ? ActualOption.Parse(entry as string) : ActualOption.Parse(entry as HtmlNode);

                        if (optionParseResult.Price.Amount == 0 && PriceMode != PriceMode.Add)
                            continue;

                        pageParseResult.OptionParseResults.Add(optionParseResult);
                    }
                }
                catch(NullReferenceException nrex) when (DefaultOptionMode == DefaultOptionMode.Alongside || DefaultOptionMode == DefaultOptionMode.Alternative)
                {
                    //swallow when actual options absence allowed
                    Logger.Log(nrex.Message + " @ PageParsing of " + relativeUri);
                }
            }            

            //Default Option Logic
            if (DefaultOptionMode != DefaultOptionMode.Ignore)
            {
                if (DefaultOptionMode == DefaultOptionMode.Single)
                    Logger.Log(Domain + " has 1 option per page setup. Add all options separately.");

                OptionParseResult defaultOption;

                try
                {
                    defaultOption = DefaultOption.Parse(entireDocumentNode);
                    defaultOption.PropertyA = defaultOption.Code = DefaultOptionMode.ToString();
                }
                catch (NullReferenceException nrex)
                {
                    Logger.Log(nrex.Message + " @ PageParsing of " + relativeUri);
                    throw;
                }

                //Prices correction according to Price Mode Setting
                if (PriceMode == PriceMode.Add)
                    foreach (OptionParseResult optParseRes in pageParseResult.OptionParseResults)
                        optParseRes.Price.Amount += defaultOption.Price.Amount;

                //Adding default option to the options list, if necessary
                if (DefaultOptionMode == DefaultOptionMode.Alongside
                    || DefaultOptionMode == DefaultOptionMode.Single
                    || (DefaultOptionMode == DefaultOptionMode.Alternative && pageParseResult.OptionParseResults.Count == 0))
                    pageParseResult.OptionParseResults.Add(defaultOption);
            }

            //Prices correction according to the Main Currency & Shop Ordering Rules
            foreach (OptionParseResult optParseRes in pageParseResult.OptionParseResults)
            {
                optParseRes.Price = optParseRes.Price.AsMainCurrency();
                var value = optParseRes.Price.Amount;

                value /= 1 + OrderDeduction;

                if(OrderExtra != null && OrderLimit != null && OrderLimit.AsMainCurrency().Amount > 0)
                {
                    decimal limitValue = OrderLimit.AsMainCurrency().Amount;
                    decimal orderExtraValue = OrderExtra.AsMainCurrency().Amount;
                    decimal piecesPerOrder = limitValue / value;
                    decimal extraPerPiece = orderExtraValue / piecesPerOrder;
                    value += extraPerPiece;
                }                    

                optParseRes.Price.Amount = value;
            }

            return pageParseResult;
        }
    }
}