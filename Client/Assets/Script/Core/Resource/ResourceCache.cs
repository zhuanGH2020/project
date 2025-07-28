using UnityEngine;

public class ResourceCache
{
    public UnityEngine.Object Resource { get; private set; }
    private int _refCount;
    public string Path { get; private set; }

    public ResourceCache(UnityEngine.Object resource, string path)
    {
        Resource = resource;
        Path = path;
        _refCount = 1;
    }

    public void Retain()
    {
        _refCount++;
    }

    public bool Release()
    {
        _refCount--;
        return _refCount <= 0;
    }
} 