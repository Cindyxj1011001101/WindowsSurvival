using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 游戏对象缓存池
/// </summary>
public class ObjectBufferPool
{
    private static ObjectBufferPool instance = new();

    public static ObjectBufferPool Instance => instance;


    /// <summary>
    /// 游戏对象容器
    /// </summary>
    private class ObjectBuffer
    {
        /// <summary>
        /// 暂时没有使用的对象
        /// </summary>
        private Stack<GameObject> objectsNotUsed = new();

        /// <summary>
        /// 场景中正在使用的对象
        /// </summary>
        private List<GameObject> objectsInUse = new();

        /// <summary>
        /// 对象在场景上的名称
        /// </summary>
        private string objectName;

        /// <summary>
        /// 预设体对象
        /// </summary>
        private GameObject prefab;

        /// <summary>
        /// 场景中最多正在使用的对象数量
        /// </summary>
        private int maxCountOfObjectsInUse;

        /// <summary>
        /// 当预设体加载完毕后执行的逻辑
        /// </summary>
        private UnityAction onPrefabLoaded;

        /// <summary>
        /// 缓存是否待删除
        /// </summary>
        public bool toBeDeleted;

        public ObjectBuffer(GameObject prefab)
        {
            objectName = prefab.name;

            this.prefab = prefab;

            if (toBeDeleted) return;

            // 获取对象最大同屏数量配置
            if (!prefab.TryGetComponent(out ObjectBufferConfig config))
            {
                Debug.LogError($"请为使用缓存池功能的预设体对象添加{typeof(ObjectBufferConfig).Name}脚本");
                maxCountOfObjectsInUse = 128;
            }
            else
            {
                maxCountOfObjectsInUse = config.maxCount;
            }

            onPrefabLoaded?.Invoke();
            onPrefabLoaded = null;
        }

        public ObjectBuffer(string bundleName, string assetName, bool sync = false)
        {
            objectName = bundleName + "/" + assetName;

            // 同步加载预制体
            if (sync)
            {
                // 记录预设体 方便以后实例化
                prefab = ResourcesManager.Instance.Load<GameObject>(objectName);

                // 获取对象最大同屏数量配置
                if (!prefab.TryGetComponent(out ObjectBufferConfig config))
                {
                    Debug.LogError($"请为使用缓存池功能的预设体对象添加{typeof(ObjectBufferConfig).Name}脚本");
                    maxCountOfObjectsInUse = 128;
                }
                else
                {
                    maxCountOfObjectsInUse = config.maxCount;
                }
            }
            // 异步加载预制体
            else
            {
                ResourcesManager.Instance.LoadAsync<GameObject>(objectName, (asset) =>
                {
                    // 记录预设体 方便以后实例化
                    prefab = asset;
                    // 若该缓存待删除 则不执行后续逻辑
                    if (toBeDeleted) return;

                    // 获取对象最大同屏数量配置
                    if (!prefab.TryGetComponent(out ObjectBufferConfig config))
                    {
                        Debug.LogError($"请为使用缓存池功能的预设体对象添加{typeof(ObjectBufferConfig).Name}脚本");
                        maxCountOfObjectsInUse = 128;
                    }
                    else
                    {
                        maxCountOfObjectsInUse = config.maxCount;
                    }

                    onPrefabLoaded?.Invoke();
                    onPrefabLoaded = null;
                });
            }
        }

        /// <summary>
        /// 从未使用的或者正在使用的游戏对象中获取一个
        /// </summary>
        /// <returns></returns>
        public void Get(System.Func<GameObject, GameObject> instaniate, UnityAction<GameObject> onInstaniated)
        {
            if (prefab == null)
            {
                onPrefabLoaded += () =>
                {
                    Get(instaniate, onInstaniated);
                };
                return;
            }

            // 执行游戏对象实例化完毕后的回调
            onInstaniated?.Invoke(Get(instaniate));
        }

        /// <summary>
        /// 同步的获取方法
        /// </summary>
        /// <param name="instaniate"></param>
        /// <returns></returns>
        public GameObject Get(System.Func<GameObject, GameObject> instaniate)
        {
            GameObject obj;

            // 有剩余对象时
            if (objectsNotUsed.Count > 0)
            {
                obj = objectsNotUsed.Pop();
            }
            // 没有剩余对象时 并且 正在使用的对象数量超上限时
            else if (objectsInUse.Count >= maxCountOfObjectsInUse)
            {
                // 获取正在使用的 使用最久的物体
                obj = objectsInUse[0];
                objectsInUse.RemoveAt(0);
            }
            // 没有使用对象 并且 正在使用的对象数量不超上限
            else
            {
                // 实例化对象
                obj = instaniate(prefab);
                // 设置对象的名称
                obj.name = objectName;
            }

            // 将该物体添加到正在使用的物体列表的尾部
            objectsInUse.Add(obj);

            // 激活对象
            obj.SetActive(true);

            return obj;
        }

        /// <summary>
        /// 回收用完的游戏对象
        /// </summary>
        /// <param name="obj"></param>
        public void Restore(GameObject obj)
        {
            // 预设体没有加载完成
            if (prefab == null)
            {
                onPrefabLoaded += () =>
                {
                    Restore(obj);
                };
                return;
            }

            // 对象失活
            obj.SetActive(false);
            // 将对象存入池中
            objectsNotUsed.Push(obj);
            // 从正在使用的物体列表中移除
            objectsInUse.Remove(obj);
        }
    }

    /// <summary>
    /// 缓存池数据结构
    /// </summary>
    private Dictionary<string, ObjectBuffer> pool = new();

    private ObjectBufferPool()
    {
    }

    #region 同步——已知预制体
    /// <summary>
    /// 获取预设体实例
    /// </summary>
    public GameObject Get(GameObject prefab, System.Func<GameObject, GameObject> instaniate)
    {
        // 如果不存在容器就创建
        if (!pool.ContainsKey(prefab.name))
            pool.Add(prefab.name, new ObjectBuffer(prefab));

        // 取出游戏对象
        // 处理游戏对象逻辑
        return pool[prefab.name].Get(instaniate);
    }

    /// <summary>
    /// 获取预设体实例
    /// </summary>
    public GameObject Get(GameObject prefab)
    {
        return Get(prefab, (prefab) => Object.Instantiate(prefab));
    }

    /// <summary>
    /// 获取预设体实例，并设置其父对象(实例的位置、旋转和缩放变为相对父对象的)
    /// </summary>
    /// <param name="parent">预设体实例的父对象</param>
    public GameObject Get(GameObject prefab, Transform parent)
    {
        return Get(prefab, (prefab) => Object.Instantiate(prefab, parent));
    }

    /// <summary>
    /// 获取预设体实例，并设置其父对象
    /// </summary>
    /// <param name="parent">预设体实例的父对象</param>
    public GameObject Get(GameObject prefab, Transform parent, bool instantiateInWorldSpace)
    {
        return Get(prefab, (prefab) => Object.Instantiate(prefab, parent, instantiateInWorldSpace));
    }

    /// <summary>
    /// 获取预设体实例，并设置其在世界坐标下的位置和旋转
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Get(prefab, (prefab) => Object.Instantiate(prefab, position, rotation));
    }

    /// <summary>
    /// 获取预设体实例，并设置其父对象和在世界坐标下的位置和旋转(实例的缩放变为相对父对象的)
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        return Get(prefab, (prefab) => Object.Instantiate(prefab, position, rotation, parent));
    }
    #endregion

    #region 同步——加载预制体
    /// <summary>
    /// 获取预设体实例
    /// </summary>
    public GameObject Get(string bundleName, string assetName, System.Func<GameObject, GameObject> instaniate)
    {
        string path = bundleName + "/" + assetName;
        // 如果不存在容器就创建
        if (!pool.ContainsKey(path))
            pool.Add(path, new ObjectBuffer(bundleName, assetName, true));

        // 取出游戏对象
        // 处理游戏对象逻辑
        return pool[path].Get(instaniate);
    }

    /// <summary>
    /// 获取预设体实例
    /// </summary>
    /// <param name="bundleName">预设体所在文件夹相对Editor文件夹的路径 或者 预设体所在的AB包的名称</param>
    /// <param name="assetName">预设体名称</param>
    public GameObject Get(string bundleName, string assetName)
    {
        return Get(bundleName, assetName, (prefab) => Object.Instantiate(prefab));
    }

    /// <summary>
    /// 获取预设体实例，并设置其父对象(实例的位置、旋转和缩放变为相对父对象的)
    /// </summary>
    /// <param name="bundleName">预设体所在文件夹相对Editor文件夹的路径 或者 预设体所在的AB包的名称</param>
    /// <param name="assetName">预设体名称</param>
    /// <param name="parent">预设体实例的父对象</param>
    public GameObject Get(string bundleName, string assetName, Transform parent)
    {
        return Get(bundleName, assetName, (prefab) => Object.Instantiate(prefab, parent));
    }

    /// <summary>
    /// 获取预设体实例，并设置其父对象
    /// </summary>
    /// <param name="bundleName">预设体所在文件夹相对Editor文件夹的路径 或者 预设体所在的AB包的名称</param>
    /// <param name="assetName">预设体名称</param>
    /// <param name="parent">预设体实例的父对象</param>
    /// <param name="worldPositionStays">是否保持预设体在世界坐标系下的位置</param>
    public GameObject Get(string bundleName, string assetName, Transform parent, bool instantiateInWorldSpace)
    {
        return Get(bundleName, assetName, (prefab) => Object.Instantiate(prefab, parent, instantiateInWorldSpace));
    }

    /// <summary>
    /// 获取预设体实例，并设置其在世界坐标下的位置和旋转
    /// </summary>
    /// <param name="bundleName">预设体所在文件夹相对Editor文件夹的路径 或者 预设体所在的AB包的名称</param>
    /// <param name="assetName">预设体名称</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    public GameObject Get(string bundleName, string assetName, Vector3 position, Quaternion rotation)
    {
        return Get(bundleName, assetName, (prefab) => Object.Instantiate(prefab, position, rotation));
    }

    /// <summary>
    /// 获取预设体实例，并设置其父对象和在世界坐标下的位置和旋转(实例的缩放变为相对父对象的)
    /// </summary>
    /// <param name="bundleName">预设体所在文件夹相对Editor文件夹的路径 或者 预设体所在的AB包的名称</param>
    /// <param name="assetName">预设体名称</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    public GameObject Get(string bundleName, string assetName, Vector3 position, Quaternion rotation, Transform parent)
    {
        return Get(bundleName, assetName, (prefab) => Object.Instantiate(prefab, position, rotation, parent));
    }

    #endregion

    #region 异步
    /// <summary>
    /// 获取预设体实例
    /// </summary>
    /// <param name="bundleName">预设体所在文件夹相对Editor文件夹的路径 或者 预设体所在的AB包的名称</param>
    /// <param name="assetName">预设体名称</param>
    /// <param name="onInstaniated">预设体实例化后执行的逻辑</param>
    public void Get(string bundleName, string assetName, System.Func<GameObject, GameObject> instaniate, UnityAction<GameObject> onInstaniated = null)
    {
        string path = bundleName + "/" + assetName;
        // 如果不存在容器就创建
        if (!pool.ContainsKey(path))
            pool.Add(path, new ObjectBuffer(bundleName, assetName));

        // 取出游戏对象
        // 处理游戏对象逻辑
        pool[path].Get(instaniate, onInstaniated);
    }

    /// <summary>
    /// 获取预设体实例
    /// </summary>
    /// <param name="bundleName">预设体所在文件夹相对Editor文件夹的路径 或者 预设体所在的AB包的名称</param>
    /// <param name="assetName">预设体名称</param>
    /// <param name="onInstaniated">预设体实例化后执行的逻辑</param>
    public void Get(string bundleName, string assetName, UnityAction<GameObject> onInstaniated = null)
    {
        Get(bundleName, assetName, (prefab) => Object.Instantiate(prefab), onInstaniated);
    }

    /// <summary>
    /// 获取预设体实例，并设置其父对象(实例的位置、旋转和缩放变为相对父对象的)
    /// </summary>
    /// <param name="bundleName">预设体所在文件夹相对Editor文件夹的路径 或者 预设体所在的AB包的名称</param>
    /// <param name="assetName">预设体名称</param>
    /// <param name="parent">预设体实例的父对象</param>
    /// <param name="onInstaniated">预设体实例化后执行的逻辑</param>
    public void Get(string bundleName, string assetName, Transform parent, UnityAction<GameObject> onInstaniated = null)
    {
        Get(bundleName, assetName, (prefab) => Object.Instantiate(prefab, parent), onInstaniated);
    }

    /// <summary>
    /// 获取预设体实例，并设置其父对象
    /// </summary>
    /// <param name="bundleName">预设体所在文件夹相对Editor文件夹的路径 或者 预设体所在的AB包的名称</param>
    /// <param name="assetName">预设体名称</param>
    /// <param name="parent">预设体实例的父对象</param>
    /// <param name="worldPositionStays">是否保持预设体在世界坐标系下的位置</param>
    /// <param name="onInstaniated">预设体实例化后执行的逻辑</param>
    public void Get(string bundleName, string assetName, Transform parent, bool instantiateInWorldSpace, UnityAction<GameObject> onInstaniated = null)
    {
        Get(bundleName, assetName, (prefab) => Object.Instantiate(prefab, parent, instantiateInWorldSpace), onInstaniated);
    }

    /// <summary>
    /// 获取预设体实例，并设置其在世界坐标下的位置和旋转
    /// </summary>
    /// <param name="bundleName">预设体所在文件夹相对Editor文件夹的路径 或者 预设体所在的AB包的名称</param>
    /// <param name="assetName">预设体名称</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <param name="onInstaniated">预设体实例化后执行的逻辑</param>
    public void Get(string bundleName, string assetName, Vector3 position, Quaternion rotation, UnityAction<GameObject> onInstaniated = null)
    {
        Get(bundleName, assetName, (prefab) => Object.Instantiate(prefab, position, rotation), onInstaniated);
    }

    /// <summary>
    /// 获取预设体实例，并设置其父对象和在世界坐标下的位置和旋转(实例的缩放变为相对父对象的)
    /// </summary>
    /// <param name="bundleName">预设体所在文件夹相对Editor文件夹的路径 或者 预设体所在的AB包的名称</param>
    /// <param name="assetName">预设体名称</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <param name="onInstaniated">预设体实例化后执行的逻辑</param>
    public void Get(string bundleName, string assetName, Vector3 position, Quaternion rotation, Transform parent, UnityAction<GameObject> onInstaniated = null)
    {
        Get(bundleName, assetName, (prefab) => Object.Instantiate(prefab, position, rotation, parent), onInstaniated);
    }
    #endregion

    /// <summary>
    /// 回收游戏对象
    /// </summary>
    /// <param name="obj">对象实例</param>
    public void Restore(GameObject obj)
    {
        if (pool.ContainsKey(obj.name))
            pool[obj.name].Restore(obj);
    }

    /// <summary>
    /// 移除所有缓存的游戏对象
    /// </summary>
    public void Clear()
    {
        foreach (var item in pool.Values)
        {
            item.toBeDeleted = true;
        }
        pool.Clear();
    }

    /// <summary>
    /// 回收所有子物体
    /// </summary>
    /// <param name="parent"></param>
    public void RestoreAllChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Restore(child.gameObject);
        }
    }
}
