using UnityEditor;
using UnityEngine;

namespace ChatPlugIn
{
    /// <summary>
    /// GraphData资源编辑器
    /// 实现双击资源文件自动打开对话编辑器并加载该图表
    /// </summary>
    public class GraphDataAssetEditor
    {
        /// <summary>
        /// 当双击资源文件时调用
        /// Unity的OnOpenAsset特性用于处理资源双击事件
        /// </summary>
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            // 获取被双击的资源对象
            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);
            
            // 检查是否是GraphData类型
            if (obj is GraphData graphData)
            {
                // 获取图表文件名（从资源路径中提取）
                string assetPath = AssetDatabase.GetAssetPath(instanceID);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                
                // 打开对话编辑器窗口
                StoryEditorWindow wnd = EditorWindow.GetWindow<StoryEditorWindow>();
                wnd.titleContent = new GUIContent("对话编辑器");
                
                // 等待窗口初始化完成后再加载图表
                // 使用EditorApplication.delayCall确保UI已创建
                EditorApplication.delayCall += () =>
                {
                    StoryEditorWindow.SetFileName(fileName);
                    wnd.LoadGraph(fileName);
                };
                
                return true; // 表示已处理该资源打开事件
            }
            
            return false; // 让Unity使用默认方式打开其他资源
        }
    }
}

