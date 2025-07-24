using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(TextMeshProUGUI))]
public class ThaiLinkedOverflowOverride : MonoBehaviour
{
    TextMeshProUGUI textMeshPro;

    public bool disableItalic;

    public int targetUnicode = 63277; //泰语的默认图片首字

    private int m_unicode;
    
    private string m_startStr;

    //private string thai = "th";
    void Awake()
    {
        //TMP_Settings.curLanguage = thai;

        m_unicode = targetUnicode;
        m_startStr = char.ConvertFromUtf32(targetUnicode).ToString();

        textMeshPro = GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.curLanguage == "th" && disableItalic)
        {
            textMeshPro.fontStyle = FontStyles.Normal;
        }
    }

    void Start()
    {
    }

    void Update()
    {
        if (TMP_Settings.curLanguage != "th")
            return;

        if (m_unicode != targetUnicode)
        {
            m_unicode = targetUnicode;
            m_startStr = char.ConvertFromUtf32(targetUnicode).ToString();
        }

        string text = textMeshPro.text;
        if (textMeshPro.text.StartsWith(m_startStr))
            return;

        text = m_startStr + text;
        textMeshPro.text = text;
    }
}
