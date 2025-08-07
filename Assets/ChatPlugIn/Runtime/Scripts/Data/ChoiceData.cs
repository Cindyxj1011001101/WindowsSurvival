using System;
using UnityEngine;

namespace ChatPlugIn
{
    [Serializable]
    public class ChoiceData
    {
        [SerializeField] private string text;
        [SerializeField] private string nextNodeID;
        public string Text { get => text; set => text = value; }
        public string NextNodeID { get => nextNodeID; set => nextNodeID = value; }
        public ChoiceData(string text)
        {
            this.text = text;
        }
    }
}