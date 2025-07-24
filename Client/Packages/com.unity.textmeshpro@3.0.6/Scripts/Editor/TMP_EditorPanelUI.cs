using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace TMPro.EditorUtilities
{

    [CustomEditor(typeof(TextMeshProUGUI), true), CanEditMultipleObjects]
    public class TMP_EditorPanelUI : TMP_BaseEditorPanel
    {
        static readonly GUIContent k_RaycastTargetLabel = new GUIContent("Raycast Target", "Whether the text blocks raycasts from the Graphic Raycaster.");
        static readonly GUIContent k_MaskableLabel = new GUIContent("Maskable", "Determines if the text object will be affected by UI Mask.");
        static readonly GUIContent k_IsASimpleJumpWord = new GUIContent("JumpWord", "If is a jump word, generate once and never update mesh later.");
        static readonly GUIContent k_UseSpriteOnly = new GUIContent("Use Sprite Only", "只使用Sprite材质.开启后SpriteMesh生成在主节点上,不再产生SubMeshUI.");
        static readonly GUIContent k_DisableCharacterSpacingInThai = new GUIContent("Disable char spacing in thai", "泰语时禁用character spacing.");
        static readonly GUIContent k_UseItalicOverride = new GUIContent("Use Italic Override", "使用斜体字角度Ovrride. Add by UITC.");
        static readonly GUIContent k_ItalicOverrideValue = new GUIContent("Italic Override Value", "斜体字角度Ovrride. Add by UITC.");
		static readonly GUIContent k_UseOSFontFallback = new GUIContent("Use OS Font Fallback", "使用OS字体作为Fallback. Add by UITC.");
        static readonly GUIContent k_ForceAdjustAllLanguage = new GUIContent("All Language Adjust", "其他语言下识别泰语强制转化. Add by UITC.");

        SerializedProperty m_RaycastTargetProp;
        private SerializedProperty m_MaskableProp;
        SerializedProperty m_IsASimpleJumpWord;
        SerializedProperty m_UseSpriteOnly;
        SerializedProperty m_DisableCharacterSpacingInThai;
        SerializedProperty m_UseItalicOverride;
        SerializedProperty m_ItalicOverrideValue;
		SerializedProperty m_UseOSFallback;
        SerializedProperty m_ForceAdjustAllLanguage;
        SerializedProperty m_overrideFontAssetIndex;
        SerializedProperty m_overrideFontAssets;


        protected override void OnEnable()
        {
            base.OnEnable();
            m_IsASimpleJumpWord = serializedObject.FindProperty("isASimpleJumpWord");
            m_UseSpriteOnly = serializedObject.FindProperty("m_useSpriteOnly");
            m_DisableCharacterSpacingInThai = serializedObject.FindProperty("m_disableCharacterSpacingInThai");
            m_RaycastTargetProp = serializedObject.FindProperty( "m_RaycastTarget" );
            m_MaskableProp = serializedObject.FindProperty( "m_Maskable" );
            m_UseItalicOverride = serializedObject.FindProperty("m_uesItalicOverride");
            m_ItalicOverrideValue = serializedObject.FindProperty("m_italicOverrideValue");
			m_UseOSFallback = serializedObject.FindProperty("m_useOSFontFallback");
            m_ForceAdjustAllLanguage = serializedObject.FindProperty("m_forceAdjustAllLanguage");
            m_overrideFontAssetIndex = serializedObject.FindProperty("m_overrideFontAssetIndex");
            m_overrideFontAssets = serializedObject.FindProperty("m_overrideFontAssets");
        }

        protected override void DrawExtraSettings()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 24);

            if (GUI.Button(rect, new GUIContent("<b>Extra Settings</b>"), TMP_UIStyleManager.sectionHeader))
                Foldout.extraSettings = !Foldout.extraSettings;

            GUI.Label(rect, (Foldout.extraSettings ? k_UiStateLabel[0] : k_UiStateLabel[1]), TMP_UIStyleManager.rightLabel);
            if (Foldout.extraSettings)
            {
                //EditorGUI.indentLevel += 1;

                DrawMargins();

                DrawGeometrySorting();

                DrawIsTextObjectScaleStatic();

                DrawRichText();

                DrawRaycastTarget();

                DrawMaskable();

                DrawParsing();

                DrawSpriteAsset();

                DrawStyleSheet();

                DrawKerning();

                DrawPadding();

                //DrawAutoAdjust();

                DrawJumpWord();

                DrawUseSprite();

                DrawDisableThaiSpacing();

                UseItalicOverride();

                DrawForceAdjustAllLanguage();

                DrawOverrideFontAssets();
                //DrawUseOSFallback(); 
                //EditorGUI.indentLevel -= 1;
            }
        }

        protected void DrawRaycastTarget()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_RaycastTargetProp, k_RaycastTargetLabel);
            if (EditorGUI.EndChangeCheck())
            {
                // Change needs to propagate to the child sub objects.
                Graphic[] graphicComponents = m_TextComponent.GetComponentsInChildren<Graphic>();
                for (int i = 1; i < graphicComponents.Length; i++)
                    graphicComponents[i].raycastTarget = m_RaycastTargetProp.boolValue;

                m_HavePropertiesChanged = true;
            }
        }

        protected void DrawMaskable()
        {
            if (m_MaskableProp == null)
                return;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_MaskableProp, k_MaskableLabel);
            if (EditorGUI.EndChangeCheck())
            {
                m_TextComponent.maskable = m_MaskableProp.boolValue;

                // Change needs to propagate to the child sub objects.
                MaskableGraphic[] maskableGraphics = m_TextComponent.GetComponentsInChildren<MaskableGraphic>();
                for (int i = 1; i < maskableGraphics.Length; i++)
                    maskableGraphics[i].maskable = m_MaskableProp.boolValue;

                m_HavePropertiesChanged = true;
            }
        }        
        
        protected void DrawJumpWord()
        {
            if (m_IsASimpleJumpWord == null)
                return;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_IsASimpleJumpWord, k_IsASimpleJumpWord);
            if (EditorGUI.EndChangeCheck())
            {
                m_TextComponent.isASimpleJumpWord = m_IsASimpleJumpWord.boolValue;
                m_HavePropertiesChanged = true;
            }
        }

        protected void DrawUseSprite()
        {
            if (m_UseSpriteOnly == null)
                return;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_UseSpriteOnly, k_UseSpriteOnly);
            if (EditorGUI.EndChangeCheck())
            {
                m_TextComponent.useSpriteOnly = m_UseSpriteOnly.boolValue;
                m_HavePropertiesChanged = true;
            }
        }
        protected void DrawDisableThaiSpacing()
        {
            if (m_DisableCharacterSpacingInThai == null)
                return;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_DisableCharacterSpacingInThai, k_DisableCharacterSpacingInThai);
            if (EditorGUI.EndChangeCheck())
            {
                m_TextComponent.disableCharacterSpacingInThai = m_DisableCharacterSpacingInThai.boolValue;
                m_HavePropertiesChanged = true;
            }
        }

        protected void DrawOverrideFontAssets()
        {
            if (m_overrideFontAssetIndex != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_overrideFontAssetIndex);
                if (EditorGUI.EndChangeCheck())
                {
                    m_TextComponent.overrideFontAssetIndex = m_overrideFontAssetIndex.intValue;
                    m_FontAssetProp.objectReferenceValue = m_TextComponent.font;
                    m_HavePropertiesChanged = true;
                }
            }

            if (m_overrideFontAssets != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_overrideFontAssets);
                if (EditorGUI.EndChangeCheck())
                {
                    m_HavePropertiesChanged = true;
                }
            }
        }

        protected void UseItalicOverride()
        {
            if (m_UseItalicOverride == null)
                return;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_UseItalicOverride, k_UseItalicOverride);
            if (m_UseItalicOverride.boolValue)
            {
                EditorGUILayout.PropertyField(m_ItalicOverrideValue, k_ItalicOverrideValue);
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_TextComponent.uesItlicOverride = m_UseItalicOverride.boolValue;
                m_TextComponent.italicOverrideValue = m_ItalicOverrideValue.intValue;
                m_HavePropertiesChanged = true;
            }
        }

        protected void DrawForceAdjustAllLanguage()
        {
            if (m_ForceAdjustAllLanguage == null)
                return;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_ForceAdjustAllLanguage, k_ForceAdjustAllLanguage);
            if (EditorGUI.EndChangeCheck())
            {
                m_TextComponent.forceAdjustAllLanguage = m_ForceAdjustAllLanguage.boolValue;
                m_HavePropertiesChanged = true;
            }
        }

        //protected void DrawUseOSFallback()
        //      {
        //          if (m_UseOSFallback == null)
        //              return;

        //          EditorGUI.BeginChangeCheck();
        //          EditorGUILayout.PropertyField(m_UseOSFallback, k_UseOSFontFallback);
        //          if (EditorGUI.EndChangeCheck())
        //          {
        //              m_TextComponent.useOSFontFallback = m_UseOSFallback.boolValue;
        //              m_HavePropertiesChanged = true;
        //          }
        //      }

        // Method to handle multi object selection
        protected override bool IsMixSelectionTypes()
        {
            GameObject[] objects = Selection.gameObjects;
            if (objects.Length > 1)
            {
                for (int i = 0; i < objects.Length; i++)
                {
					if (objects[i].GetComponent<TextMeshProUGUI>() == null)
                        return true;
                }
            }
            return false;
        }
        protected override void OnUndoRedo()
        {
            int undoEventId = Undo.GetCurrentGroup();
            int lastUndoEventId = s_EventId;

            if (undoEventId != lastUndoEventId)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    //Debug.Log("Undo & Redo Performed detected in Editor Panel. Event ID:" + Undo.GetCurrentGroup());
                    TMPro_EventManager.ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED(true, targets[i] as TextMeshProUGUI);
                    s_EventId = undoEventId;
                }
            }
        }
    }
}
