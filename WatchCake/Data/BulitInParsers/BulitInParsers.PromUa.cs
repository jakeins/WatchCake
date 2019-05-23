using System;
using WatchCake.Parsers;
using WatchCake.Parsers.Enums;
using WatchCake.Services.Currencier;
using WatchCake.Services.Molder;

namespace WatchCake.Data.BulitInParsers
{
    public static partial class BuiltInParsers
    {
        public static PageParser PromUa = new PageParser()
        {
            ID = 7,
            Name = "PromUa",
            Domain = new Uri("https://prom.ua"),

            OrderDeduction = 0,
            OrderLimit = null,
            OrderExtra = null,

            Title = new BitParser()
            {
                SelectMethod = SelectMethod.XPath,
                DetailType = SelectDetailType.Property,
                SelectQuery = "//title",
                Detail = "InnerText",
                PostProcess = new[]
                {
                    new Mold(MoldType.HtmlDecode),
                    new Mold(MoldType.Before, ":"),
                    new Mold(MoldType.Before, " - "),
                    new Mold(MoldType.Trim)
                }
            },


            DefaultOptionMode = DefaultOptionMode.Single,

            DefaultOption = new OptionParser()
            {
                Code = new BitParser()
                {
                    SelectMethod = SelectMethod.XPath,
                    SelectQuery = "//span[@data-qaid='product_code']",
                    DetailType = SelectDetailType.Attribute,
                    Detail = "title"
                },
                Stock = new BitParser()
                {
                    SelectMethod = SelectMethod.XPath,
                    SelectQuery = "//li[@data-qaid='presence_data']",
                    DetailType = SelectDetailType.Property,
                    Detail = "InnerText"
                },
                PriceAmount = new BitParser()
                {
                    SelectMethod = SelectMethod.XPath,
                    SelectQuery = "//span[@data-qaid='product_price']",
                    DetailType = SelectDetailType.Property,
                    Detail = "InnerText",
                    PostProcess = new[]
                    {
                        new Mold(MoldType.Commas2points),
                        new Mold(MoldType.OnlyFloatChars)
                    }
                },
                PriceCurrency = Currency.UAH
            },

            PriceMode = PriceMode.AsIs
        };
    }
}
