using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

#pragma warning disable 0649 // Disabled warnings related to serialized fields not assigned in this script but used in the editor.

namespace TMPro
{
    /// <summary>
    /// Scaling options for the sprites
    /// </summary>
    //public enum SpriteRelativeScaling
    //{
    //    RelativeToPrimary   = 0x1,
    //    RelativeToCurrent   = 0x2,
    //}

    [System.Serializable][ExcludeFromPresetAttribute]
    public class TMP_Settings : ScriptableObject
    {
        private static TMP_Settings s_Instance;

        public static string curLanguage
        {
            get { return m_curLanguage; }
            set 
            {
                if ( m_curLanguage == null || m_curLanguage != value)
                {
                    m_curLanguage = value;
                    //ThaiFontAdjuster.enable = m_curLanguage == "th";
                }
            }
        }
        private static string m_curLanguage;
        /// <summary>
        /// Returns the release version of the product.
        /// </summary>
        public static string version
        {
            get { return "1.4.0"; }
        }

        /// <summary>
        /// Controls if Word Wrapping will be enabled on newly created text objects by default.
        /// </summary>
        public static bool enableWordWrapping
        {
            get { return instance.m_enableWordWrapping; }
        }
        [SerializeField]
        private bool m_enableWordWrapping;

        /// <summary>
        /// Controls if Kerning is enabled on newly created text objects by default.
        /// </summary>
        public static bool enableKerning
        {
            get { return instance.m_enableKerning; }
        }
        [SerializeField]
        private bool m_enableKerning;

        /// <summary>
        /// Controls if Extra Padding is enabled on newly created text objects by default.
        /// </summary>
        public static bool enableExtraPadding
        {
            get { return instance.m_enableExtraPadding; }
        }
        [SerializeField]
        private bool m_enableExtraPadding;

        /// <summary>
        /// Controls if TintAllSprites is enabled on newly created text objects by default.
        /// </summary>
        public static bool enableTintAllSprites
        {
            get { return instance.m_enableTintAllSprites; }
        }
        [SerializeField]
        private bool m_enableTintAllSprites;

        /// <summary>
        /// Controls if Escape Characters will be parsed in the Text Input Box on newly created text objects.
        /// </summary>
        public static bool enableParseEscapeCharacters
        {
            get { return instance.m_enableParseEscapeCharacters; }
        }
        [SerializeField]
        private bool m_enableParseEscapeCharacters;

        /// <summary>
        /// Controls if Raycast Target is enabled by default on newly created text objects.
        /// </summary>
        public static bool enableRaycastTarget
        {
            get { return instance.m_EnableRaycastTarget; }
        }
        [SerializeField]
        private bool m_EnableRaycastTarget = true;

        /// <summary>
        /// Determines if OpenType Font Features should be retrieved at runtime from the source font file.
        /// </summary>
        public static bool getFontFeaturesAtRuntime
        {
            get { return instance.m_GetFontFeaturesAtRuntime; }
        }
        [SerializeField]
        private bool m_GetFontFeaturesAtRuntime = true;

        /// <summary>
        /// The character that will be used as a replacement for missing glyphs in a font asset.
        /// </summary>
        public static int missingGlyphCharacter
        {
            get { return instance.m_missingGlyphCharacter; }
            set { instance.m_missingGlyphCharacter = value; }
        }
        [SerializeField]
        private int m_missingGlyphCharacter;

        /// <summary>
        /// Controls the display of warning message in the console.
        /// </summary>
        public static bool warningsDisabled
        {
            get { return instance.m_warningsDisabled; }
        }
        [SerializeField]
        private bool m_warningsDisabled;

        /// <summary>
        /// Returns the Default Font Asset to be used by newly created text objects.
        /// </summary>
        public static TMP_FontAsset defaultFontAsset
        {
            get { return instance.m_defaultFontAsset; }
        }
        [SerializeField]
        private TMP_FontAsset m_defaultFontAsset;

        /// <summary>
        /// The relative path to a Resources folder in the project.
        /// </summary>
        public static string defaultFontAssetPath
        {
            get { return instance.m_defaultFontAssetPath; }
        }
        [SerializeField]
        private string m_defaultFontAssetPath;

        /// <summary>
        /// Returns the Default OS Font Asset to fallback.
        /// </summary>
        public static TMP_FontAsset defaultOSFontAsset
        {
            get { return instance.m_defaultOSFontAsset; }
        }
        [SerializeField]
        private TMP_FontAsset m_defaultOSFontAsset;

        /// <summary>
        /// The Default Point Size of newly created text objects.
        /// </summary>
        public static float defaultFontSize
        {
            get { return instance.m_defaultFontSize; }
        }
        [SerializeField]
        private float m_defaultFontSize;

        /// <summary>
        /// The multiplier used to computer the default Min point size when Text Auto Sizing is used.
        /// </summary>
        public static float defaultTextAutoSizingMinRatio
        {
            get { return instance.m_defaultAutoSizeMinRatio; }
        }
        [SerializeField]
        private float m_defaultAutoSizeMinRatio;

        /// <summary>
        /// The multiplier used to computer the default Max point size when Text Auto Sizing is used.
        /// </summary>
        public static float defaultTextAutoSizingMaxRatio
        {
            get { return instance.m_defaultAutoSizeMaxRatio; }
        }
        [SerializeField]
        private float m_defaultAutoSizeMaxRatio;

        /// <summary>
        /// The Default Size of the Text Container of a TextMeshPro object.
        /// </summary>
        public static Vector2 defaultTextMeshProTextContainerSize
        {
            get { return instance.m_defaultTextMeshProTextContainerSize; }
        }
        [SerializeField]
        private Vector2 m_defaultTextMeshProTextContainerSize;

        /// <summary>
        /// The Default Width of the Text Container of a TextMeshProUI object.
        /// </summary>
        public static Vector2 defaultTextMeshProUITextContainerSize
        {
            get { return instance.m_defaultTextMeshProUITextContainerSize; }
        }
        [SerializeField]
        private Vector2 m_defaultTextMeshProUITextContainerSize;

        /// <summary>
        /// Set the size of the text container of newly created text objects to match the size of the text.
        /// </summary>
        public static bool autoSizeTextContainer
        {
            get { return instance.m_autoSizeTextContainer; }
        }
        [SerializeField]
        private bool m_autoSizeTextContainer;

        /// <summary>
        /// Disables InternalUpdate() calls when true. This can improve performance when the scale of the text object is static.
        /// </summary>
        public static bool isTextObjectScaleStatic
        {
            get { return instance.m_IsTextObjectScaleStatic; }
            set { instance.m_IsTextObjectScaleStatic = value; }
        }
        [SerializeField]
        private bool m_IsTextObjectScaleStatic;


        /// <summary>
        /// Returns the list of Fallback Fonts defined in the TMP Settings file.
        /// </summary>
        public static List<TMP_FontAsset> fallbackFontAssets
        {
            get { return instance.m_fallbackFontAssets; }
        }
        [SerializeField]
        private List<TMP_FontAsset> m_fallbackFontAssets;

        /// <summary>
        /// Is fallback OS font list inited.
        /// </summary>
        private static bool isFallbackOSFontsInited;

        /// <summary>
        /// OS fallbck font file paths.
        /// </summary>
        public static List<string> fallbackFontPaths
        {
            get
            { 
                if (m_fallbackFontPaths == null || m_fallbackFontFaceIndex == null)
                { 
                   InitFontOsList();
                }
                return m_fallbackFontPaths;
            }
            //set
            //{ 
            //    instance.m_fallbackFontPaths = value;
            //}
        }
        private static List<string> m_fallbackFontPaths = null;

        public static List<int> fallbackFontFaceIndex
        {
            get
            {
                return m_fallbackFontFaceIndex;
            }
        }
        private static List<int> m_fallbackFontFaceIndex = null;
        
        /// <summary>
        /// OS fallback fonts name on all platform.
        /// </summary>
        public static List<string> fallbackOSFontsCommon
        {
            get { return instance.m_fallbackOSFontsCommon; }
        }
        [SerializeField]
        private List<string> m_fallbackOSFontsCommon;
        
        /// <summary>
        /// OS fallback fonts name on Android.
        /// </summary>
        public static List<string> fallbackOSFontsAndroid
        {
            get { return instance.m_fallbackOSFontsAndroid; }
        }
        [SerializeField]
        private List<string> m_fallbackOSFontsAndroid;

        /// <summary>
        /// OS fallback fonts name on Windows
        /// </summary>
        public static List<string> fallbackOSFontsWindows
        {
            get { return instance.m_fallbackOSFontsWindows; }
        }
        [SerializeField]
        private List<string> m_fallbackOSFontsWindows;

        /// <summary>
        /// OS fallback fonts name on IOS.
        /// </summary>
        public static List<string> fallbackOSFontsIOS
        {
            get { return instance.m_fallbackOSFontsIOS; }
        }
        [SerializeField]
        private List<string> m_fallbackOSFontsIOS;

        /// <summary>
        /// Controls whether or not TMP will create a matching material preset or use the default material of the fallback font asset.
        /// </summary>
        public static bool matchMaterialPreset
        {
            get { return instance.m_matchMaterialPreset; }
        }
        [SerializeField]
        private bool m_matchMaterialPreset;

        /// <summary>
        /// The Default Sprite Asset to be used by default.
        /// </summary>
        public static TMP_SpriteAsset defaultSpriteAsset
        {
            get { return instance.m_defaultSpriteAsset; }
        }
        [SerializeField]
        private TMP_SpriteAsset m_defaultSpriteAsset;

        /// <summary>
        /// The relative path to a Resources folder in the project.
        /// </summary>
        public static string defaultSpriteAssetPath
        {
            get { return instance.m_defaultSpriteAssetPath; }
        }
        [SerializeField]
        private string m_defaultSpriteAssetPath;

        /// <summary>
        /// Determines if Emoji support is enabled in the Input Field TouchScreenKeyboard.
        /// </summary>
        public static bool enableEmojiSupport
        {
            get { return instance.m_enableEmojiSupport; }
            set { instance.m_enableEmojiSupport = value; }
        }
        [SerializeField]
        private bool m_enableEmojiSupport;

        /// <summary>
        /// The unicode value of the sprite that will be used when the requested sprite is missing from the sprite asset and potential fallbacks.
        /// </summary>
        public static uint missingCharacterSpriteUnicode
        {
            get { return instance.m_MissingCharacterSpriteUnicode; }
            set { instance.m_MissingCharacterSpriteUnicode = value; }
        }
        [SerializeField]
        private uint m_MissingCharacterSpriteUnicode;

        /// <summary>
        /// Determines if sprites will be scaled relative to the primary font asset assigned to the text object or relative to the current font asset.
        /// </summary>
        //public static SpriteRelativeScaling spriteRelativeScaling
        //{
        //    get { return instance.m_SpriteRelativeScaling; }
        //    set { instance.m_SpriteRelativeScaling = value; }
        //}
        //[SerializeField]
        //private SpriteRelativeScaling m_SpriteRelativeScaling = SpriteRelativeScaling.RelativeToCurrent;

        /// <summary>
        /// The relative path to a Resources folder in the project that contains Color Gradient Presets.
        /// </summary>
        public static string defaultColorGradientPresetsPath
        {
            get { return instance.m_defaultColorGradientPresetsPath; }
        }
        [SerializeField]
        private string m_defaultColorGradientPresetsPath;

        /// <summary>
        /// The Default Style Sheet used by the text objects.
        /// </summary>
        public static TMP_StyleSheet defaultStyleSheet
        {
            get { return instance.m_defaultStyleSheet; }
        }
        [SerializeField]
        private TMP_StyleSheet m_defaultStyleSheet;

        /// <summary>
        /// The relative path to a Resources folder in the project that contains the TMP Style Sheets.
        /// </summary>
        public static string styleSheetsResourcePath
        {
            get { return instance.m_StyleSheetsResourcePath; }
        }
        [SerializeField]
        private string m_StyleSheetsResourcePath;

        /// <summary>
        /// Text file that contains the leading characters used for line breaking for Asian languages.
        /// </summary>
        public static TextAsset leadingCharacters
        {
            get { return instance.m_leadingCharacters; }
        }
        [SerializeField]
        private TextAsset m_leadingCharacters;

        /// <summary>
        /// Text file that contains the following characters used for line breaking for Asian languages.
        /// </summary>
        public static TextAsset followingCharacters
        {
            get { return instance.m_followingCharacters; }
        }
        [SerializeField]
        private TextAsset m_followingCharacters;

        /// <summary>
        /// Text file that contains the following characters used for line breaking for Asian languages.
        /// </summary>  
        public static TextAsset thaiWordDictionary
        {
            get { return instance.m_thaiWordDictionary; }
        }
        [SerializeField]
        private TextAsset m_thaiWordDictionary;

        /// <summary>
        ///
        /// </summary>
        public static LineBreakingTable linebreakingRules
        {
            get
            {
                if (instance.m_linebreakingRules == null)
                    LoadLinebreakingRules();

                return instance.m_linebreakingRules;
            }
        }
        [SerializeField]
        private LineBreakingTable m_linebreakingRules;

        /// <summary>
        /// Thai word dicitionary. Use for word breaking in GenerateTextMesh
        /// </summary>
        public static TextAsset thaiWordsDictionary
        {
            get
            {
                return instance.m_thaiWordsDictionary;
            }
        }
        [SerializeField]
        private TextAsset m_thaiWordsDictionary;

        /// <summary>
        /// Thai Word Tree
        /// </summary>
        public static WordTree thaiWordTree
        {
            get
            {
                if( instance.m_thaiWordsDictionary == null )
                {
                    Debug.Log( "[TATest_TMP] Thai words dictionary not set. Thai word tree not inited. Return" );
                    return null;
                }
                if( instance.m_thaiWordTree == null )
                {
                    instance.m_thaiWordTree = new WordTree();
                    instance.m_thaiWordTree.GenTreeWithWordAndSeparate(instance.m_thaiWordsDictionary.text);
                }
                return instance.m_thaiWordTree;
            }
        }
        private WordTree m_thaiWordTree;
        // TODO : Potential new feature to explore where multiple font assets share the same atlas texture.
        //internal static TMP_DynamicAtlasTextureGroup managedAtlasTextures
        //{
        //    get
        //    {
        //        if (instance.m_DynamicAtlasTextureGroup == null)
        //        {
        //            instance.m_DynamicAtlasTextureGroup = TMP_DynamicAtlasTextureGroup.CreateDynamicAtlasTextureGroup();
        //        }

        //        return instance.m_DynamicAtlasTextureGroup;
        //    }
        //}
        //[SerializeField]
        //private TMP_DynamicAtlasTextureGroup m_DynamicAtlasTextureGroup;

        /// <summary>
        /// Determines if Modern or Traditional line breaking rules should be used for Korean text.
        /// </summary>
        public static bool useModernHangulLineBreakingRules
        {
            get { return instance.m_UseModernHangulLineBreakingRules; }
            set { instance.m_UseModernHangulLineBreakingRules = value; }
        }
        [SerializeField]
        private bool m_UseModernHangulLineBreakingRules;

        /// <summary>
        /// Get a singleton instance of the settings class.
        /// </summary>
        public static TMP_Settings instance
        {
            get
            {
                if (TMP_Settings.s_Instance == null)
                {
                    TMP_Settings.s_Instance = Resources.Load<TMP_Settings>("TMP Settings");

                    #if UNITY_EDITOR
                    // Make sure TextMesh Pro UPM packages resources have been added to the user project
                    if (TMP_Settings.s_Instance == null)
                    {
                        // Open TMP Resources Importer
                        TMP_PackageResourceImporterWindow.ShowPackageImporterWindow();
                    }
                    #endif
                }

                return TMP_Settings.s_Instance;
            }
        }


        /// <summary>
        /// Static Function to load the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_Settings LoadDefaultSettings()
        {
            if (s_Instance == null)
            {
                // Load settings from TMP_Settings file
                TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
                if (settings != null)
                    s_Instance = settings;
            }

            return s_Instance;
        }


        /// <summary>
        /// Returns the Sprite Asset defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_Settings GetSettings()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance;
        }


        /// <summary>
        /// Returns the Font Asset defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_FontAsset GetFontAsset()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance.m_defaultFontAsset;
        }


        /// <summary>
        /// Returns the Sprite Asset defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_SpriteAsset GetSpriteAsset()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance.m_defaultSpriteAsset;
        }


        /// <summary>
        /// Returns the Style Sheet defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_StyleSheet GetStyleSheet()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance.m_defaultStyleSheet;
        }


        public static void LoadLinebreakingRules()
        {
            //Debug.Log("Loading Line Breaking Rules for Asian Languages.");

            if (TMP_Settings.instance == null) return;

            if (s_Instance.m_linebreakingRules == null)
                s_Instance.m_linebreakingRules = new LineBreakingTable();

            s_Instance.m_linebreakingRules.leadingCharacters = GetCharacters(s_Instance.m_leadingCharacters);
            s_Instance.m_linebreakingRules.followingCharacters = GetCharacters(s_Instance.m_followingCharacters);
        }


        /// <summary>
        ///  Get the characters from the line breaking files
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static Dictionary<int, char> GetCharacters(TextAsset file)
        {
            Dictionary<int, char> dict = new Dictionary<int, char>();
            string text = file.text;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                // Check to make sure we don't include duplicates
                if (dict.ContainsKey((int)c) == false)
                {
                    dict.Add((int)c, c);
                    //Debug.Log("Adding [" + (int)c + "] to dictionary.");
                }
                //else
                //    Debug.Log("Character [" + text[i] + "] is a duplicate.");
            }

            return dict;
        }

        /// <summary>
        ///  初始化系统Fallback字路径列表.Add by UITC.
        /// </summary>
        /// <returns></returns>
        public static void InitFontOsList()
        {
            List<string> platformFontFallbackList = new List<string>();
            List<int> fontstates = new List<int>();

#if UNITY_ANDROID && !UNITY_EDITOR
            platformFontFallbackList = fallbackOSFontsAndroid;
#elif UNITY_IOS && !UNITY_EDITOR
            platformFontFallbackList = fallbackOSFontsIOS;
#else
            platformFontFallbackList = fallbackOSFontsWindows;
#endif
            var fallbackList = new List<string>();
            fallbackList.AddRange(platformFontFallbackList);
            fallbackList.AddRange(fallbackOSFontsCommon);
            fallbackList= fallbackList.Distinct().ToList();

            fontstates = new List<int>(Enumerable.Repeat(0, fallbackList.Count).ToArray());
            
            var fallbackPaths = new List<string>();
            var fallbackFaceIndex = new List<int>();

            //Debug.Log($"TMP_Setting fallbackList count: {fallbackList.Count}");
            string[] osfontpaths = Font.GetPathsToOSFonts();
            //Debug.Log($"TMP_Setting osfontpaths count: {osfontpaths.Length}");
            for (int i = 0; i < fallbackList.Count; i++)
            {
                var result = fallbackList[i].Split(",");
                for (int j = 0; j < osfontpaths.Length; j++)
                {
                    string path = osfontpaths[j];
                    int lastIndex = path.LastIndexOf('/');
                    int lastDot = path.LastIndexOf('.');
                    if (lastDot <= lastIndex)
                        lastDot = path.Length;
                    string fontname = path.Substring(lastIndex + 1, lastDot - (lastIndex + 1)).ToLower();
                    //Debug.Log($"TMP_Setting find os font: {fontname}");
                    if (result[0] == fontname && fontstates[i] == 0)
                    {
                        fallbackPaths.Add(osfontpaths[j]);

                        var fIndex = 0;
                        if (result.Length > 1 && Int32.TryParse(result[1], out int fid))
                            fIndex = fid;
                        fallbackFaceIndex.Add(fIndex);

                        fontstates[i] = 1;
                        //Debug.Log($"TMP_Setting add os font: {fontname}");
                    }
                }
            }
#if false
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < fallbackPaths.Count; i++)
                sb.Append(i + " : " + fallbackPaths[i] + '\n');
            Debug.LogFormat("[TATest][TMPOSFallback] Add Os fallback fonts: \n" + sb.ToString());
#endif
            m_fallbackFontPaths =  fallbackPaths;
            m_fallbackFontFaceIndex = fallbackFaceIndex;
        }

        public static void ClearOSFallback()
        {
            m_fallbackFontPaths = null;
            if (defaultOSFontAsset != null)
            { 
                defaultOSFontAsset.ClearFontAssetData(true);
            }
        }
		
		public static void ClearThaiWordTree()
        {
            instance.m_thaiWordTree = null;
        }

        public class LineBreakingTable
        {
            public Dictionary<int, char> leadingCharacters;
            public Dictionary<int, char> followingCharacters;
        }
    }
}
