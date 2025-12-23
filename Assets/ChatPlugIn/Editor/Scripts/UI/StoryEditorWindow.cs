using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private DropdownField dropdownGraphList;//图表列表下拉框
        private Button btnSave;
        private Button btnOpen;
        private Button btnNew;
        private Button btnMinimize; // 最小化按钮
        // private Button btnClear; // 已注释：清空功能已禁用
        private Button btnMiniMap;
        
        private static StoryEditorWindow currentWindow; // 保存当前窗口实例
        
        [MenuItem("ChatPlugIn/打开对话编辑器")]
        public static void OpenEditorWindow()
        {
            //获取窗口（如果已存在则获取现有窗口，否则创建新窗口）
            StoryEditorWindow wnd = GetWindow<StoryEditorWindow>();
            currentWindow = wnd;
            //修改窗口标题
            wnd.titleContent = new GUIContent("对话编辑器");
            // 确保窗口可见
            wnd.Show();
            wnd.Focus();
            
            // 恢复之前保存的文件名（如果有）
            RestoreWindowState(wnd);
        }
        
        /// <summary>
        /// 恢复窗口状态
        /// </summary>
        private static void RestoreWindowState(StoryEditorWindow wnd)
        {
            // 只有在没有手动设置文件名时才恢复保存的状态
            // 这样可以避免双击资源文件打开时被覆盖
            if (tfdFileName != null && (string.IsNullOrEmpty(tfdFileName.value) || tfdFileName.value == "新段落"))
            {
                string savedFileName = EditorPrefs.GetString("StoryEditorWindow_FileName", "");
                if (!string.IsNullOrEmpty(savedFileName))
                {
                    tfdFileName.value = savedFileName;
                }
            }
        }
        
        /// <summary>
        /// 保存窗口状态
        /// </summary>
        private static void SaveWindowState()
        {
            if (tfdFileName != null && !string.IsNullOrEmpty(tfdFileName.value))
            {
                EditorPrefs.SetString("StoryEditorWindow_FileName", tfdFileName.value);
            }
        }

        /// <summary>
        /// 设置文件名（供外部调用，如双击资源时）
        /// </summary>
        public static void SetFileName(string fileName)
        {
            if (tfdFileName != null)
            {
                tfdFileName.value = fileName;
            }
        }

        /// <summary>
        /// 加载图表（供外部调用，如双击资源时）
        /// </summary>
        public void LoadGraph(string fileName)
        {
            if (storyGraphView != null)
            {
                storyGraphView.LoadGraph(fileName);
            }
        }

        private void CreateGUI()
        {
            AddToolbar();
            AddGraphView();
            AddStyles();
            // 窗口打开时刷新图表列表
            RefreshGraphList();
            // 恢复之前保存的状态（延迟执行，确保UI已创建）
            EditorApplication.delayCall += () => {
                RestoreWindowState(this);
            };
        }

        private void AddToolbar()
        {
            tfdFileName = ElementUtility.CreateTextField(defaultFileWindow, "当前段落名", null);
            
            // 创建图表列表下拉框
            List<string> graphFileNames = GetAllGraphFileNames();
            dropdownGraphList = ElementUtility.CreateDropdownField(
                graphFileNames, 
                null, 
                "快速选择", 
                (evt) => {
                    if (!string.IsNullOrEmpty(evt.newValue))
                    {
                        tfdFileName.value = evt.newValue;
                        LoadGraph();
                    }
                }
            );
            
            btnSave= ElementUtility.CreateButton("保存", () => SaveGraph());
            btnOpen = ElementUtility.CreateButton("打开", () => LoadGraph());
            btnNew = ElementUtility.CreateButton("新建", () => NewGraph());
            btnMinimize = ElementUtility.CreateButton("最小化", () => MinimizeWindow());
            // btnClear = ElementUtility.CreateButton("清空", () => ClearGraph()); // 已注释：清空功能已禁用
            //创建工具栏
            toolbar = new();
            //将UI元素加入工具栏
            toolbar.Add(tfdFileName);
            toolbar.Add(dropdownGraphList);
            toolbar.Add(btnSave);
            toolbar.Add(btnOpen);
            toolbar.Add(btnNew);
            toolbar.Add(btnMinimize);
            // toolbar.Add(btnClear); // 已注释：清空按钮已禁用
            //将工具栏加入窗口
            rootVisualElement.Add(toolbar);
        }

        /// <summary>
        /// 获取所有图表文件名列表
        /// </summary>
        private List<string> GetAllGraphFileNames()
        {
            List<string> fileNames = new List<string>();
            string directoryPath = "Assets/Resources/DialogueGraphs";
            
            if (Directory.Exists(directoryPath))
            {
                string[] assetPaths = Directory.GetFiles(directoryPath, "*.asset", SearchOption.TopDirectoryOnly);
                foreach (string assetPath in assetPaths)
                {
                    string fileName = Path.GetFileNameWithoutExtension(assetPath);
                    fileNames.Add(fileName);
                }
            }
            
            return fileNames.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// 刷新图表列表下拉框
        /// </summary>
        private void RefreshGraphList()
        {
            if (dropdownGraphList != null)
            {
                var graphFileNames = GetAllGraphFileNames();
                dropdownGraphList.choices = graphFileNames;
                // 如果当前文件名在列表中，设置为选中项
                if (!string.IsNullOrEmpty(tfdFileName?.value) && graphFileNames.Contains(tfdFileName.value))
                {
                    dropdownGraphList.value = tfdFileName.value;
                }
            }
        }

        #region 添加
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
        

        #endregion

        #region 按钮功能

        private void SaveGraph()
        {
            if (storyGraphView == null || string.IsNullOrEmpty(tfdFileName.value))
            {
                Debug.LogWarning("无法保存：图视图为空或文件名为空");
                return;
            }
            
            storyGraphView.SaveGraph(tfdFileName.value);
            // 保存后刷新图表列表
            RefreshGraphList();
        }
        // 加载图数据
        private void LoadGraph()
        {
            // ClearGraph(); // 已注释：清空功能已禁用
            if (!string.IsNullOrEmpty(tfdFileName.value))
            {
                storyGraphView.LoadGraph(tfdFileName.value);
            }
        }
        
        // 新建图
        private void NewGraph()
        {
            if (storyGraphView != null)
            {
                // ClearGraph(); // 已注释：清空功能已禁用
                tfdFileName.value = defaultFileWindow;
            }
        }

        /// <summary>
        /// 最小化窗口（关闭窗口，状态已保存，可通过菜单重新打开）
        /// </summary>
        private void MinimizeWindow()
        {
            // 保存当前状态
            SaveWindowState();
            // 关闭窗口
            Close();
        }
        
        /// <summary>
        /// 窗口关闭时保存状态
        /// </summary>
        private void OnDestroy()
        {
            SaveWindowState();
        }
        
        // 清空图（已禁用）
        // private void ClearGraph()
        // {
        //     if (storyGraphView != null)
        //     {
        //         storyGraphView.ClearGraph(tfdFileName.value);
        //     }
        // }
        #endregion
    }
}

