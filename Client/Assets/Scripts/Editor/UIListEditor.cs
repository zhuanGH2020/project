// Last modified_2: 2024-12-28 10:30:00
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIList))]
public class UIListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        UIList uiList = (UIList)target;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("AddListItem"))
        {
            uiList.AddListItem();
        }
        
        if (GUILayout.Button("RepositionNow"))
        {
            uiList.Reposition();
        }
        
        if (GUILayout.Button("RemoveAll"))
        {
            uiList.RemoveAll();
        }
    }
}
