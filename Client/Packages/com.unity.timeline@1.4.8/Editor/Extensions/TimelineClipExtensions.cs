using UnityEngine.Timeline;
using UnityEngine;

namespace UnityEditor.Timeline
{
    internal static class TimelineClipExtensions
    {
        public static void BattleSkillEditRecordToAnimationClipMode(this TimelineClip clip)
        {
            if (clip == null)
                return;

            var animClip = clip.animationClip;
            var track = clip.parentTrack;
            var path = GenerateClipPath(AssetDatabase.GetAssetPath(track));

            var newAnimClip = new AnimationClip();
            EditorUtility.CopySerialized(animClip, newAnimClip);
            AssetDatabase.CreateAsset(newAnimClip, path);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            if (clip.asset is AnimationPlayableAsset asset)
            {
                asset.clip = newAnimClip;
                clip.recordable = false;
            }

            EditorUtility.SetDirty(track);
            Object.DestroyImmediate(animClip, true);
        }

        static string GenerateClipPath(string timelineAssetPath)
        {
            var index = 0;
            var dir = timelineAssetPath.Substring(0, timelineAssetPath.LastIndexOf('/'));
            var path = $"{dir}/recorded_{index}.anim";
            while (System.IO.File.Exists(path))
            {
                ++index;
                path = $"{dir}/recorded_{index}.anim";
            }
            return path;
        }
    }
}
