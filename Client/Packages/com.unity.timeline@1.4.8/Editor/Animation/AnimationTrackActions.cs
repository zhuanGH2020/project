using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    [MenuEntry("Add Override Track", MenuPriority.CustomTrackActionSection.addOverrideTrack), UsedImplicitly]
    class AddOverrideTrackAction : TrackAction
    {
        public override bool Execute(IEnumerable<TrackAsset> tracks)
        {
            foreach (var animTrack in tracks.OfType<AnimationTrack>())
            {
                if (animTrack.GetChildTracks().Any())
                {
                    if (EditorUtility.DisplayDialog("提示", $"轨道 {animTrack.name} 可能已经存在Override轨道，" +
                                                          $"第二条Override轨道在保存时会自动合并，可能导致数据丢失，" +
                                                          $"是否继续？", "确定", "取消"))
                    {
                        TimelineHelpers.CreateTrack(typeof(AnimationTrack), animTrack, "Override " + animTrack.GetChildTracks().Count());
                    }
                }
                else
                {
                    TimelineHelpers.CreateTrack(typeof(AnimationTrack), animTrack, "Override " + animTrack.GetChildTracks().Count());
                }
            }

            return true;
        }

        public override ActionValidity Validate(IEnumerable<TrackAsset> tracks)
        {
            if (tracks.Any(t => t.isSubTrack || !t.GetType().IsAssignableFrom(typeof(AnimationTrack))))
                return ActionValidity.NotApplicable;

            if (tracks.Any(t => t.lockedInHierarchy))
                return ActionValidity.Invalid;

            return ActionValidity.Valid;
        }
    }

    [MenuEntry("Convert To Clip Track", MenuPriority.CustomTrackActionSection.convertToClipMode), UsedImplicitly]
    class ConvertToClipModeAction : TrackAction
    {
        public override bool Execute(IEnumerable<TrackAsset> tracks)
        {
            foreach (var animTrack in tracks.OfType<AnimationTrack>())
                animTrack.ConvertToClipMode();

            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);

            return true;
        }

        public override ActionValidity Validate(IEnumerable<TrackAsset> tracks)
        {
            if (tracks.Any(t => !(t.GetType().IsAssignableFrom(typeof(AnimationTrack)) || t.GetType().IsSubclassOf(typeof(AnimationTrack)))))
                return ActionValidity.NotApplicable;

            if (tracks.Any(t => t.lockedInHierarchy))
                return ActionValidity.Invalid;

            if (tracks.OfType<AnimationTrack>().All(a => a.CanConvertToClipMode()))
                return ActionValidity.Valid;

            return ActionValidity.NotApplicable;
        }
    }

    [MenuEntry("Skill Fx/Convert To Clip Track", MenuPriority.BattleSkillEditor.convertClip), UsedImplicitly]
    class BattleSkillEditorConvertToClipModeAction : TrackAction
    {
        public override bool Execute(IEnumerable<TrackAsset> tracks)
        {
            foreach (var animTrack in tracks.OfType<AnimationTrack>())
                animTrack.BattleSkillEditorConvertToClipMode();

            UnityEditor.AssetDatabase.Refresh();
            return true;
        }

        public override ActionValidity Validate(IEnumerable<TrackAsset> tracks)
        {
            if (tracks.Any(t => !(t.GetType().IsAssignableFrom(typeof(AnimationTrack)) || t.GetType().IsSubclassOf(typeof(AnimationTrack)))))
                return ActionValidity.NotApplicable;

            if (tracks.Any(t => t.lockedInHierarchy))
                return ActionValidity.Invalid;

            if (tracks.OfType<AnimationTrack>().All(a => a.CanConvertToClipMode()))
                return ActionValidity.Valid;

            return ActionValidity.NotApplicable;
        }
    }

    [MenuEntry("Convert To Infinite Clip", MenuPriority.CustomTrackActionSection.convertFromClipMode), UsedImplicitly]
    class ConvertFromClipTrackAction : TrackAction
    {
        public override bool Execute(IEnumerable<TrackAsset> tracks)
        {
            foreach (var animTrack in tracks.OfType<AnimationTrack>())
                animTrack.ConvertFromClipMode(TimelineEditor.inspectedAsset);

            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);

            return true;
        }

        public override ActionValidity Validate(IEnumerable<TrackAsset> tracks)
        {
            if (tracks.Any(t => !t.GetType().IsAssignableFrom(typeof(AnimationTrack))))
                return ActionValidity.NotApplicable;

            if (tracks.Any(t => t.lockedInHierarchy))
                return ActionValidity.Invalid;

            if (tracks.OfType<AnimationTrack>().All(a => a.CanConvertFromClipMode()))
                return ActionValidity.Valid;

            return ActionValidity.NotApplicable;
        }
    }

    abstract class TrackOffsetBaseAction : TrackAction
    {
        public abstract TrackOffset trackOffset { get; }

        public override ActionValidity Validate(IEnumerable<TrackAsset> tracks)
        {
            if (tracks.Any(t => !t.GetType().IsAssignableFrom(typeof(AnimationTrack))))
                return ActionValidity.NotApplicable;

            if (tracks.Any(t => t.lockedInHierarchy))
            {
                return ActionValidity.Invalid;
            }

            return ActionValidity.Valid;
        }

        public override bool Execute(IEnumerable<TrackAsset> tracks)
        {
            foreach (var animTrack in tracks.OfType<AnimationTrack>())
            {
                animTrack.UnarmForRecord();
                animTrack.trackOffset = trackOffset;
            }

            TimelineEditor.Refresh(RefreshReason.ContentsModified);
            return true;
        }
    }


    [MenuEntry("Track Offsets/Apply Transform Offsets", MenuPriority.CustomTrackActionSection.applyTrackOffset), UsedImplicitly]
    [ApplyDefaultUndo]
    class ApplyTransformOffsetAction : TrackOffsetBaseAction
    {
        public override TrackOffset trackOffset
        {
            get { return TrackOffset.ApplyTransformOffsets; }
        }
    }

    [MenuEntry("Track Offsets/Apply Scene Offsets", MenuPriority.CustomTrackActionSection.applySceneOffset), UsedImplicitly]
    [ApplyDefaultUndo]
    class ApplySceneOffsetAction : TrackOffsetBaseAction
    {
        public override TrackOffset trackOffset
        {
            get { return TrackOffset.ApplySceneOffsets; }
        }
    }

    [MenuEntry("Track Offsets/Auto (Deprecated)", MenuPriority.CustomTrackActionSection.applyAutoOffset), UsedImplicitly]
    [ApplyDefaultUndo]
    class ApplyAutoAction : TrackOffsetBaseAction
    {
        public override TrackOffset trackOffset
        {
            get { return TrackOffset.Auto; }
        }
    }
}
