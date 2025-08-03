using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTip : MonoBehaviour
{
    public Text tipText;
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = canvasGroup.interactable = false;
    }

    public void ShowTip(string tip, Vector3 position, float duration = 1f)
    {
        tipText.text = tip;
        Show(position, duration);
    }

    private void Show(Vector3 position, float duration)
    {
        transform.position = position - new Vector3(position.x, position.y + 100, position.z);

        var seq = DOTween.Sequence();

        seq.Join(transform.DOMove(position, 0.2f).SetEase(Ease.OutQuad));
        seq.Join(canvasGroup.DOFade(1, 0.2f));
        seq.AppendInterval(duration);
        seq.Append(canvasGroup.DOFade(0, 0.2f).OnComplete(() => Destroy(gameObject)));

        seq.Play();
    }
}