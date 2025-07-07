// using ScritableObject;
// using UnityEngine;
// using UnityEditor;
// public class ScriptableObjectTool 
// {
//     [MenuItem("ScritableObject/CardData/CreateFoodCardData")]
//     public static void CreateFoodCardData()
//     {
//         //创建数据资源文件
//         //泛型是继承自ScriptableObject的类
//         FoodCardData asset = ScriptableObject.CreateInstance<FoodCardData>();
//         //前一步创建的资源只是存在内存中，现在要把它保存到本地
//         //通过编辑器API，创建一个数据资源文件，第二个参数为资源文件在Assets目录下的路径
//         AssetDatabase.CreateAsset(asset, "Assets/Resources/ScriptableObject/FoodCard/FoodCardData.asset");
//         //保存创建的资源
//         AssetDatabase.SaveAssets();
//         //刷新界面
//         AssetDatabase.Refresh();
//     }
//     [MenuItem("ScritableObject/CardData/CreateResourceCardData")]
//     public static void CreateResourceCardData()
//     {
//         //创建数据资源文件
//         //泛型是继承自ScriptableObject的类
//         ResourceCardData asset = ScriptableObject.CreateInstance<ResourceCardData>();
//         //前一步创建的资源只是存在内存中，现在要把它保存到本地
//         //通过编辑器API，创建一个数据资源文件，第二个参数为资源文件在Assets目录下的路径
//         AssetDatabase.CreateAsset(asset, "Assets/Resources/ScriptableObject/ResourceCard/ResourceCardData.asset");
//         //保存创建的资源
//         AssetDatabase.SaveAssets();
//         //刷新界面
//         AssetDatabase.Refresh();
//     }
//     [MenuItem("ScritableObject/CardData/CreateToolCardData")]
//     public static void CreateToolCardData()
//     {
//         //创建数据资源文件
//         //泛型是继承自ScriptableObject的类
//         ToolCardData asset = ScriptableObject.CreateInstance<ToolCardData>();
//         //前一步创建的资源只是存在内存中，现在要把它保存到本地
//         //通过编辑器API，创建一个数据资源文件，第二个参数为资源文件在Assets目录下的路径
//         AssetDatabase.CreateAsset(asset, "Assets/Resources/ScriptableObject/ToolCard/ToolCardData.asset");
//         //保存创建的资源
//         AssetDatabase.SaveAssets();
//         //刷新界面
//         AssetDatabase.Refresh();
//     }
//     [MenuItem("ScritableObject/CreateInitPlayerStateData")]
//     public static void CreateInitPlayerStateData()
//     {
//         //创建数据资源文件
//         //泛型是继承自ScriptableObject的类
//         InitPlayerStateData asset = ScriptableObject.CreateInstance<InitPlayerStateData>();
//         //前一步创建的资源只是存在内存中，现在要把它保存到本地
//         //通过编辑器API，创建一个数据资源文件，第二个参数为资源文件在Assets目录下的路径
//         AssetDatabase.CreateAsset(asset, "Assets/Resources/ScriptableObject/InitPlayerStateData.asset");
//         //保存创建的资源
//         AssetDatabase.SaveAssets();
//         //刷新界面
//         AssetDatabase.Refresh();
//     }
//     [MenuItem("ScritableObject/Bag/CreatePlayerBagData")]
//     public static void CreatePlayerBagData()
//     {
//         //创建数据资源文件
//         //泛型是继承自ScriptableObject的类
//         BagData asset = ScriptableObject.CreateInstance<BagData>();
//         //前一步创建的资源只是存在内存中，现在要把它保存到本地
//         //通过编辑器API，创建一个数据资源文件，第二个参数为资源文件在Assets目录下的路径
//         AssetDatabase.CreateAsset(asset, "Assets/Resources/ScriptableObject/Bag/BagData.asset");
//         //保存创建的资源
//         AssetDatabase.SaveAssets();
//         //刷新界面
//         AssetDatabase.Refresh();
//     }
//     [MenuItem("ScritableObject/EventTrigger/CreateValueEvent")]
//     public static void CreateValueEvent()
//     {
//         //创建数据资源文件
//         //泛型是继承自ScriptableObject的类
//         ValueEvent asset = ScriptableObject.CreateInstance<ValueEvent>();
//         //前一步创建的资源只是存在内存中，现在要把它保存到本地
//         //通过编辑器API，创建一个数据资源文件，第二个参数为资源文件在Assets目录下的路径
//         AssetDatabase.CreateAsset(asset, "Assets/Resources/ScriptableObject/EventTrigger/ValueEvent.asset");
//         //保存创建的资源
//         AssetDatabase.SaveAssets();
//         //刷新界面
//         AssetDatabase.Refresh();
//     }
//     [MenuItem("ScritableObject/EventTrigger/CreateDropEvent")]
//     public static void CreateDropEvent()
//     {
//         //创建数据资源文件
//         //泛型是继承自ScriptableObject的类
//         DropEvent asset = ScriptableObject.CreateInstance<DropEvent>();
//         //前一步创建的资源只是存在内存中，现在要把它保存到本地
//         //通过编辑器API，创建一个数据资源文件，第二个参数为资源文件在Assets目录下的路径
//         AssetDatabase.CreateAsset(asset, "Assets/Resources/ScriptableObject/EventTrigger/DropEvent.asset");
//         //保存创建的资源
//         AssetDatabase.SaveAssets();
//         //刷新界面
//         AssetDatabase.Refresh();
//     }
// }