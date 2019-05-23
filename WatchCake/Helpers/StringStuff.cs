using System;
using System.Text.RegularExpressions;

namespace WatchCake.Helpers
{
    public static class StringStuff
    {
        /// <summary>
        /// PHP-like substr, which can take negative length, indicating the right bound relative to the current end
        /// </summary>
        public static string SubstrSafe(string inString, int inStart, int inLength = int.MaxValue)
        {
            int realStart = inStart >= 0 ? inStart : (inString.Length + inStart);

            if (realStart < 0)
                realStart = 0;

            string subject = inString.Substring(realStart);
            int maxLen = inLength > subject.Length ? subject.Length : inLength;
            subject = subject.Substring(0, maxLen);
            return subject;
        }

        /// <summary>
        /// Generates hash of a limited length.
        /// </summary>
        public static string ShortHashCode(string plainText, int limit = int.MaxValue) => SubstrSafe(Base64Encode(plainText.GetHashCode().ToString()), 0, limit);
        
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Convert uri symbols so it can be used as a filename. Irreversible.
        /// </summary>
        public static string Uri2filename(Uri inUri)
        {
            string ouut = Regex.Replace(inUri.ToString().Replace('/', '{').Replace('?', '7').Replace(':', '$')
                                    , @"[^a-zA-Z0-9{=\-_+&.$]", "") + "_";
            return SubstrSafe(ouut, 0, 100) + SHAer.GenerateSHA256String(inUri.ToString());
        }
    }    
}
