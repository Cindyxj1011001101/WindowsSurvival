using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


namespace ChatPlugIn
{
    public static class ElementUtility
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new(onClick)
            {
                text = text,
            };
            return button;
        }
        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new()
            {
                text = title,
                value = !collapsed
            };
            return foldout;
        }
        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new()
            {
                value = value,
                label = label,
            };
            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }
            return textField;
        }

        public static FloatField CreateFloatField(float value = 0f, string label = null, EventCallback<ChangeEvent<float>> onValueChanged = null)
        {
            FloatField floatField = new FloatField()
            {
                value = value,
                label = label,
            };
    
            if (onValueChanged != null)
            {
                floatField.RegisterValueChangedCallback(onValueChanged);
            }
    
            return floatField;
        }
        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textArea = CreateTextField(value, label, onValueChanged);
            textArea.style.width = 300;
            textArea.style.whiteSpace = WhiteSpace.Normal;
            textArea.multiline = true;
            if (onValueChanged != null)
            {
                textArea.RegisterValueChangedCallback(onValueChanged);
            }
            return textArea;
        }
        
        
        public static Port CreatePort(this BaseNode node,string portName="",Orientation orientation=Orientation.Horizontal,Direction direction=Direction.Output,Port.Capacity capacity=Port.Capacity.Multi)
        {
            Port port = node.InstantiatePort(orientation, (UnityEditor.Experimental.GraphView.Direction)direction, capacity, typeof(bool));
            port.portName = portName;
            return port;
        }
        
        public static DropdownField CreateEnumDropdown<T>(T value, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null) where T :Enum
        {
            // 获取枚举所有值
            var choices = Enum.GetNames(typeof(T));

            // 创建下拉菜单
            var dropdown = new DropdownField(typeof(T).ToString(), choices.ToList(), 0);
            
            dropdown.value = value.ToString();
            //设置当前显示值

            if (onValueChanged != null)
            {
                dropdown.RegisterValueChangedCallback(onValueChanged);
            }
            return dropdown;
        }

    }

}