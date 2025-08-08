using System;
using UnityEngine;


namespace ChatPlugIn
{
    [Serializable]
    public enum RoleEnum
    {
        NPC,
        Player,
        Aside
    }
    [Serializable]
    public class SentenceData
    {
        [SerializeField]private RoleEnum role;
        [SerializeField]private string text;
        [SerializeField]private float waitTime;
        
        public RoleEnum Role {get=>role;set=>role=value; }
        public string Text {get=>text;set=>text=value; }
        public float WaitTime {get=>waitTime;set=>waitTime=value; }
        public SentenceData( RoleEnum role,string text, float waitTime)
        {
            this.role = role;
            this.text = text;
            this.waitTime = waitTime;
        }
    }
}