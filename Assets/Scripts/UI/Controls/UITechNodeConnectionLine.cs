using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UITechNodeConnectionLine : MonoBehaviour
{
    [SerializeField] private RectTransform baseLayer;
    [SerializeField] private RectTransform fillMask;
    private Image image;
    private float oringinalFillMaskWidth;
    private float animTransition = 0.5f;

    private Color32 normalColor = new(179, 179, 179, 255);
    private Color32 lockedColor = new(52, 52, 52, 255);

    [SerializeField] private UITechNode successorNodeUI; // 连接线连接的后继科技节点
    private ScriptableTechnologyNode successorNode; // 后继科技节点
    private TechNodeState successorNodeState; // 后继科技节点的状态

    public void Init()
    {
        successorNode = TechnologyManager.Instance.GetTechNodeByName(successorNodeUI.name);
        successorNodeState = TechnologyManager.Instance.GetTechNodeState(successorNode);

        image = GetComponentInChildren<Image>();

        var rect = baseLayer.rect;
        baseLayer.anchorMin = new(0, 0.5f);
        baseLayer.anchorMax = new(0, 0.5f);
        baseLayer.pivot = new(0, 0.5f);
        baseLayer.sizeDelta = new(rect.width, rect.height);
        fillMask.sizeDelta = new(rect.width, rect.height);

        // 复制 baselayer
        Instantiate(baseLayer.gameObject, fillMask);

        fillMask.GetComponentInChildren<Image>().color = ColorManager.Cyan;
        
        oringinalFillMaskWidth = fillMask.sizeDelta.x;
        
        fillMask.sizeDelta = new(0, fillMask.sizeDelta.y);

        Display(false);
    }

    public void RefreshDisplay()
    {
        if (successorNode == null) return;

        var newState = TechnologyManager.Instance.GetTechNodeState(successorNode);
        if (newState == successorNodeState) return;

        successorNodeState = newState;

        Display(true);
    }

    private void Display(bool playAnim)
    {
        image.color = normalColor;
        fillMask.gameObject.SetActive(true);

        switch (successorNodeState)
        {
            case TechNodeState.Locked:
                fillMask.gameObject.SetActive(false);
                image.color = lockedColor;
                break;
            case TechNodeState.Complished:
                fillMask.sizeDelta = new(oringinalFillMaskWidth, fillMask.sizeDelta.y);
                break;
            case TechNodeState.BeingStudied:
                if (playAnim)
                    fillMask.DOSizeDelta(new(oringinalFillMaskWidth, fillMask.sizeDelta.y), animTransition);
                else
                    fillMask.sizeDelta = new(oringinalFillMaskWidth, fillMask.sizeDelta.y);
                break;
            default:
                if (playAnim)
                    fillMask.DOSizeDelta(new(0, fillMask.sizeDelta.y), animTransition);
                else
                    fillMask.sizeDelta = new(0, fillMask.sizeDelta.y);
                break;
        }
    }
}