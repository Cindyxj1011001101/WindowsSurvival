using UnityEngine;
using UnityEngine.UI;

public class StateToggle : MonoBehaviour
{
    public Text stateNameText;
    public Image offImage;
    public Image onImage;

    public Color offColor;

    public void SetOn(string stateName, bool value)
    {
        stateNameText.text = stateName;
        onImage.gameObject.SetActive(value);
        offImage.color = stateNameText.color = value ? Color.white : offColor;
    }
}