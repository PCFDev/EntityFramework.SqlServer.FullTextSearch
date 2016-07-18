using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EntityFramework.SqlServer.FullTextSearch
{
    internal static class FullTextTags
    {
        //We need tags that are unlikely to occur in a normal contains predicate.
        public const string ContainsTag = "{CONTAINS-AF2E-457D-81C2-85DACAA23B9C}";

        public const string FreeTextTag = "{FREETEXT-CE74-4E5F-9166-878AFA2AC1DF}";

        public static readonly Regex AnyTag = new Regex(String.Format(CultureInfo.InvariantCulture, "(?:{0}|{1})", Regex.Escape(ContainsTag), Regex.Escape(FreeTextTag)), RegexOptions.Compiled);

        public static string Contains(string predicate)
        {
            return String.Format(CultureInfo.InvariantCulture, "({0}{1})", ContainsTag, predicate);
        }

        public static string FreeText(string predicate)
        {
            return String.Format(CultureInfo.InvariantCulture, "({0}{1})", FreeTextTag, predicate);
        }
    }
}