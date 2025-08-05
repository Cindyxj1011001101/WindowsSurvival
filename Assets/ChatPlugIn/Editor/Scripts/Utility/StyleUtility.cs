using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace ChatPlugIn
{
    public static class StyleUtility
    {
        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (string item in classNames)
            {
                element.AddToClassList(item);
            }

            return element;
        }

        public static VisualElement AddStyleSheets(this VisualElement element, params string[] filePath)
        {
            foreach (string item in filePath)
            {
                //载入文件
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(item);
                //添加引用
                element.styleSheets.Add(styleSheet);
            }
            return element;
        }
    }
}