using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace TMPro
{
    public static class ThaiFontAdjuster
    {
        public static bool enable = false;
        public static bool enableThaiWordTree = true;
        //private static bool enableNarrow = false;

        private static StringBuilder sb = new StringBuilder( 512 );

        public static bool IsThaiString( string s )
        {
            if( s != null )
            {
                var length = s.Length;
                for( var i = 0; i < length; i++ )
                {
                    var c = s[ i ];
                    if(IsThaiChar(c))
                        return true;
                }
            }
            return false;
        }

        public static bool IsThaiChar(char c)
        {
            return (c >= '\x0E00' && c <= '\x0E7F') || (c >= '\xF6FF' && c <= '\xF72C');
        }
        // ========== EXTENDED CHARACTER TABLE ==========
        // F700:     uni0E10.descless    (base.descless)
        // F701~04:  uni0E34~37.left     (upper.left)
        // F705~09:  uni0E48~4C.lowleft  (top.lowleft)
        // F70A~0E:  uni0E48~4C.low      (top.low)
        // F70F:     uni0E0D.descless    (base.descless)
        // F710~12:  uni0E31,4D,47.left  (upper.left)
        // F713~17:  uni0E48~4C.left     (top.left)
        // F718~1A:  uni0E38~3A.low      (lower.low)
        // ==============================================

        public static string Adjust( string s )
        {
            // http://www.bakoma-tex.com/doc/fonts/enc/c90/c90.pdf
            if( s == null || s.Length == 0 )
            {
                return s;
            }

            var length = s.Length;
            sb.EnsureCapacity( length );
            sb.Clear();

            //var sb = new StringBuilder(length);
            for( var i = 0; i < length; i++ )
            {
                var c = s[ i ];

                // [base] ~ [top]
                if( IsTop( c ) && i > 0 )
                {
                    // [base]             [top] -> [base]             [top.low]
                    // [base]     [lower] [top] -> [base]     [lower] [top.low]
                    // [base.asc]         [top] -> [base.asc]         [top.lowleft]
                    // [base.asc] [lower] [top] -> [base.asc] [lower] [top.lowleft]
                    c = (char)( c + ( '\xF713' - '\x0E48' ) );
                    var b = s[ i - 1 ];
                    if( IsLower( b ) && i > 1 )
                        b = s[ i - 2 ];
                    if( IsBase( b ) )
                    {
                        var followingNikhahit = ( i < length - 1 && ( s[ i + 1 ] == '\x0E33' || s[ i + 1 ] == '\x0E4D' ) );
                        if( IsBaseAsc( b ) )
                        {
                            if( followingNikhahit )
                            {
                                //��߸߶ȣ���������Ȧ��
                                // [base.asc] [top] [sara am] -> [base.asc] [nikhahit] [top.left] [sara aa]
                                //c = (char)(c + ('\xF713' - '\x0E48'));
                                sb.Append( '\xF711' );
                                sb.Append( c );
                                if( s[ i + 1 ] == '\x0E33' )
                                    sb.Append( '\x0E32' );
                                i += 1;
                                continue;
                            }
                            else
                            {
                                //Edit: �еȸ߶�
                                //c = (char)(c + ('\xF705' - '\x0E48'));
                                //c = (char)(c + ('\xF713' - '\x0E48'));
                                //continue;
                                //c = (char)(c + ('\xF705' - '\x0E48'));
                            }
                        }
                        else
                        {
                            // Edit: ����̧�ߵ�Ԫ�� 
                            if (followingNikhahit == false)
                            {
                                c = (char)(c - ('\xF713' - '\x0E48'));
                                sb.Append(c);
                                continue;
                            }
                            //c = (char)(c + ('\xF713' - '\x0E48'));
                            //c = (char)(c + ('\xF70A' - '\x0E48'));
                        }
                    }

                    // [base.asc] [upper] [top] -> [base.asc] [upper] [top.left]
                    if( i > 1 && IsUpper( b ))
                    {
                        if( IsBaseAsc( s[ i - 2 ] ) )
                        {
                            c = (char)(c + ('\xF728' - '\xF713'));
                        }
                    } 
                }
                else if( IsUpper( c ) )
                {
                    // [base.asc] [upper] -> [base.asc] [upper-left]
                    //Edit: Case narrow
                    //if( enableNarrow && i < s.Length - 1)
                    //{
                    //    c = (char)( c + ( '\xF701' - '\x0E34' ) );
                    //}
                    if ((i > 0 && IsBaseAsc(s[i - 1])))
                    {
                        switch (c)
                        {
                            case '\x0E31': c = '\xF710'; break;
                            case '\x0E34': c = '\xF701'; break;
                            case '\x0E35': c = '\xF702'; break;
                            case '\x0E36': c = '\xF703'; break;
                            case '\x0E37': c = '\xF704'; break;
                            case '\x0E4D': c = '\xF711'; break;
                            case '\x0E47': c = '\xF712'; break;
                        }
                    }
                }
                // [base.desc] [lower] -> [base.desc] [lower.low]
                else if( IsLower( c ) && i > 0 && IsBaseDesc( s[ i - 1 ] ) )
                {
                    c = (char)( c + ( '\xF718' - '\x0E38' ) );
                }
                // [YO YING] [lower] -> [YO YING w/o lower] [lower]
                else if( c == '\x0E0D' && i < length - 1 && IsLower( s[ i + 1 ] ) )
                {
                    c = '\xF70F';
                }
                // [THO THAN] [lower] -> [THO THAN w/o lower] [lower]
                else if( c == '\x0E10' && i < length - 1 && IsLower( s[ i + 1 ] ) )
                {
                    c = '\xF700';
                }
                //RULE: liga, no \x0E26  
                else if( c == '\x0E24' && i < length - 1 && s[ i + 1 ] == '\x0E45' )
                {
                    c = '\xF71F';
                    i += 1;
                }
                //RULE: aalt-SinglrSubsitution
                else if(/*enableNarrow && */IsSpecialBase( c ) /*&& i < length - 1 && IsUpper(s[i + 1])*/)
                {
                    bool isnextUpper = (i < length - 1) && (IsUpper(s[i + 1])||IsTop(s[i + 1]));
                    bool isnextLower = (i < length - 1) && IsLower(s[i + 1]);
                    if (isnextUpper && c == '\x0E2C')
                    {
                        c = '\xF71B';
                    }
                    else if (isnextLower)
                    {
                        switch (c)
                        {
                            case '\x0E0D': //have down follow and nerrow
                                    c = '\xF70F';
                                break;
                            case '\x0E0E'://have down follow and nerrow
                                    c = '\xF71C';
                                break;
                            case '\x0E10'://have down follow and nerrow
                                c = '\xF700';
                                break;
                            case '\x0E0F'://have down follow and nerrow
                                c = '\xF71E';
                                break;
                            case '\x0E24'://have down follow and nerrow
                                c = '\xF71D';
                                break;
                        }
                    }
                }
                sb.Append( c );
            }
            return sb.ToString( 0, sb.Length );
        }

        public static int GetAThaiCharacter( string s, int startIndex)
        {
            if (s == null || s.Length == 0)
            {
                return startIndex;
            }

            var length = s.Length;
            int i = startIndex;
            var c = s[i];
            if (IsBase(c) || IsSpecialBase(c) || IsBaseDesc(c) || IsBaseAsc(c))
            {
                while (i + 1 < length)
                {
                    c = s[i + 1];
                    if (IsBase(c) || IsSpecialBase(c) || IsBaseDesc(c) || IsBaseAsc(c) || !IsThaiChar(c))
                    { 
                        break;
                    }
                    i++;
                }
            }
            i = Mathf.Clamp(i + 1, startIndex, s.Length - 1);
            return i;
        }

        public static bool IsBaseOrNotThai(char c)
        {
            return !IsThaiChar(c) ||IsBase(c) || IsSpecialBase(c) || IsBaseDesc(c) || IsBaseAsc(c) || IsPictureChar(c);
        }

        private static bool IsSpecialBase( char c )
        {
            //lochula-thai  => lochulathai.short
            //yoying-thai   => yoyingthai.less
            //dochada-thai  => dochadathai.short
            //thothan-thai  => thothanthai.less
            //topatak-thai  => topatakthai.short
            //ru-thai       => ruthai.short
            //To   c == '\xF71B' || c == '\xF70F' || c == '\xF71C' || c == '\xF700' || c == '\xF71E' || c == '\xF71D'        
            return c == '\x0E2C' || c == '\x0E0D' || c == '\x0E0E' || c == '\x0E10' || c == '\x0E0F' || c == '\x0E24';

        }
        private static bool IsBase( char c )
        {
            return ( c >= '\x0E01' && c <= '\x0E30') || c == '\x0E32' || (c >= '\x0E3F' && c <= '\x0E46');
        }

        private static bool IsBaseDesc( char c )
        {
            return c == '\x0E0E' || c == '\x0E0F';
        }

        private static bool IsBaseAsc( char c )
        {
            return c == '\x0E1B' || c == '\x0E1D' || c == '\x0E1F' || c == '\x0E2C';
        }

        private static bool IsTop( char c )
        {
            // Tone Mark, THANTHAKHAT
            return c >= '\x0E48' && c <= '\x0E4C';
        }

        private static bool IsLower( char c )
        {
            // SARA U, SARA UU, PHINTHU
            return c >= '\x0E38' && c <= '\x0E3A';
        }

        private static bool IsUpper( char c )
        {
            return c == '\x0E31' || c == '\x0E34' || c == '\x0E35' || c == '\x0E36' ||
                   c == '\x0E37' || c == '\x0E47' || c == '\x0E4D';
        }

        private static bool IsPictureChar(char c)
        {
            // Picture Icon char
            return c == '\xF72D';
        }

        public static string GetAFullThaiCharacter(string s, int start)
        {

            return " ";
        }
    }
}