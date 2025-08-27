using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MySceneManager
{
    public static void LoadScene(int sceneBuildIndex)
    {
        // 停止所有DOTween动画
        DOTween.KillAll();
        // 清空对象池
        ObjectBufferPool.Instance.Clear();
        // 卸载未使用的资源
        ResourcesManager.Instance.UnloadUnusedAssets(() => PublicMono.Instance.StartCoroutine(LoadSceneAsync(sceneBuildIndex))); // 卸载完成后异步加载场景
    }

    private static IEnumerator LoadSceneAsync(int sceneBuildIndex)
    {
        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneBuildIndex);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}