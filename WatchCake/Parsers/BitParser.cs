using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System;
using System.Linq;
using WatchCake.Parsers.Enums;
using WatchCake.Services.Molder;

namespace WatchCake.Parsers
{
    /// <summary>
    /// Instruction on how to extract single deepest information piece, or list of those.
    /// </summary>
    public class BitParser
    {
        /// <summary>
        /// The choosen method for selection of the external information piece.
        /// </summary>
        public SelectMethod SelectMethod { get; set; }

        /// <summary>
        /// Selection query string to be used for a SelectMethod.
        /// </summary>
        public string SelectQuery { get; set; }

        /// <summary>
        /// Type of the specified Detail.
        /// </summary>
        public SelectDetailType DetailType { get; set; }

        /// <summary>
        /// The deepest level information containing entity.
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// Text processing conveyor to shape the deepest information piece.
        /// </summary>
        public IEnumerable<Mold> PostProcess { get; set; }

        #region Single extraction

        /// <summary>
        /// Single-mode apadtor.
        /// </summary>
        public string ExtractSingle(string str) => ExtractSingle(str, null);

        /// <summary>
        /// Single-mode apadtor.
        /// </summary>
        public string ExtractSingle(HtmlNode node) => ExtractSingle(null, node);

        /// <summary>
        /// Router. Selects appropriate processing method and uses it, does postprocessing.
        /// </summary>
        public string ExtractSingle(string str, HtmlNode node)
        {
            bool strFormalMode = str != null;
            bool nodeFormalMode = node != null;
            bool regexMode = SelectMethod == SelectMethod.Regex;

            if (strFormalMode == nodeFormalMode)
                throw new InvalidOperationException("This is a two-mode method, please provide single and only single parameter.");

            if(strFormalMode != regexMode)
                throw new InvalidOperationException("String formal mode can be used only with Regex SelectMethod.");

            string result = "";

            if (strFormalMode)
            {
                result = ExtractSingleFromString(str);
            }
            else
            {
                if (regexMode)
                    result = ExtractSingleFromString(node.InnerHtml);
                else
                    result = ExtractSingleFromNode(node);
            }

            //Console.WriteLine("Postprocessing.");

            if (PostProcess != null)
                result = result.Mold(PostProcess);

            //Console.WriteLine("Postprocessed.");

            return result;
        }

        /// <summary>
        /// Actual single extraction: from Node.
        /// </summary>
        public string ExtractSingleFromNode(HtmlNode node)
        {
            string result = "";

            HtmlNode deepNode = node.SelectSingleNode(SelectQuery);

            if (deepNode == null)
                throw new NullReferenceException($"Failed selecting '{SelectQuery}' from '{node.Name}' ('{node.XPath}').");

            switch (DetailType)
            {
                case SelectDetailType.Attribute:
                    result = deepNode.GetAttributeValue(Detail, "");
                    break;
                case SelectDetailType.Property:
                    switch (Detail)
                    {
                        case "InnerText":
                            result = deepNode.InnerText;
                            break;
                    }
                    break;
            }        

            return result;
        }

        /// <summary>
        /// Actual single extraction: from string.
        /// </summary>
        public string ExtractSingleFromString(string entireString)
        {
           return Regex.Match(entireString, SelectQuery).Groups[1].Value;
        }

        #endregion Single extraction

        #region List Extraction
        /// <summary>
        /// Actual list extraction: from Node.
        /// </summary>
        public List<HtmlNode> ExtractList(HtmlNode node)
        {
            var nodes = node.SelectNodes(SelectQuery);

            if (nodes == null || nodes.Count < 1)
                throw new NullReferenceException($"Failed selecting '{SelectQuery}' from '{node.Name}'.");

            if (PostProcess != null)
                throw new InvalidOperationException("Bulk nodes Postprocessing is not supported here.");

            return nodes.ToList();
        }

        /// <summary>
        /// Actual list extraction: from string.
        /// </summary>
        public List<string> ExtractList(string entireString)
        {
            var strings = new List<string>();

            MatchCollection matches = Regex.Matches(entireString, SelectQuery);

            foreach (Match match in matches)
                strings.Add(match.Value);

            if (strings.Count < 1)
                throw new NullReferenceException("Extraction yielded 0 results.");

            if (PostProcess != null)
                for (int i = 0; i < strings.Count; i++)
                    strings[i] = strings[i].Mold(PostProcess);

            return strings;
        }
        #endregion List Extraction
    }
}
