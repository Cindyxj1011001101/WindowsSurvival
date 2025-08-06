using System;
using UnityEditor.Experimental.GraphView;
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
        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textArea = CreateTextField(value, label, onValueChanged);
            textArea.multiline = true;
            return textArea;
        }
        
        public static Port CreatePort(this BaseNode node,string portName="",Orientation orientation=Orientation.Horizontal,Direction direction=Direction.Output,Port.Capacity capacity=Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
            port.portName = portName;
            return port;
        }

    }

}