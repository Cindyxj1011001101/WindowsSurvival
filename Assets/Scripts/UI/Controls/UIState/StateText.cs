using UnityEngine;
using UnityEngine.UI;

public class StateText : MonoBehaviour
{
    public Text stateNameText;
    public Text valueText;

    public void SetStateName(string name)
    {
        stateNameText.text = name;
    }

    public void SetValue(string value)
    {
        valueText.text = value;
    }
}