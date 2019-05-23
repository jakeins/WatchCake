using System;
using WatchCake.Helpers;
using WatchCake.Parsers;
using WatchCake.Parsers.Enums;
using WatchCake.Services.Currencier;
using WatchCake.Services.Molder;

namespace WatchCake.Data.BulitInParsers
{
    public static partial class BuiltInParsers
    {
        public static PageParser SportsDirect = new PageParser()
        {
            ID = 8,
            Name = "SportsDirect (-0% +30€)",
            Domain = new Uri("https://ua.sportsdirect.com"),

            OrderDeduction = 0,
            OrderLimit = new Money(150, Currency.EUR),
            OrderExtra = new Money(30M, Currency.EUR),

            PagePreprocess = new[]
            {
                new Mold(MoldType.RegexReplace, "&quot;", "\""),
                new Mold(MoldType.StripHtmlComments),
                new Mold(MoldType.HtmlDecode),

            },
            Title = new BitParser()
            {
                SelectMethod = SelectMethod.XPath,
                DetailType = SelectDetailType.Property,
                SelectQuery = "//span[@id='lblProductBrand']",
                Detail = "InnerText",
            },
            Subtitle = new BitParser()
            {
                SelectMethod = SelectMethod.XPath,
                DetailType = SelectDetailType.Property,
                SelectQuery = "//span[@id='lblProductName']",
                Detail = "InnerText",
            },
            DefaultOptionMode = DefaultOptionMode.Ignore,

            OptionsList = new BitParser()
            {
                SelectMethod = SelectMethod.Regex,
                SelectQuery = "{\"[sS]ize[nN]ame\":.+?}",
            },
            ActualOption = new OptionParser()
            {
                Code = new BitParser()
                {
                    SelectMethod = SelectMethod.Regex,
                    SelectQuery = "[sS]ize[vV]ar[iI][dD]\":\"(\\d+)",
                    PostProcess = new[]
                    {
                        new Mold(MoldType.Substr, (-5).ToString() ),
                    }
                },
                PropertyA = new BitParser()
                {
                    SelectMethod = SelectMethod.Regex,
                    SelectQuery = "[sS]ize[vV]ar[iI][dD]\":\"(\\d+)",
                    PostProcess = new[]
                    {
                        new Mold(MoldType.Substr, (-5).ToString(), 2.ToString() ),
                        new Mold(MoldType.Prepend, "color "),
                        new Mold(MoldType.Trim)
                    }
                },
                PropertyB = new BitParser()
                {
                    SelectMethod = SelectMethod.Regex,
                    SelectQuery = "[sS]ize[nN]ame\":\"(.+?)\",",
                    PostProcess = new[]
                    {
                        new Mold(MoldType.Remove, "=\"\""),
                        new Mold(MoldType.Trim)
                    }
                },
                PriceAmount = new BitParser()
                {
                    SelectMethod = SelectMethod.Regex,
                    SelectQuery = "[sS]ell[pP]rice[aA]mount\":(.+?),",
                },
                PriceCurrency = Currency.EUR
            }
        };
    }
}
