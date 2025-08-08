using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChatPlugIn
{
    [Serializable]
    public class ParagraphData
    {
        [SerializeField]private string paragraphName;
        [SerializeField]private string paragraphCondition;
        [SerializeField]private float paragraphPriority;
        [SerializeField]private List<SentenceData> sentenceDatas;

        public string ParagraphName
        {
            get { return paragraphName; }
            set { paragraphName = value; }
        }
        public string ParagraphCondition
        {
            get { return paragraphCondition; }
            set { paragraphCondition = value; }
        }

        public float ParagraphPriority
        {
            get { return paragraphPriority; }
            set { paragraphPriority = value; }
        }
        public List<SentenceData> SentenceDatas
        {
            get { return sentenceDatas; }
            set { sentenceDatas = value; }
        }
        public ParagraphData(string paragraphName, string paragraphCondition,float paragraphPriority, List<SentenceData> sentenceDatas)
        { 
            this.paragraphName = paragraphName;
            this.paragraphCondition = paragraphCondition;
            this.paragraphPriority = paragraphPriority;
            this.sentenceDatas = sentenceDatas;
        }
    }
}