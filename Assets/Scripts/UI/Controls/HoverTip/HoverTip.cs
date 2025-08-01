using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HoverTip : MonoBehaviour
{
    public Text descText;
    public Text timeText;
    public GameObject forPlayer;
    public GameObject forEnvironment;

    public VerticalLayoutGroup verticalLayout;
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
    }

    public void SetEvent(Event e, bool interactable)
    {
        // 显示描述
        descText.gameObject.SetActive(!string.IsNullOrEmpty(e.description));
        if (!interactable)
        {
            descText.text = e.hint;
            // 更新高度
            MonoUtility.UpdateVerticalLayoutSize(verticalLayout);
            return;
        }

        descText.text = e.description;

        // 显示时间
        timeText.transform.parent.gameObject.SetActive(e.Time > 0);
        timeText.text = $"{e.Time}min";

        // 玩家状态变化
        forPlayer.SetActive(e.PlayerStateDict.Count > 0);

        foreach (var (type, delta) in e.PlayerStateDict)
        {
            if (StateManager.Instance.PlayerStateDict.TryGetValue(type, out var state))
            {
                var stateTip = transform.Find($"P_{type}").GetComponent<UIStateTip>();
                stateTip.SetValue(state, delta);
                stateTip.gameObject.SetActive(true);
            }
        }

        // 环境状态变化
        forEnvironment.SetActive(e.EnvironmentStateDict.Count > 0);

        foreach (var (type, delta) in e.EnvironmentStateDict)
        {
            if (GameManager.Instance.CurEnvironmentBag.StateDict.TryGetValue(type, out var state))
            {
                var stateTip = transform.Find($"E_{type}").GetComponent<UIStateTip>();
                stateTip.SetValue(state, delta);
                stateTip.gameObject.SetActive(true);
            }
        }

        // 更新高度
        MonoUtility.UpdateVerticalLayoutSize(verticalLayout);
    }

    public void Show()
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(1, 0.1f).SetEase(Ease.OutQuad);
    }

    public void Hide()
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(0, 0.1f).SetEase(Ease.OutQuad);
    }
}