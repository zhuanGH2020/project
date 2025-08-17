using UnityEngine;

/// <summary>
/// 地图工具类 - 提供地图和地形相关的工具方法
/// 无状态设计，所有方法为静态方法
/// </summary>
public static class MapUtils
{
    #region 地形检测
    
    /// <summary>
    /// 获取指定XZ坐标的地面位置（自动检测地形高度）
    /// </summary>
    /// <param name="posX">X坐标</param>
    /// <param name="posZ">Z坐标</param>
    /// <param name="rayStartHeight">射线起始高度，默认100f</param>
    /// <param name="maxDistance">射线最大检测距离，默认200f</param>
    /// <param name="layerMask">检测层级，默认所有层</param>
    /// <returns>包含地面高度的3D位置</returns>
    public static Vector3 GetGroundPosition(float posX, float posZ, float rayStartHeight = 100f, float maxDistance = 200f, int layerMask = -1)
    {
        // 从较高位置向下发射射线检测地面
        Vector3 rayStart = new Vector3(posX, rayStartHeight, posZ);
        Vector3 rayDirection = Vector3.down;
        
        // 射线检测地面
        if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, maxDistance, layerMask))
        {
            // 检测到地面，使用检测到的高度
            return hit.point;
        }
        else
        {
            // 未检测到地面，使用默认高度
            Debug.LogWarning($"[MapUtils] 在位置 ({posX}, {posZ}) 未检测到地面，使用默认高度 0");
            return new Vector3(posX, 0f, posZ);
        }
    }
    
    /// <summary>
    /// 获取指定Vector2坐标的地面位置
    /// </summary>
    public static Vector3 GetGroundPosition(Vector2 position, float rayStartHeight = 100f, float maxDistance = 200f, int layerMask = -1)
    {
        return GetGroundPosition(position.x, position.y, rayStartHeight, maxDistance, layerMask);
    }
    
    /// <summary>
    /// 检测指定位置是否有地面
    /// </summary>
    public static bool HasGround(float posX, float posZ, float rayStartHeight = 100f, float maxDistance = 200f, int layerMask = -1)
    {
        Vector3 rayStart = new Vector3(posX, rayStartHeight, posZ);
        Vector3 rayDirection = Vector3.down;
        
        return Physics.Raycast(rayStart, rayDirection, maxDistance, layerMask);
    }
    
    /// <summary>
    /// 获取地面高度（仅返回Y坐标）
    /// </summary>
    public static float GetGroundHeight(float posX, float posZ, float rayStartHeight = 100f, float maxDistance = 200f, int layerMask = -1)
    {
        Vector3 groundPos = GetGroundPosition(posX, posZ, rayStartHeight, maxDistance, layerMask);
        return groundPos.y;
    }
    
    #endregion
    
    #region 智能生成位置
    
    /// <summary>
    /// 获取安全的生成位置 - 检测冲突并自动寻找附近空闲位置
    /// </summary>
    /// <param name="targetPosition">目标生成位置（XZ坐标）</param>
    /// <param name="checkRadius">冲突检测半径，使用GameSettings默认值</param>
    /// <param name="searchRadius">搜索空闲位置的半径，使用GameSettings默认值</param>
    /// <param name="maxAttempts">最大搜索尝试次数，使用GameSettings默认值</param>
    /// <param name="layerMask">检测的层级，默认所有层</param>
    /// <param name="groundLayerMask">地面检测层级，默认所有层</param>
    /// <returns>安全的3D生成位置，如果找不到则返回原始位置</returns>
    public static Vector3 GetSafeSpawnPosition(Vector2 targetPosition, 
        float checkRadius = GameSettings.MapSafeSpawnCheckRadius, 
        float searchRadius = GameSettings.MapSafeSpawnSearchRadius, 
        int maxAttempts = GameSettings.MapSafeSpawnMaxAttempts, 
        int layerMask = -1,
        int groundLayerMask = -1)
    {
        // 首先检查目标位置是否安全
        Vector3 targetWorld = GetGroundPosition(targetPosition);
        
        if (IsPositionSafe(targetWorld, checkRadius, layerMask))
        {
            return targetWorld;
        }
        
        // 目标位置被占用，搜索附近安全位置
        Debug.Log($"[MapUtils] 位置 ({targetPosition.x:F1}, {targetPosition.y:F1}) 被占用，搜索附近安全位置...");
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // 在搜索半径内随机生成一个位置
            Vector2 randomOffset = Random.insideUnitCircle * searchRadius;
            Vector2 candidatePos = targetPosition + randomOffset;
            
            // 检查该位置是否有地面
            if (!HasGround(candidatePos.x, candidatePos.y, layerMask: groundLayerMask))
            {
                continue;
            }
            
            // 获取地面位置并检查是否安全
            Vector3 candidateWorld = GetGroundPosition(candidatePos);
            
            if (IsPositionSafe(candidateWorld, checkRadius, layerMask))
            {
                Debug.Log($"[MapUtils] 找到安全位置 ({candidateWorld.x:F1}, {candidateWorld.z:F1})，尝试次数: {attempt + 1}");
                return candidateWorld;
            }
        }
        
        // 搜索失败，返回原始位置并警告
        Debug.LogWarning($"[MapUtils] 无法找到安全生成位置，使用原始位置 ({targetWorld.x:F1}, {targetWorld.z:F1})，最大尝试次数: {maxAttempts}");
        return targetWorld;
    }
    
    /// <summary>
    /// 获取安全的生成位置 - 重载方法，支持3D输入位置
    /// </summary>
    public static Vector3 GetSafeSpawnPosition(Vector3 targetPosition, 
        float checkRadius = GameSettings.MapSafeSpawnCheckRadius, 
        float searchRadius = GameSettings.MapSafeSpawnSearchRadius, 
        int maxAttempts = GameSettings.MapSafeSpawnMaxAttempts, 
        int layerMask = -1,
        int groundLayerMask = -1)
    {
        Vector2 targetPos2D = new Vector2(targetPosition.x, targetPosition.z);
        return GetSafeSpawnPosition(targetPos2D, checkRadius, searchRadius, maxAttempts, layerMask, groundLayerMask);
    }
    
    /// <summary>
    /// 检查指定位置是否安全（没有其他物体）
    /// </summary>
    /// <param name="position">要检查的3D位置</param>
    /// <param name="checkRadius">检测半径，使用GameSettings默认值</param>
    /// <param name="layerMask">检测的层级</param>
    /// <returns>true表示位置安全，false表示有冲突</returns>
    public static bool IsPositionSafe(Vector3 position, float checkRadius = GameSettings.MapSafeSpawnCheckRadius, int layerMask = -1)
    {
        // 使用OverlapSphere检测是否有碰撞体
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius, layerMask);
        
        foreach (var collider in colliders)
        {
            // 忽略Trigger类型的碰撞体（如触发器、传送门等）
            if (collider.isTrigger)
                continue;
                
            // 忽略地面碰撞体（通过标签或组件判断）
            if (IsGroundCollider(collider))
                continue;
                
            // 发现非地面的实体碰撞体，位置不安全
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 判断碰撞体是否为地面
    /// </summary>
    private static bool IsGroundCollider(Collider collider)
    {
        // 通过标签判断
        if (collider.CompareTag("Ground") || collider.CompareTag("Terrain"))
        {
            return true;
        }
        
        // 通过GameObject名称判断（可根据项目需要调整）
        string objName = collider.gameObject.name.ToLower();
        if (objName.Contains("ground") || objName.Contains("terrain") || objName.Contains("floor"))
        {
            return true;
        }
        
        return false;
    }
    
    #endregion
    
    #region 坐标转换
    
    /// <summary>
    /// 将3D世界坐标转换为2D地图坐标（XZ平面）
    /// </summary>
    public static Vector2 WorldToMapCoordinate(Vector3 worldPosition)
    {
        return new Vector2(worldPosition.x, worldPosition.z);
    }
    
    /// <summary>
    /// 将2D地图坐标转换为3D世界坐标（自动检测地面高度）
    /// </summary>
    public static Vector3 MapToWorldCoordinate(Vector2 mapPosition)
    {
        return GetGroundPosition(mapPosition.x, mapPosition.y);
    }
    
    #endregion
} 