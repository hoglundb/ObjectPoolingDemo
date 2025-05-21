## Project Overview

This project implements a highly flexible **object pooling system** in **C#/Unity**, designed to manage any object type that inherits from the `PoolableObject` abstract class. Object pooling is a critical optimization technique in game development, especially for frequently instantiated objects. This repository provides a complete Unity project showcasing the object pool's functionality, emphasizing its ability to handle generic object types through a well-defined inheritance structure.

### Key Features:

*   **Generic Object Pool:** The `ObjectPool<T>` class is generic, allowing you to create pools for any component that inherits from `PoolableObject`.
*   **Inheritance-Driven:**  The `PoolableObject` abstract class provides a base for all poolable objects, defining `OnSpawn()` and `OnDespawn()` methods for custom logic.
*   **Dynamic Instantiation:** The pool can dynamically instantiate new objects if the pool is empty. It will also grow at run-time if required.
*   **Clean Hierarchy:** Pooled objects are organized under a dedicated parent transform for better scene organization.
*   **Example Scene:** A simple Unity scene demonstrates how to use the object pool with various `PoolableObject` components.
*   **Clear Documentation:** This README provides detailed instructions on how to use the object pool in your own projects.


### Benefits of Object Pooling:

*   **Reduced Garbage Collection:** Reusing objects minimizes garbage collection, leading to smoother gameplay.
*   **Improved Performance:** Pre-allocating objects makes "spawning" much faster.
*   **Type Safety and Flexibility:** The generic approach combined with inheritance ensures type safety and allows for a wide range of poolable objects.

## Unity Project

The Unity project contains everything you need to see the object pool in action. Below is a detailed explanation of how each component works.

### 1. The PoolableObject Class
This is an abstract base class for all poolable objects. We require derived classes to implement `OnSpawn()` (called when an object is taken from the pool) and `OnDespawn()` (called when an object is returned to the pool). `IsInPool` tracks the object's pool status.

```csharp
   public abstract class PoolableObject : MonoBehaviour
    {

        public bool IsInPool = false;

        public abstract void OnSpawn();
        public abstract void OnDespawn();
    }
```

### 2. The ObjectPool Class. 

*   `pool`: Stores the instances of pooled objects.
*   `prefab`: The prefab used to create new instances.
*   `poolParent`: The parent transform for all pooled objects in the hierarchy, keeping the scene organized.

``` csharp
private List<T> pool = new List<T>();
private T prefab;
private Transform poolParent;
```
<br>

Constructor: Initializes the object pool. Takes the prefab, initial size, and an optional parent transform as arguments. Instantiates the initial pool of objects and sets them as inactive.
```csharp
public ObjectPool(T prefab, int initialSize, Transform parent = null)
{
    this.prefab = prefab;

    if (parent == null)
    {
        var poolObject = new GameObject(prefab.name + " Pool");
        poolParent = poolObject.transform;
    }
    else poolParent = parent;

    for (int i = 0; i < initialSize; i++)
    {
        T obj = Object.Instantiate(prefab, poolParent);
        obj.gameObject.name = prefab.name;
        obj.IsInPool = true;
        obj.OnDespawn();
        pool.Add(obj);
    }
}
```
<br>

`Get()` Retrieves an available object from the pool. If no object is available, it instantiates a new one.

```csharp
public T Get()
{
    for (int i = 0; i < pool.Count; i++)
    {
        if (pool[i] != null && pool[i].IsInPool)
        {
            pool[i].IsInPool = false;
            pool[i].OnSpawn();
            return pool[i];
        }
    }

    T newObj = Object.Instantiate(prefab, poolParent);
    newObj.name = prefab.name;
    newObj.gameObject.name = prefab.name;
    newObj.IsInPool = false;
    newObj.OnSpawn();
    pool.Add(newObj);
    return newObj;
}
```
<br>

`Return()` Returns an object to the pool, making it available for reuse.

```csharp
public void Return(T obj)
{
    if (obj == null)
    {
        Debug.LogError("Trying to return null object to pool");
        return;
    }
    obj.IsInPool = true;
    obj.OnDespawn();
}
```

### 3. The ObjectPoolManager

First we'll use a psudo singleton for this class to make it globally accessable

```csharp
public static PoolManager Instance;

private void Awake() => Instance = this;
```
<br>

This dictionary stores all of our individual object pools.  The key is the name of the prefab, and the value is the actual `ObjectPool` instance.  We use `object` as the value type because we'll be storing `ObjectPool<T>` instances, and `T` can be different types.
``` csharp
private Dictionary<string, object> pools = new();
```

<br>

The core of the PoolManager is the `Get`<T> method. Here we use lazy loading and the idea is that we don't create the pool for a given object until it is requested:

```csharp
 public T Get<T>(T prefab, int initialSize = 10) where T : PoolableObject
{
   
    string key = prefab.name;

    if (string.IsNullOrEmpty(key))
    {
        Debug.LogError("PrefabIdentity component is missing a prefab Id: " + prefab.name);
        return null;
    }

    if (!pools.ContainsKey(key))
        pools[key] = new ObjectPool<T>(prefab, initialSize);

    return ((ObjectPool<T>)pools[key]).Get();
}
```

<br>

This method returns an object to its pool.  It's crucial for object pooling to work correctly. It finds the appropriate pool based on the object's name and returns the object to it.  Error handling is included to catch cases where you try to return a null object or an object to a pool that doesn't exist.

```csharp
 public void Return<T>(T obj) where T : PoolableObject
 {
    if (obj == null)
    {
        Debug.LogError("Trying to return null object");
        return;
    }

    var key = obj.name;

    if (string.IsNullOrEmpty(key))
    {
        Debug.LogError("PrefabIdentity component is missing a prefab Id: " + obj.name);
        return;
        }

    if (pools.ContainsKey(key))
    {
        ((ObjectPool<T>)pools[key]).Return(obj);
        return;
    }

    Debug.LogError("Trying to return object to non-existent pool: " + key + " obj name: " + obj.name);
 }
```

<br>

By using the `PoolManager`, you can easily manage all your object pools in one place, making your code cleaner and more efficient.  It handles the creation and retrieval of pools, so you don't have to worry about the details in your other scripts.


### 4. Example: Time-Based Poolable Object

Let's create an example of a poolable object that automatically returns to the pool after a certain amount of time. This is a common use case for object pooling, especially for effects or temporary objects. We'll call this class `TimeBasedPoolableObject`. Note that
we use the `Update()` loop instead of a `coroutine` as this will be more efficient for working with large numbers of objects.

```csharp
using UnityEngine;

namespace Code.Pooling
{
    public class TimeBasedPoolableObject : PoolableObject
    {
        [Space]
        [SerializeField] private float lifeTimeDuration = 10;

        private float timeSinceSpawned;

        private void OnValidate() => enabled = false;

        public override void OnSpawn()
        {
            timeSinceSpawned = 0;
            enabled = true;
            gameObject.SetActive(true);
        }

        public override void OnDespawn()
        {
            enabled = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            timeSinceSpawned += Time.deltaTime;
            if (timeSinceSpawned < lifeTimeDuration) return;
            ReturnToPool();
        }

        private void ReturnToPool() => PoolManager.Instance.Return(this);
    }
}
```

<br> 

### 5. Remarks

This object pooling system provides a robust and efficient solution for managing frequently instantiated objects in Unity.  By leveraging inheritance and a centralized PoolManager, it simplifies object pooling implementation and improves performance.  Feel free to adapt and extend this system to fit the specific needs of your projects.
