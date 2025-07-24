using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ThaiWordDicTransfer", menuName = "ScriptableObjects/ThaiWordDictionaryTransfer", order = 1)]
public class ThaiWordDictionaryTransfer : ScriptableObject
{
    public TextAsset originThaiDicText;
    public TextAsset targetThaiDicText;

    public void Transfer()
    {
        if (originThaiDicText == null || targetThaiDicText == null)
            return;

        File.WriteAllText(AssetDatabase.GetAssetPath(targetThaiDicText), ThaiFontAdjuster.Adjust(originThaiDicText.text));
        EditorUtility.SetDirty(targetThaiDicText);
    }
}
