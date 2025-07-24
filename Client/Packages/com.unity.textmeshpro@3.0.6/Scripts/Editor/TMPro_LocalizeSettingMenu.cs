using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class TMPro_LocalizeSettingMenu
{
    [MenuItem("CONTEXT/TMP_Text/±æµÿªØ/Ã©”Ô", false, -100)]
    static void SwapThaiAdjustState(MenuCommand menuCommand)
    { 
        ThaiFontAdjuster.enable = !ThaiFontAdjuster.enable;
        string state = ThaiFontAdjuster.enable ? "ø™" : "πÿ";
        Debug.LogFormat("«–ªªÃ©”Ô  ≈‰◊¥Ã¨: " + state);
    }
}
