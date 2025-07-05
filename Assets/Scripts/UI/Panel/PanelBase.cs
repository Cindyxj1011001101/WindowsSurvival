using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public abstract class PanelBase : MonoBehaviour
{
    /// <summary>
    /// ��ʾ��ʽ
    /// </summary>
    public enum ShowMode
    {
        Fade, // ���뵭��
        Animator // ��������
    }

    /// <summary>
    /// �������пؼ���͸���ȣ����뵭��ʱ�õ�
    /// </summary>
    private CanvasGroup canvasGroup;

    /// <summary>
    /// ���뵭��Ч���ĳ���ʱ��
    /// </summary>
    private float fadeTime = 0.1f;

    private Animator animator;

    /// <summary>
    /// �����ȫ��ʾ��ִ��
    /// </summary>
    public UnityEvent onShown { get; private set; } = new UnityEvent();
    /// <summary>
    /// �����ȫ���غ�ִ��
    /// </summary>
    public UnityEvent onHidden { get; private set; } = new UnityEvent();

    protected virtual void Awake()
    {
        // ��ȡ�ر����
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
    /// ��ʼ��������һ����������д�ؼ������¼���ע��
    /// </summary>
    protected abstract void Init();

    /// <summary>
    /// ��ʾ���
    /// </summary>
    /// <param name="onFinished">�����ȫ��ʾ�Ժ�ִ�е��߼�</param>
    public virtual void Show(ShowMode showMode = ShowMode.Fade, UnityAction onFinished = null)
    {
        if (onFinished != null)
            onShown.AddListener(onFinished);
        // �Ե��뵭����ʽ�򶯻���ʽ��ʾ���
        switch (showMode)
        {
            case ShowMode.Fade:
                // PublicMonobBehaviour��һ���̳�MonoBehaviour�Ľű�
                // �������������ڳ�������Զ����
                // ʹ������ִ��Э����Ϊ�˱�֤Э�̵�ִ�в�����Ϊ��������ٶ�ֹͣ
                PublicMonobBehaviour.Instance.StartCoroutine(FadeIn());
                break;
            case ShowMode.Animator:
                PublicMonobBehaviour.Instance.StartCoroutine(PlayAnimatorShow());
                break;
        }
    }

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="onFinished">�����ȫ�����Ժ�ִ�е��߼�</param>
    public virtual void Hide(ShowMode showMode = ShowMode.Fade, UnityAction onFinished = null)
    {
        if (onFinished != null)
            onHidden.AddListener(onFinished);
        // �Ե��뵭����ʽ�򶯻���ʽ�������
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
    /// ��嵭��
    /// </summary>
    private IEnumerator FadeIn()
    {
        // �����͸��������Ϊ0
        canvasGroup.alpha = 0;

        // ����fadeTimeʱ�䣬����͸��������Ϊ1
        float fadeSpeed = 1f / fadeTime;
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += fadeSpeed * Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1;

        // ���������ȫ��ʾ��Ļص�
        onShown?.Invoke();
        onShown.RemoveAllListeners();
    }

    /// <summary>
    /// ��嵭��
    /// </summary>
    private IEnumerator FadeOut()
    {
        // �����͸��������Ϊ1
        canvasGroup.alpha = 1;

        // ����fadeTimeʱ�䣬����͸���ȼ���Ϊ0
        float fadeSpeed = 1f / fadeTime;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }

        // ���������ȫ���غ�Ļص�
        onHidden?.Invoke();
        onHidden.RemoveAllListeners();
    }

    /// <summary>
    /// �������������ʾ
    /// </summary>
    private IEnumerator PlayAnimatorShow()
    {
        // ��֤����͸������1
        canvasGroup.alpha = 1;

        // �ȴ������������
        if (animator != null)
        {
            // ������Ҫ��֤����������Animator�ű�
            // ������Show�����������ʾ�Ķ���
            // ��Hide������������صĶ���
            animator.ResetTrigger("Hide");
            animator.SetTrigger("Show");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
        yield return null; // �ٵȴ�һ֡����֤�ȶ�

        // ���������ȫ��ʾ��Ļص�
        onShown?.Invoke();
        onShown.RemoveAllListeners();
    }

    /// <summary>
    /// ���������������
    /// </summary>
    private IEnumerator PlayAnimatorHide()
    {
        // �ȴ������������
        if (animator != null)
        {
            animator.ResetTrigger("Show");
            animator.SetTrigger("Hide");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
        yield return null; // �ٵȴ�һ֡����֤�ȶ�

        // ���������ȫ���غ�Ļص�
        onHidden?.Invoke();
        onHidden.RemoveAllListeners();
    }
}
