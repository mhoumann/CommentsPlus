using System;
using System.Collections.Generic;
using System.Linq;

namespace CommentsPlus
{
    static class ListExtensions
    {
        public static List<T> AsList<T>(this IEnumerable<T> source)
        {
            var list = source as List<T>;
            if (list != null)
                return list;
            return source.ToList();
        }

        public static bool Contains<T>(this List<T> list, T value, IEqualityComparer<T> comparer)
        {
            if (list != null)
            {
                if (comparer == null)
                    comparer = EqualityComparer<T>.Default;

                foreach (var t in list)
                {
                    if (comparer.Equals(t, value))
                        return true;
                }

            }
            return false;
        }
    }

    static class StringExtensions
    {
        public static bool Contains(this string text, string value, StringComparison comparison)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
                return false;

            int index = text.IndexOf(value, comparison);
            return index >= 0;
        }

        public static bool StartsWith(this string text, string value, int startIndex, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(text) || startIndex > text.Length || string.IsNullOrEmpty(value))
                return false;

            return text.IndexOf(value, startIndex, comparison) == startIndex;
        }

        public static string StartsWithOneOf(this string text, string[] strings, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(text) || strings == null || strings.Length == 0)
                return null;

            foreach (string t in strings)
            {
                if (text.StartsWith(t, comparison))
                    return t;
            }

            return null;
        }

        public static string StartsWithOneOf(this string text, int startIndex, string[] strings, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(text) || strings == null || strings.Length == 0)
                return null;

            foreach (string t in strings)
            {
                if (text.StartsWith(t, startIndex, comparison))
                    return t;
            }

            return null;
        }

        public static bool StartsWithWhiteSpace(this string text, int startIndex)
        {
            if (string.IsNullOrEmpty(text) || startIndex >= text.Length)
                return false;
            return char.IsWhiteSpace(text, startIndex);
        }

        public static string EqualsOneOf(this string text, string[] strings, StringComparison comparison = StringComparison.Ordinal)
        {
            if (strings == null || strings.Length == 0)
                return null;

            foreach (string t in strings)
            {
                if (string.Equals(text, t, comparison))
                    return t;
            }
            return null;
        }
    }

}
