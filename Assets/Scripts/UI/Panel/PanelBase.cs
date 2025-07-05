using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public abstract class PanelBase : MonoBehaviour
{
    /// <summary>
    /// 显示方式
    /// </summary>
    public enum ShowMode
    {
        Fade, // 淡入淡出
        Animator // 动画控制
    }

    /// <summary>
    /// 控制所有控件的透明度，淡入淡出时用到
    /// </summary>
    private CanvasGroup canvasGroup;

    /// <summary>
    /// 淡入淡出效果的持续时间
    /// </summary>
    private float fadeTime = 0.1f;

    private Animator animator;

    /// <summary>
    /// 面板完全显示后执行
    /// </summary>
    public UnityEvent onShown { get; private set; } = new UnityEvent();
    /// <summary>
    /// 面板完全隐藏后执行
    /// </summary>
    public UnityEvent onHidden { get; private set; } = new UnityEvent();

    protected virtual void Awake()
    {
        // 获取必备组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
        }
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Init();
    }

    /// <summary>
    /// 初始化方法，一般在里面书写控件交互事件的注册
    /// </summary>
    protected abstract void Init();

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <param name="onFinished">面板完全显示以后执行的逻辑</param>
    public virtual void Show(ShowMode showMode = ShowMode.Fade, UnityAction onFinished = null)
    {
        if (onFinished != null)
            onShown.AddListener(onFinished);
        // 以淡入淡出方式或动画方式显示面板
        switch (showMode)
        {
            case ShowMode.Fade:
                // PublicMonobBehaviour是一个继承MonoBehaviour的脚本
                // 挂载它的物体在场景中永远存在
                // 使用它来执行协程是为了保证协程的执行不会因为物体的销毁而停止
                PublicMonobBehaviour.Instance.StartCoroutine(FadeIn());
                break;
            case ShowMode.Animator:
                PublicMonobBehaviour.Instance.StartCoroutine(PlayAnimatorShow());
                break;
        }
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <param name="onFinished">面板完全隐藏以后执行的逻辑</param>
    public virtual void Hide(ShowMode showMode = ShowMode.Fade, UnityAction onFinished = null)
    {
        if (onFinished != null)
            onHidden.AddListener(onFinished);
        // 以淡入淡出方式或动画方式隐藏面板
        switch (showMode)
        {
            case ShowMode.Fade:
                PublicMonobBehaviour.Instance.StartCoroutine(FadeOut());
                break;
            case ShowMode.Animator:
                PublicMonobBehaviour.Instance.StartCoroutine(PlayAnimatorHide());
                break;
        }
    }

    /// <summary>
    /// 面板淡入
    /// </summary>
    private IEnumerator FadeIn()
    {
        // 将面板透明度设置为0
        canvasGroup.alpha = 0;

        // 经过fadeTime时间，面板的透明度增长为1
        float fadeSpeed = 1f / fadeTime;
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += fadeSpeed * Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1;

        // 调用面板完全显示后的回调
        onShown?.Invoke();
        onShown.RemoveAllListeners();
    }

    /// <summary>
    /// 面板淡出
    /// </summary>
    private IEnumerator FadeOut()
    {
        // 将面板透明度设置为1
        canvasGroup.alpha = 1;

        // 经过fadeTime时间，面板的透明度减少为0
        float fadeSpeed = 1f / fadeTime;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }

        // 调用面板完全隐藏后的回调
        onHidden?.Invoke();
        onHidden.RemoveAllListeners();
    }

    /// <summary>
    /// 动画控制面板显示
    /// </summary>
    private IEnumerator PlayAnimatorShow()
    {
        // 保证面板的透明度是1
        canvasGroup.alpha = 1;

        // 等待动画播放完毕
        if (animator != null)
        {
            // 这里需要保证该面板挂载了Animator脚本
            // 并且由Show来触发面板显示的动画
            // 由Hide来触发面板隐藏的动画
            animator.ResetTrigger("Hide");
            animator.SetTrigger("Show");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
        yield return null; // 再等待一帧，保证稳定

        // 调用面板完全显示后的回调
        onShown?.Invoke();
        onShown.RemoveAllListeners();
    }

    /// <summary>
    /// 动画控制面板隐藏
    /// </summary>
    private IEnumerator PlayAnimatorHide()
    {
        // 等待动画播放完毕
        if (animator != null)
        {
            animator.ResetTrigger("Show");
            animator.SetTrigger("Hide");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
        yield return null; // 再等待一帧，保证稳定

        // 调用面板完全隐藏后的回调
        onHidden?.Invoke();
        onHidden.RemoveAllListeners();
    }
}
