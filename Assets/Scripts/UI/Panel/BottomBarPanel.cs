using UnityEngine;
using UnityEngine.UI;

public class BottomBarPanel : PanelBase
{
    [SerializeField] private Button m_backpackButton;

    protected override void Init()
    {
        m_backpackButton.onClick.AddListener(OnBackpackButtonCLick);
    }

    private void OnBackpackButtonCLick()
    {

    }
}