using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace UnityEngine.Timeline
{
    partial class AnimationPlayableAsset : ISerializationCallbackReceiver
    {
        enum Versions
        {
            Initial = 0,
            RotationAsEuler = 1,
        }
        static readonly int k_LatestVersion = (int)Versions.RotationAsEuler;
        [SerializeField, HideInInspector] int m_Version;

        [SerializeField, Obsolete("Use m_RotationEuler Instead", false), HideInInspector]
        private Quaternion m_Rotation = Quaternion.identity;  // deprecated. now saves in euler angles

        /// <summary>
        /// Called before Unity serializes this object.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            m_Version = k_LatestVersion;
        }

        /// <summary>
        /// Called after Unity deserializes this object.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_Version < k_LatestVersion)
            {
                OnUpgradeFromVersion(m_Version); //upgrade derived classes
            }
        }

        void OnUpgradeFromVersion(int oldVersion)
        {
            if (oldVersion < (int)Versions.RotationAsEuler)
                AnimationPlayableAssetUpgrade.ConvertRotationToEuler(this);
        }

        static class AnimationPlayableAssetUpgrade
        {
            public static void ConvertRotationToEuler(AnimationPlayableAsset asset)
            {
#pragma warning disable 618
                asset.m_EulerAngles = asset.m_Rotation.eulerAngles;
#pragma warning restore 618
            }
        }

    }
    
    public static class AnimationPlayableAssetUtil
    {  
        
#if UNITY_EDITOR
        public static void SyncMaterialLod(AnimationClip clip, string originStr, List<string> targetStrs)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            
            var wrappers = new List<CurveWrapper>(bindings.Count());
            int curveWrapperId = 0;
            foreach (EditorCurveBinding b in bindings)
            {
                if (!b.path.Contains(originStr)) continue;

                foreach (var targetStr in targetStrs)
                {
                    var newBinding = new EditorCurveBinding
                    {
                        path = b.path.Replace(originStr, targetStr),
                        type = typeof(SkinnedMeshRenderer),
                        propertyName = b.propertyName
                    };
                    // General configuration
                    var wrapper = new CurveWrapper
                    {
                        id = curveWrapperId++,
                        binding = newBinding,
                        groupId = -1,
                        hidden = false,
                        readOnly = false,
                        getAxisUiScalarsCallback = () => new Vector2(1, 1)
                    };

                    // Specific configuration
                    ConfigureCurveWrapper(wrapper, clip, b);

                    wrappers.Add(wrapper);
                }
            }
            
            wrappers.ForEach(w => AnimationUtility.SetEditorCurve(clip, w.binding, w.curve));
        }

        private static void ConfigureCurveWrapper(CurveWrapper wrapper, AnimationClip animationClip, EditorCurveBinding originBinding)
        {
            wrapper.color = CurveUtility.GetPropertyColor(wrapper.binding.propertyName);
            wrapper.renderer = new NormalCurveRenderer(AnimationUtility.GetEditorCurve(animationClip, originBinding));
            wrapper.renderer.SetCustomRange(0.0f, animationClip.length);
        }
#endif
    
    }
}
