using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

[CustomEditor(typeof(ThaiWordDictionaryTransfer))]
public class ThaiWordDictionaryTransferEditor : Editor
{
    SerializedProperty m_originThaiDicText;
    SerializedProperty m_targetThaiDicText;

    public void OnEnable()
    {
        if (target == null)
            return;

        m_originThaiDicText = serializedObject.FindProperty("originThaiDicText");
        m_targetThaiDicText = serializedObject.FindProperty("targetThaiDicText");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {

        serializedObject.Update();
        ThaiWordDictionaryTransfer transfer = target as ThaiWordDictionaryTransfer;
        EditorGUILayout.PropertyField(m_originThaiDicText, new GUIContent("原文本"));
        EditorGUILayout.PropertyField(m_targetThaiDicText, new GUIContent("目标文本"));
        if (GUILayout.Button("转换"))
        {
            transfer.Transfer();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
