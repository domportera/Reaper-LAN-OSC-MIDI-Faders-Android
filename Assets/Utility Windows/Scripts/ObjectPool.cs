using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly GameObject _prefab;
    private readonly Transform _parent;
    private readonly Queue<GameObject> _pool = new();
    private readonly HashSet<GameObject> _objectsSpawnedFromPool = new();

    public ObjectPool(GameObject prefab, RectTransform parent, int preSpawnCount)
    {
        _prefab = prefab;
        _parent = parent;

        for(int i = 0; i < preSpawnCount; i++)
        {
            AddToPool();
        }
    }

    public GameObject Instantiate()
    {
        if(_pool.Count == 0)
        {
            AddToPool();
        }

        GameObject obj = _pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    void AddToPool()
    {
        GameObject obj = UnityEngine.Object.Instantiate(_prefab, _parent);
        obj.SetActive(false);
        _pool.Enqueue(obj);
        _objectsSpawnedFromPool.Add(obj);
    }
    
    public void ReturnToPool(GameObject obj)
    {
        if(!_objectsSpawnedFromPool.Contains(obj))
        {
            Debug.LogError($"Object {obj.name} was not instantiated with the {_prefab.name} pool you are trying to return it to", obj);
            return;
        }

        obj.SetActive(false);
        _pool.Enqueue(obj);
    }

    public void NukeFromPool(GameObject obj)
    {
        if(!_objectsSpawnedFromPool.Contains(obj))
        {
            Debug.LogError($"Object {obj.name} was not instantiated with the {_prefab.name} pool you are trying to return it to", obj);
            return;
        }

        Object.Destroy(obj);
        _objectsSpawnedFromPool.Remove(obj);
    }
}