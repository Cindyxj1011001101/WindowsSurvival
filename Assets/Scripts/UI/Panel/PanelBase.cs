using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public abstract class PanelBase : MonoBehaviour
{
    /// <summary>
    /// 面板显示方式：淡入或动画器控制
    /// </summary>
    public enum ShowMode
    {
        Fade,      // 淡入效果
        Animator   // 使用Animator控制
    }

    /// <summary>
    /// 控制面板透明度的CanvasGroup组件
    /// </summary>
    protected CanvasGroup canvasGroup;

    /// <summary>
    /// 淡入/淡出时间
    /// </summary>
    private float fadeTime = 0.1f;

    private Animator animator;

    /// <summary>
    /// 当面板显示完成时触发的事件
    /// </summary>
    public UnityEvent onShown { get; private set; } = new UnityEvent();

    /// <summary>
    /// 当面板隐藏完成时触发的事件
    /// </summary>
    public UnityEvent onHidden { get; private set; } = new UnityEvent();

    protected virtual void Awake()
    {
        // 获取CanvasGroup组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        Init();
    }

    public virtual void SetStartButton(GameObject button) { }

    protected abstract void Init();

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <param name="showMode">显示方式</param>
    /// <param name="onFinished">显示完成后回调</param>
    public virtual void Show(ShowMode showMode = ShowMode.Fade, UnityAction onFinished = null)
    {
        // 注册完成回调
        if (onFinished != null)
            onShown.AddListener(onFinished);

        //canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        onShown.AddListener(() =>
        {
            canvasGroup.interactable = true;
        });

        // 根据模式启动协程
        switch (showMode)
        {
            case ShowMode.Fade:
                PublicMonoBehaviour.Instance.StartCoroutine(FadeIn());
                break;
            case ShowMode.Animator:
                PublicMonoBehaviour.Instance.StartCoroutine(PlayAnimatorShow());
                break;
        }
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <param name="showMode">隐藏方式</param>
    /// <param name="onFinished">隐藏完成后回调</param>
    public virtual void Hide(ShowMode showMode = ShowMode.Fade, UnityAction onFinished = null)
    {
        if (onFinished != null)
            onHidden.AddListener(onFinished);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        onHidden.AddListener(() =>
        {
            canvasGroup.blocksRaycasts = false;
        });

        switch (showMode)
        {
            case ShowMode.Fade:
                PublicMonoBehaviour.Instance.StartCoroutine(FadeOut());
                break;
            case ShowMode.Animator:
                PublicMonoBehaviour.Instance.StartCoroutine(PlayAnimatorHide());
                break;
        }
    }

    /// <summary>
    /// 淡入动画协程
    /// </summary>
    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0;
        float fadeSpeed = 1f / fadeTime;

        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += fadeSpeed * Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1;

        onShown?.Invoke();
        onShown.RemoveAllListeners();
    }

    /// <summary>
    /// 淡出动画协程
    /// </summary>
    private IEnumerator FadeOut()
    {
        canvasGroup.alpha = 1;
        float fadeSpeed = 1f / fadeTime;

        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }

        onHidden?.Invoke();
        onHidden.RemoveAllListeners();
    }

    /// <summary>
    /// 使用Animator播放显示动画
    /// </summary>
    private IEnumerator PlayAnimatorShow()
    {
        canvasGroup.alpha = 1;

        if (animator != null)
        {
            animator.ResetTrigger("Hide");
            animator.SetTrigger("Show");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }

        yield return null;

        onShown?.Invoke();
        onShown.RemoveAllListeners();
    }

    /// <summary>
    /// 使用Animator播放隐藏动画
    /// </summary>
    private IEnumerator PlayAnimatorHide()
    {
        canvasGroup.alpha = 1;

        if (animator != null)
        {
            animator.ResetTrigger("Show");
            animator.SetTrigger("Hide");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }

        yield return null;

        onHidden?.Invoke();
        onHidden.RemoveAllListeners();
    }
}
