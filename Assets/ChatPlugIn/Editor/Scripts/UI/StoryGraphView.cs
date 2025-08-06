using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    public class StoryGraphView : GraphView
    {
        //关联窗口
        private StoryEditorWindow storyEditorWindow;
        //构造器
        public StoryGraphView(StoryEditorWindow storyEditorWindow)
        {
            this.storyEditorWindow = storyEditorWindow;
            AddGridBackground();
            AddManipulators();
        }
        //添加网格背景 
        private void AddGridBackground()
        {
            //实例化网格背景
             GridBackground gridBackground = new GridBackground();
             //将网格背景尺寸 拉伸至与视图相同
             gridBackground.StretchToParentSize();
             //将网格背景插入视图最底层
             Insert(0, gridBackground);
        }
        //添加视图操作组件
        private void AddManipulators()
        {
            //添加视图缩放组件
            this.AddManipulator(new ContentZoomer());
            SetupZoom(0.2f,2.0f);
            //添加视图拖拽组件
            this.AddManipulator(new ContentDragger());
            //添加选中对象拖拽组件
            this.AddManipulator(new SelectionDragger());
            //添加框选组件
            this.AddManipulator(new RectangleSelector());
        }
    }
}

