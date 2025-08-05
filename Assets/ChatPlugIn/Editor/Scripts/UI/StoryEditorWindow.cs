using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    public class StoryEditorWindow : EditorWindow
    {
        [MenuItem("ChatPlugIn/打开对话编辑器")]
        public static void OpenEditorWindow()
        {
            //获取窗口
            StoryEditorWindow wnd = GetWindow<StoryEditorWindow>();
            //修改窗口标题
            wnd.titleContent = new GUIContent("对话编辑器");
        }

        private void CreateGUI()
        {
            //获取窗口UI元素
            VisualElement root = rootVisualElement;
            //创建标签
            VisualElement label = new Label("Hello World! From C#");
            //标签放入根元素
            root.Add(label);

        }
    }
}

