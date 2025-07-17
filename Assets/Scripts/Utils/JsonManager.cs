using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public static class JsonManager
{
    static JsonSerializerSettings settings = new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Auto, // 存储类型信息
        Formatting = Formatting.Indented
    };

    /// <summary>
    /// 保存数据到 Application.persistentDataPath
    /// </summary>
    /// <param name="data">要保存的数据</param>
    /// <param name="fileName">文件名</param>
    public static void SaveData(object data, string fileName)
    {
        // 确定存储路径
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        // 序列化
        string json = SerializeObject(data);
        // 将序列化后的字符串写入指定路径的文件中
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// 加载数据 (优先从Application.persistentDataPath中加载，其次从Application.streamingAssetsPath中加载)
    /// </summary>
    /// <typeparam name="T">要加载的数据的类型</typeparam>
    /// <param name="fileName">文件名</param>
    /// <returns>反序列化后的数据类</returns>
    public static T LoadData<T>(string fileName) where T : new()
    {
        // 确定读取路径
        // 先判断persistentDataPath中是否有文件
        string path = Application.persistentDataPath + "/" + fileName + ".json";

        // 再判断persistentDataPath中是否有文件
        if (!File.Exists(path))
            path = Application.streamingAssetsPath + "/" + fileName + ".json";

        // 都没有就返回默认值
        if (!File.Exists(path))
            return new T();

        // 反序列化
        string json = File.ReadAllText(path);

        return DeserializeObject<T>(json);
    }

    public static string SerializeObject(object obj)
    {
        return JsonConvert.SerializeObject(obj, settings);
    }

    public static T DeserializeObject<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, settings);
    }

    public static T DeepCopy<T>(T obj)
    {
        return DeserializeObject<T>(SerializeObject(obj));
    }
}
