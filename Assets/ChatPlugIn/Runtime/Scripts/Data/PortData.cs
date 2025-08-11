using System;
using UnityEngine;

namespace ChatPlugIn
{
    [Serializable]
    public class PortData
    {
        public string PortName;
        public string PortCondition;
        public PortData(string portName, string portCondition)
        {
            PortName = portName;
            PortCondition = portCondition;
        }

        public PortData(string portName)
        {
            PortName = portName;
            PortCondition ="";
        }
    }
}