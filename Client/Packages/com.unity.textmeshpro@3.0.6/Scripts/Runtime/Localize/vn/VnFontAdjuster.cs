using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace TMPro
{
    public static class VnFontAdjuster
    {
        public static bool enable = false;

        private static StringBuilder sb = new StringBuilder(512);

        public static string Adjust(string s)
        {
            if (s == null || s.Length == 0)
            {
                return s;
            }

            var length = s.Length;
            sb.EnsureCapacity(length);
            sb.Clear();

            for (var i = 0; i < length; i++)
            {
                var c = s[i];

                char past_c = '\0';
                if (i - 1 >= 0)
                {
                    past_c = s[i - 1];
                }
                char past2_c = '\0';
                if (i - 2 >= 0)
                {
                    past2_c = s[i - 2];
                }
                // i check
                char next_c = '\0';
                if (i + 1 < length)
                {
                    next_c = s[i + 1];
                }

                if ((IsVowelUpper(c) && past_c != '\0' && IsLower(past_c)) || (IsVowelUpper(c) && past_c != '\0' && IsStress(past_c) && past2_c != '\0' && IsLower(past2_c)))
                {
                    c = Switch2Lower(c);
                    sb.Append(c);
                }else if(IsLower(c) && IsSpecialLetter(c) && next_c != '\0' && IsSymbol(next_c))
                {
                    c = Switch2NoHead(c);
                    sb.Append(c);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString(0,sb.Length);
        }
        private static char Switch2Lower(char c)
        {
            switch (c)
            {
                case '\x0301':
                    c = '\xF750';
                    break;
                case '\x0302':
                    c = '\xF751';
                    break;
                case '\x0300':
                    c = '\xF752';
                    break;
                case '\x0303':
                    c = '\xF753';
                    break;
                case '\x0306':
                    c = '\xF754';
                    break;
                case '\x0309':
                    c = '\xF755';
                    break;
                case '\x60':
                    c = '\xF756';
                    break;
                case '\xB4':
                    c = '\xF757';
                    break;
            }
            return c;
        }

        private static char Switch2NoHead(char c)
        {
            switch (c)
            {
                case '\x69':
                    c = '\x0131';
                    break;
                case '\xFF49':
                    c = '\x0131';
                    break;
            }
            return c;
        }

        private static bool IsStress(char c)
        {
            return c == '\x0323';
        }

        private static bool IsSpecialLetter(char c)
        {
            return c == '\x69' || c == '\xFF49';
        }

        private static bool IsSymbol(char c)
        {
            return IsVowelUpper(c) || 
                c == '\xF750' || c == '\xF751' || c == '\xF752' || c == '\xF753' || c == '\xF754' || c == '\xF755' || c == '\xF756' || c == '\xF757';
        }

        private static bool IsVowelUpper(char c)
        {
            return c == '\x0301' || c == '\x0302' || c == '\x0300' || c == '\x0303' || c == '\x0306' || c == '\x0309' || c == '\x60' || c == '\xB4';
        }

        private static bool IsLower(char c)
        {
            // SARA U, SARA UU, PHINTHU
            return (c >= '\x61' && c <= '\x7A') || (c >= '\xFF41' && c <= '\xFF5A');
        }

        private static bool IsUpper(char c)
        {
            return (c >= '\x41' && c <= '\x5A') || (c >= '\xFF21' && c <= '\xFF3A'); ;
        }


    }
}

