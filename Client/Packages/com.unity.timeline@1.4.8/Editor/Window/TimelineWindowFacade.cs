using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Playables;

namespace UnityEditor.Timeline
{
    public static class TimelineWindowFacade
    {
        public static void OpenTimelineWindow()
        {
            if(TimelineWindow.instance == null)
            {
                TimelineWindow timelineWindow = EditorWindow.GetWindow<TimelineWindow>();
                timelineWindow.Show();
            }
            else
            {
                TimelineWindow.instance.Show();
            }
        }
        public static void FocusTimelineWindow()
        {
            if (TimelineWindow.instance == null)
            {
                TimelineWindow timelineWindow = EditorWindow.GetWindow<TimelineWindow>();
                timelineWindow.Focus();
            }
            else
            {
                TimelineWindow.instance.Focus();
            }
        }
        public static void SetCurrentTimeline(PlayableDirector playableDirector)
        {
            if (TimelineWindow.instance == null)
            {
                TimelineWindow timelineWindow = EditorWindow.GetWindow<TimelineWindow>();
                timelineWindow.SetCurrentTimeline(playableDirector);
            }
            else
            {
                TimelineWindow.instance.SetCurrentTimeline(playableDirector);
            }
        }
        public static void BindTimelinePreviewEvent(Action onPreviewUdate, Action onPreviewEnd)
        {
            TimelineWindow.onPreviewUpdate -= onPreviewUdate;
            TimelineWindow.onPreviewUpdate += onPreviewUdate;
            TimelineWindow.onPreviewEnd -= onPreviewEnd;
            TimelineWindow.onPreviewEnd += onPreviewEnd;
        }
        public static void UnbindTimelinePreviewEvent(Action[] onPreviewUdate, Action[] onPreviewEnd)
        {
            foreach (var action in onPreviewUdate)
            {
                TimelineWindow.onPreviewUpdate -= action;
            }
            foreach (var action in onPreviewEnd)
            {
                TimelineWindow.onPreviewEnd -= action;
            }
        }
        public static bool Play()
        {
            var timelineWindow = TimelineWindow.instance;
            if (timelineWindow == null)
            {
                return false;
            }
            timelineWindow.state.SetPlaying(true);
            return true;
        }
        public static bool NextFrame()
        {
            var timelineWindow = TimelineWindow.instance;
            if (timelineWindow == null)
            {
                return false;
            }
            timelineWindow.state.referenceSequence.frame += 1;
            if (IsEnd())
            {
                return false;
            }
            return true;
        }
        public static bool IsEnd()
        {
            var timelineWindow = TimelineWindow.instance;
            if (timelineWindow == null)
            {
                return true;
            }
            var totalFrame = TimelineWindow.instance.state.referenceSequence.duration * TimelineWindow.instance.state.referenceSequence.frameRate;
            if(timelineWindow.state.referenceSequence.frame >= totalFrame)
            {
                return true;
            }
            return false;
        }
        public static bool GoToBegin()
        {
            var timelineWindow = TimelineWindow.instance;
            if (timelineWindow == null)
            {
                return false;
            }
            var state = timelineWindow.state;
            state.referenceSequence.time = 0;
            //state.EnsurePlayHeadIsVisible();
            return true;
        }

    }
}
