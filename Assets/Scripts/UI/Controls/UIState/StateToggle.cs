using UnityEngine;
using UnityEngine.UI;

public class StateToggle : MonoBehaviour
{
    public Text stateNameText;
    public Image offImage;
    public Image onImage;

    public Color onColor;
    public Color offColor;

    public void SetStateName(string name)
    {
        stateNameText.text = name;
    }

    public void SetValue(bool value)
    {
        onImage.gameObject.SetActive(value);
        onImage.color = offImage.color = stateNameText.color = value ? onColor : offColor;
    }
}