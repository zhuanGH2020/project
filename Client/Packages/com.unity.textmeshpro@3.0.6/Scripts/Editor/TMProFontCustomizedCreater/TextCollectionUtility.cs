using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
    public class TextCollectionUtility
    {
        public static string configPath = System.Environment.CurrentDirectory + "/CsvTable";

        [MenuItem("Tools/TextMeshPro工具/Collect all text in one txt")]
        public static void CollectAllTextInOneTxt()
        {
            string prefabTextCollection = CollectAllTextWithStringInProject();
            string allText = StrCutRepeat(prefabTextCollection + FindTextAssets());
            FileStream newFileStream = new FileStream(Application.dataPath + "/Res/UI/Fonts/AllText.txt", FileMode.Create, FileAccess.Write);
            using (StreamWriter sw = new StreamWriter(newFileStream, new UTF8Encoding(false)))
            {
                foreach (var s in allText)
                {
                    sw.Write(s);
                }
            }
            AssetDatabase.Refresh();
            newFileStream.Close();
        }

        [MenuItem("Tools/TextMeshPro工具/Auto Set All Text")]
        public static void AutoSetText()
        {

            TMProFontCustomizedCreaterWindow tempWindow = TMProFontCustomizedCreaterWindow.CreateWindow<TMProFontCustomizedCreaterWindow>();
            
            tempWindow.Generate();
        }

        public static string CollectAllTextWithStringInProject()
        {
            string[] allPath = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });
            StringBuilder textCollection = new StringBuilder(100);
            for (int i = 0; i < allPath.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(allPath[i]);
                var obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (obj != null)
                {
                    var texts = obj.GetComponentsInChildren<TMP_Text>();
                    foreach (var text in texts)
                    {
                        textCollection.Append(StrCutRepeat(text.text));
                    }
                }
            }
            return StrCutRepeat(textCollection.ToString());
            //return textCollection.ToString();
        }

        public static List<string> textAssetList = new List<string>();
        private static string FindTextAssets()
        {
            StringBuilder textCollection = new StringBuilder(100);
            if (Directory.Exists(configPath))
            {
                DirectoryInfo info = new DirectoryInfo(configPath);
                FileInfo[] files = info.GetFiles("*", SearchOption.AllDirectories);

                for (int i = 0; i < files.Length; i++)
                {
                    //去除meat文件
                    if (!files[i].Name.EndsWith(".csv.meta"))
                    {
                        var str = File.ReadAllText(files[i].ToString(), new UTF8Encoding(false));
                        Debug.Log(str);
                        textCollection.Append(StrCutRepeat(str));
                    }
                }
            }
            return StrCutRepeat(textCollection.ToString());
        }

        private void ClearTextAsset(string path)
        {
            if (File.Exists(path))
            {
                File.WriteAllText(path, "X");
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 存储字符串进txt
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="textContent"></param>
        public static void SaveStringInFile(string filePath, string textContent)
        {
            using (StreamWriter sw = new StreamWriter(Application.dataPath + filePath))
            {
                foreach (var s in textContent)
                {
                    sw.Write(s);
                }
            }
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 去重复
        /// </summary>
        /// <param name="oriStr"></param>
        /// <returns></returns>
        private static string StrCutRepeat(string oriStr)
        {
            return string.Join("", oriStr.ToArray().Distinct().ToArray()).Replace("\n", "").Replace(" ", "");
        }


        public static void OpenFontCreatorWindow()
        {
            EditorWindow createWindow = EditorWindow.CreateWindow<TMPro_FontAssetCreatorWindow>();
            TMPro_FontAssetCreatorWindow.ShowFontAtlasCreatorWindow();

        }

    }
}
