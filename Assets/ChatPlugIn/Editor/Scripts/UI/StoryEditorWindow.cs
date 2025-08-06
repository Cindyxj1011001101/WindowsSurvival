using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    public class StoryEditorWindow : EditorWindow
    {
        //只读字段
        private readonly string defaultFileWindow = "新段落";
        private readonly string variablesPath ="Assets/ChatPlugIn/Editor/StyleSheets/Variables.uss";
        private readonly string toolbarStylePath ="Assets/ChatPlugIn/Editor/StyleSheets/ToolbarStyle.uss";
        private readonly string graphViewStylePath ="Assets/ChatPlugIn/Editor/StyleSheets/GraphViewStyle.uss";
        //关联的视图
        private StoryGraphView storyGraphView;
        //UI元素
        private Toolbar toolbar;//工具栏
        private static TextField tfdFileName;//文件名输入框
        private Button btnSave;
        private Button btnOpen;
        private Button btnNew;
        private Button btnClear;
        private Button btnMiniMap;
        
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
            AddToolbar();
            AddGraphView();
            AddStyles();
        }

        private void AddToolbar()
        {
            tfdFileName = ElementUtility.CreateTextField(defaultFileWindow, "当前段落名", null);
            btnSave= ElementUtility.CreateButton("保存", null);
            btnOpen = ElementUtility.CreateButton("打开", null);
            btnNew = ElementUtility.CreateButton("新建", null);
            btnClear = ElementUtility.CreateButton("清空", null);
            btnMiniMap = ElementUtility.CreateButton("小地图", null);
            //创建工具栏
            toolbar = new();
            //将UI元素加入工具栏
            toolbar.Add(tfdFileName);
            toolbar.Add(btnSave);
            toolbar.Add(btnOpen);
            toolbar.Add(btnNew);
            toolbar.Add(btnClear);
            toolbar.Add(btnMiniMap);
            //将工具栏加入窗口
            rootVisualElement.Add(toolbar);
        }
        //添加视图
        private void AddGraphView()
        {
            //实例化视图类
            storyGraphView = new StoryGraphView(this);
            //将视图加入窗口
            rootVisualElement.Add(storyGraphView);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets(variablesPath);
            toolbar.AddStyleSheets(toolbarStylePath);
            storyGraphView.AddStyleSheets(graphViewStylePath);
        }
    }
}

