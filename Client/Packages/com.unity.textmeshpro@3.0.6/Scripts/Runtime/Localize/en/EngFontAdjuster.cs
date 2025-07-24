using System.Drawing;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace TMPro
{
    public static class EngFontAdjuster
    {
        public static bool IsEngPunctuation(char c)
        {
            return (c >= '\x0020' && c <= '\x002F') || (c >= '\x003A' && c <= '\x0040') || (c >= '\x005B' && c <= '\x0060') || (c >= '\x007B' && c <= '\x007E'
                // " in Deutsch.
                || c == '\x201E');
        }

        //是否为基础字符
        public static bool IsBasicLetter(char c)
        {
            return char.IsLetter(c);
        }


        //是否为装饰字符
        public static bool IsDecoLetter(char c)
        {
            return !IsBasicLetter(c);
        }

        ///获取下一个非装饰字符的索引
        public static int GetAEngCharacter(string s, int startIndex)
        {
            if (s == null || s.Length == 0)
            {
                return startIndex;
            }

            var length = s.Length;
            int i = startIndex;
            var c = s[i];
            bool isDecoOnlyBefore = IsDecoLetter(c);
            if (isDecoOnlyBefore)
            {
                while (i + 1 < length)
                {
                    c = s[i + 1];
                    if (IsBasicLetter(c))
                    {
                        break;
                    }
                    i++;
                }
            }
            i = Mathf.Clamp(i + 1, startIndex, s.Length - 1);
            return i;
        }
    }
}