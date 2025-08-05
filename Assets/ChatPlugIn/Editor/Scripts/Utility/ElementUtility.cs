using System;
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
        public static Foldout CreateFoldout(string title,bool collapsed=false)
        {
            Foldout foldout = new()
            {
                text = title,
                value = !collapsed
            };
            return foldout;
        }
    }

}