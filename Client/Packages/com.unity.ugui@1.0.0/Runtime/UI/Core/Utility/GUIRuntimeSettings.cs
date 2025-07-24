namespace UnityEngine.UI
{
    //for some reason
    //we don't need some callback in some case
    //so we need some params to control it
    
    public static class GUIRuntimeSettings
    {
        public static bool OpenOnCanvasHierarchyChanged = true;

        public static bool OpenOnBeforeTransformParentChanged = true;

        public static bool OpenOnCanvasGroupChanged = true;

        public static bool OpenRectTrasnformChangeRebuild = true;
    }
}