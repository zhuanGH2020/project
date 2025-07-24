﻿using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace TMPro.EditorUtilities
{
    public class TMProFontCustomizedCreater
    {
        [MenuItem("Tools/TextMeshPro工具/TextMeshPro 字库生成工具")]
        static void Open()
        {
            EditorWindow.GetWindow<TMProFontCustomizedCreaterWindow>();
        }

        public static CustomizedCreaterSettings GetCustomizedCreaterSettings()
        {
            var settings = new CustomizedCreaterSettings
            {
                //fontFolderPath = "Assets/AB/Font",
                fontFolderPath = "Assets/Res/UI/Fonts",
                //fontMaterialsFolderPath = "Assets/TextMesh Pro/Resources_nopack/Fonts & Materials",
                fontMaterialsFolderPath = "Assets/Res/UI/Fonts",
                //fontBackupPaths = new[]
                //    {"Assets/TextMesh Pro/NewFonts/FZYaSong-B.TTF", "Assets/TextMesh Pro/NewFonts/SYHT.OTF"},
                fontBackupPaths = new[]
                    {"Assets/Res/UI/Fonts"},
                pointSizeSamplingMode = 0,
                pointSize = 22,
                padding = 4,
                packingMode = 0, // Fast
                atlasWidth = 4069,
                atlasHeight = 4096,
                characterSetSelectionMode = 8, // Character List from File
                //characterSequenceFile = "Assets/TextMesh Pro/chinese_3500.txt",
                characterSequenceFile = "Assets/Res/UI/Fonts/AllText.txt",
                //fontStyle = (int)FaceStyles.Normal,
                //fontStyleModifier = 2,
                renderMode = (int) GlyphRenderMode.SDFAA,
                includeFontFeatures = false,
                characterUseFontBackup = new[] {new[] {"。，"}, new[] {"+-"}}
            };
            return settings;
        }

        public struct CustomizedCreaterSettings
        {
            /// <summary>
            /// 字体的目录
            /// </summary>
            public string fontFolderPath;

            /// <summary>
            /// 字体材质的目录
            /// </summary>
            public string fontMaterialsFolderPath;

            /// <summary>
            /// 备用字体路径
            /// </summary>
            public string[] fontBackupPaths;

            /// <summary>
            /// 字体大小模式（0表示自动大小，1表示自定义大小）
            /// </summary>
            public int pointSizeSamplingMode;

            /// <summary>
            /// 字体大小
            /// </summary>
            public int pointSize;

            /// <summary>
            /// 间距
            /// </summary>
            public int padding;

            public int packingMode;
            public int atlasWidth;
            public int atlasHeight;
            public int characterSetSelectionMode;

            public string characterSequenceFile;

            //public int fontStyle;
            //public float fontStyleModifier;
            public int renderMode;
            public bool includeFontFeatures;

            /// <summary>
            /// 某些字符指定使用备用字体，根据字体顺序
            /// </summary>
            public string[][] characterUseFontBackup;
        }

        /// <summary>
        /// 因为图集复用，所以不能用原本的接口，这里根据每个项目不同的命名规则来查找材质
        /// </summary>
        /// <param name="fontAsset"></param>
        /// <returns></returns>
        public static Material[] FindMaterialReferences(TMP_FontAsset fontAsset)
        {
            List<Material> materialList = new List<Material>();
            Material material1 = fontAsset.material;
            materialList.Add(material1);
            string str1 = "t:Material ";
            string str2 = fontAsset.name;
            string str3 = "_";
            string str4 = str2 + str3;
            foreach (string asset in AssetDatabase.FindAssets(str1 + str4))
            {
                // str4后只能接数字，否则不是此字体资产的
                string str5 = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(asset));
                string str6 = str5.Substring(str4.Length);
                int num;
                if (!int.TryParse(str6, out num))
                {
                    continue;
                }

                Material material2 = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(asset));
                if (material2.HasProperty(ShaderUtilities.ID_MainTex))
                    materialList.Add(material2);
            }

            return materialList.ToArray();
        }
    }
}