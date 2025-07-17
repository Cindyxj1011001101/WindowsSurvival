using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RepeatableDropListConfigurator))]
public class RepeatableDropListConfiguratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认 inspector 内容
        DrawDefaultInspector();

        // 添加间距
        EditorGUILayout.Space(10);

        // 获取目标脚本引用
        RepeatableDropListConfigurator configurator = (RepeatableDropListConfigurator)target;

        // 添加按钮
        if (GUILayout.Button("Generate Disposable Drop List", GUILayout.Height(30)))
        {
            configurator.GenerateRepeatableDropList();
            Debug.Log("Disposable drop list generated successfully!");
        }
    }
}