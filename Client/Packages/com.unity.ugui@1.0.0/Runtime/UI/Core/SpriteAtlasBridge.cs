using UnityEngine.U2D;

namespace UnityEngine.UI
{
    public static class SpriteAtlasBridge
    {
        public static void RegistAtlas(SpriteAtlas atlas)
        {
            SpriteAtlasManager.Register(atlas);
        }
    }
}