using UnityEngine;

public class ResourceExample : MonoBehaviour
{
    private GameObject _axe;
    private GameObject _shotgun;

	private GameObject _axeInstance;
	private GameObject _shotgunInstance;
    
    private void Start()
    {
        LoadEquips();
    }

    /// <summary>
    /// 加载装备示例 - 已从LoadWeapons改为LoadEquips，统一使用装备系统
    /// </summary>
    private void LoadEquips()
    {
        // 加载斧头
        _axe = ResourceManager.Instance.Load<GameObject>("Prefabs/Equips/pbsc_equip_axe");
        if (_axe != null)
        {
            _axeInstance = Instantiate(_axe);
            _axeInstance.transform.position = new Vector3(-2, 0, 0);
        }

        // 加载散弹枪
        _shotgun = ResourceManager.Instance.Load<GameObject>("Prefabs/Equips/pbsc_equip_shotgun");
        if (_shotgun != null)
        {
            _shotgunInstance = Instantiate(_shotgun);
            _shotgunInstance.transform.position = new Vector3(2, 0, 0);
        }
    }

    private void OnDestroy()
    {
		if (_axeInstance != null)
		{
			Destroy(_axeInstance);
		}
		if (_shotgunInstance != null)
		{
			Destroy(_shotgunInstance);
		}

        if (_axe != null)
        {
            ResourceManager.Instance.Release(_axe);
            _axe = null;
        }

        if (_shotgun != null)
        {
            ResourceManager.Instance.Release(_shotgun);
            _shotgun = null;
        }
    }
} 