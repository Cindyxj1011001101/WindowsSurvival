using UnityEngine;

public class SingleScriptableObject<T> : ScriptableObject where T :ScriptableObject
{
    //所有数据资源文件都放在Resources文件夹下加载对应的数据资源文件
    //对需要复用的唯一的数据资源文件名定一个规则：文件名和类名一致
    private static string scriptableObjectPath = "ScriptableObject/"+typeof(T).Name;
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                //如果为空，首先应该去资源路径下加载对应的数据资源文件
                instance = Resources.Load<T>(scriptableObjectPath);
            }
            //如果没有这个文件，直接创建一个数据
            if (instance == null)
            {
                instance = CreateInstance<T>();
            }
            return instance;
        }      
    }
}