using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using WatchCake.Helpers;

namespace WatchCake.Services.Molder
{
    public static class Molder
    {
        /// <summary>
        /// Map of all available molders.
        /// </summary>
        static readonly Dictionary<MoldType, Action<StringBuilder, IList<string>>> map;


        /// <summary>
        /// Static constructor, initializes molds map.
        /// </summary>
        static Molder()
        {
            map = new Dictionary<MoldType, Action<StringBuilder, IList<string>>>()
            {
                { MoldType.Append, Append },
                { MoldType.Prepend, Prepend },
                { MoldType.FloatAdd, FloatAdd },
                { MoldType.FloatMult, FloatMult },
                { MoldType.Float1DivX, Float1DivX },
                { MoldType.AddIfMissing, AddIfMissing },
                { MoldType.Spacer, Spacer },
                { MoldType.DeSpace, DeSpace },
                { MoldType.OnlyFloatChars, OnlyFloatChars },
                { MoldType.FakeLength, FakeLength },
                { MoldType.Substr, Substr },
                { MoldType.Trim, Trim },
                { MoldType.HtmlDecode, HtmlDecode },
                { MoldType.Before, Before },
                { MoldType.After, After },
                { MoldType.AfterLast, AfterLast },
                { MoldType.Between, Between },
                { MoldType.Commas2points, Commas2points },
                { MoldType.Replace, Replace },
                { MoldType.Remove, Remove },
                { MoldType.StripHtmlComments, StripHtmlComments },
                { MoldType.SetIfEmpty, SetIfEmpty },
                { MoldType.ReplaceIfLonger, ReplaceIfLonger },
                { MoldType.CutEnd, CutEnd },
                { MoldType.CutStart, CutStart },
                { MoldType.EasyHash, EasyHash },
                { MoldType.TitleCase, TitleCase },
                { MoldType.RegexReplace, RegexReplace },
                { MoldType.Override, Override }
            };
        }

        /// <summary>
        /// Processes string in builder with a specified Mold.
        /// </summary>
        public static void Mold(this StringBuilder subject, Mold mold)
        {
            if (mold == null)
                throw new ArgumentNullException(nameof(mold));

            map[mold.Type](subject, mold.Attributes);
        }

        /// <summary>
        /// Processes string in builder with a specified set of Molds.
        /// </summary>
        public static void Mold(this StringBuilder subject, IEnumerable<Mold> processingList)
        {
            if (processingList == null)
                throw new ArgumentNullException(nameof(processingList));

            foreach (Mold mold in processingList)
            {
                subject.Mold(mold);
            }
        }
        
        /// <summary>
        /// Returns processed string with the specified Mold.
        /// </summary>
        public static string Mold(this string subject, Mold mold)
        {
            var response = new StringBuilder(subject);
            response.Mold(mold);
            return response.ToString();
        }

        /// <summary>
        /// Returns processed string with the specified Molds set.
        /// </summary>
        public static string Mold(this string subject, IEnumerable<Mold> processList)
        {
            var response = new StringBuilder(subject);
            response.Mold(processList);
            return response.ToString();
        }   

        /// <summary>
        /// Adds string to the end of the subject stirng.
        /// </summary>
        public static void Append(StringBuilder subject, IList<string> attributes)
        {
            subject.Append(attributes[0]);
        }

        /// <summary>
        /// Adds string to the beginning of the subject stirng.
        /// </summary>
        public static void Prepend(StringBuilder subject, IList<string> attributes)
        {
            var g = subject.ToString();
            subject.Insert(0, attributes[0]);
            var h = subject.ToString();
        }

        /// <summary>
        /// Make a mathematical addition of the subject and given values, parsed as double.
        /// </summary>
        public static void FloatAdd(StringBuilder subject, IList<string> attributes)
        {
            if (double.TryParse(subject.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedSubj)
             && double.TryParse(attributes[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedAttr))
            {
                string stringy = (parsedSubj + parsedAttr).ToString(CultureInfo.InvariantCulture);
                subject.Clear();
                subject.Append(stringy);
            }
        }

        /// <summary>
        /// Make a mathematical multiplication of the subject and given values, parsed as double.
        /// </summary>
        public static void FloatMult(StringBuilder subject, IList<string> attributes)
        {
            if (double.TryParse(subject.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedSubj)
             && double.TryParse(attributes[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedAttr))
            {
                string stringy = (parsedSubj * parsedAttr).ToString(CultureInfo.InvariantCulture);
                subject.Clear();
                subject.Append(stringy);
            }
        }

        /// <summary>
        /// Make a mathematical dision of 1 and subject, parsed as double.
        /// </summary>
        public static void Float1DivX(StringBuilder subject, IList<string> attributes)
        {
            string stringy;

            if (double.TryParse(subject.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedSubjZ) && parsedSubjZ != 0)
                stringy = (1 / parsedSubjZ).ToString(CultureInfo.InvariantCulture);
            else
                stringy = 0.ToString();

            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Add the given string to the subject string, if first one is not already there.
        /// </summary>
        public static void AddIfMissing(StringBuilder subject, IList<string> attributes)
        {
            if(subject.ToString().IndexOf(attributes[0]) != -1)
                subject.Append(attributes[0]);
        }

        /// <summary>
        /// Replace all multiple whitespaces with a single space " ".
        /// </summary>
        public static void Spacer(StringBuilder subject, IList<string> attributes)
        {
            string stringy = Regex.Replace(subject.ToString(), @"\s+", " ");
            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Remove all spaces from the subject string.
        /// </summary>
        public static void DeSpace(StringBuilder subject, IList<string> attributes)
        {
            subject.Replace(" ", "");
        }

        /// <summary>
        /// Remove all characters that are not representing float number. Point and comma are both considered as float characters.
        /// </summary>
        public static void OnlyFloatChars(StringBuilder subject, IList<string> attributes)
        {
            string stringy = Regex.Replace(subject.ToString(), @"[^\d-.,]+", "");
            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Ensure the subject string has at least the given length, by adding slashes at the end.
        /// </summary>
        public static void FakeLength(StringBuilder subject, IList<string> attributes)
        {
            while (subject.Length<int.Parse(attributes[0]))
                subject.Append("/");
        }

        /// <summary>
        /// PHP-like substring, takes negative and out-of-bounds values.
        /// </summary>
        public static void Substr(StringBuilder subject, IList<string> attributes)
        {
            int parsedStart = int.Parse(attributes[0]);
            string stringy;

            if (attributes.Count > 1)
            {
                var parsedEnd = int.Parse(attributes[1]);
                stringy = StringStuff.SubstrSafe(subject.ToString(), parsedStart, parsedEnd);
            }
            else
            {
                stringy = StringStuff.SubstrSafe(subject.ToString(), parsedStart);
            }

            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Gets rid of whitespaces surrounding subject string.
        /// </summary>
        public static void Trim(StringBuilder subject, IList<string> attributes)
        {
            string stringy = subject.ToString().Trim();
            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Converts HTML character codes into special characters.
        /// </summary>
        public static void HtmlDecode(StringBuilder subject, IList<string> attributes)
        {
            string stringy = WebUtility.HtmlDecode(subject.ToString());
            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Gets the part of the subject string, that is before the given string.
        /// </summary>
        public static void Before(StringBuilder subject, IList<string> attributes)
        {
            int position = subject.ToString().IndexOf(attributes[0]);
            if (position >= 0)
            {
                string stringy = StringStuff.SubstrSafe(subject.ToString(), 0, position);
                subject.Clear();
                subject.Append(stringy);
            }
        }

        /// <summary>
        /// Gets the part of the subject string, that is after the given string.
        /// </summary>
        public static void After(StringBuilder subject, IList<string> attributes)
        {
            int position = subject.ToString().IndexOf(attributes[0]);
            if (position >= 0)
            {
                string stringy = StringStuff.SubstrSafe(subject.ToString(), position + attributes[0].Length);
                subject.Clear();
                subject.Append(stringy);
            }
        }

        /// <summary>
        /// Gets the part of the subject string, that is after the last occurence of the given string.
        /// </summary>
        public static void AfterLast(StringBuilder subject, IList<string> attributes)
        {
            int position = subject.ToString().IndexOf(attributes[0]);
            if (position >= 0)
            {
                string stringy = StringStuff.SubstrSafe(subject.ToString(), position + attributes[0].Length);
                subject.Clear();
                subject.Append(stringy);
            }
        }

        /// <summary>
        /// Gets the part of the subject string, that is between the two given strings.
        /// </summary>
        public static void Between(StringBuilder subject, IList<string> attributes)
        {
            int startPosition = subject.ToString().IndexOf(attributes[0]);
            int endPosition = subject.ToString().IndexOf(attributes[1]);
            string stringy = StringStuff.SubstrSafe(subject.ToString(), startPosition + 1, endPosition - startPosition - 1);
            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Converts all commmas to points.
        /// </summary>
        public static void Commas2points(StringBuilder subject, IList<string> attributes)
        {
            string stringy = subject.ToString().Replace(',', '.');
            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Replaces all occurences of the 1st given string with the 2nd given string.
        /// </summary>
        public static void Replace(StringBuilder subject, IList<string> attributes)
        {
            subject.Replace(attributes[0], attributes[1]);
        }

        /// <summary>
        /// Removes all occurences of the given string.
        /// </summary>
        public static void Remove(StringBuilder subject, IList<string> attributes)
        {
            subject.Replace(attributes[0], "");
        }

        /// <summary>
        /// Removes all html comments from the subject string.
        /// </summary>
        public static void StripHtmlComments(StringBuilder subject, IList<string> attributes)
        {
            string stringy = Regex.Replace(subject.ToString(), "<!--(.*?)-->", "");
            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Set the content of the subject string as the given string, if the subject string is effectively empty.
        /// </summary>
        public static void SetIfEmpty(StringBuilder subject, IList<string> attributes)
        {
            if (string.IsNullOrEmpty(subject.ToString()))
            {
                string stringy = attributes[0];
                subject.Clear();
                subject.Append(stringy);
            }
        }

        /// <summary>
        /// Replace the content of the subject string with the 2nd given string, if the subject string is shorter than 1st length attribute.
        /// </summary>
        public static void ReplaceIfLonger(StringBuilder subject, IList<string> attributes)
        {
            int limit = int.Parse(attributes[0]);

            if (subject.Length > limit)
            {
                string stringy = attributes[1];
                subject.Clear();
                subject.Append(stringy);
            }            
        }

        /// <summary>
        /// Limit the length of the subject string by the specified given legths, by cutting off the end.
        /// </summary>
        public static void CutEnd(StringBuilder subject, IList<string> attributes)
        {
            int limit = int.Parse(attributes[0]);
            if (subject.Length > limit)
            {
                string stringy = StringStuff.SubstrSafe(subject.ToString(), 0, limit) + "…";
                subject.Clear();
                subject.Append(stringy);
            }
        }

        /// <summary>
        /// Limit the length of the subject string by the specified given legths, by cutting off start.
        /// </summary>
        public static void CutStart(StringBuilder subject, IList<string> attributes)
        {
            int limit = int.Parse(attributes[0]);
            if (subject.Length > limit)
            {
                string stringy = "…" + StringStuff.SubstrSafe(subject.ToString(), -limit);

                subject.Clear();
                subject.Append(stringy);
            }
        }
        
        /// <summary>
        /// Do a short hash based on the subject, of a given length.
        /// </summary>
        public static void EasyHash(StringBuilder subject, IList<string> attributes)
        {
            string stringy;

            if (string.IsNullOrEmpty(attributes[0]))
                stringy = StringStuff.ShortHashCode(subject.ToString());
            else
                stringy = StringStuff.ShortHashCode(subject.ToString(), int.Parse(attributes[0]));

            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Convert subject string to a title (proper) case.
        /// </summary>
        public static void TitleCase(StringBuilder subject, IList<string> attributes)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string stringy = textInfo.ToTitleCase(subject.ToString());
            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Perform a Regex-Replace on a subject string with the given attributes.
        /// </summary>
        public static void RegexReplace(StringBuilder subject, IList<string> attributes)
        {
            string stringy = Regex.Replace(subject.ToString(), attributes[0], attributes[1]);
            subject.Clear();
            subject.Append(stringy);
        }

        /// <summary>
        /// Set the content to the specified 0th attribute.
        /// </summary>
        public static void Override(StringBuilder subject, IList<string> attributes)
        {
            subject.Clear();
            subject.Append(attributes[0]);
        }
    }
}
