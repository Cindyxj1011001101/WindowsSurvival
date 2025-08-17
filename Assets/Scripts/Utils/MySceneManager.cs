using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MySceneManager
{
    public static void LoadScene(int sceneBuildIndex)
    {
        PublicMono.Instance.StartCoroutine(LoadSceneAsync(sceneBuildIndex));
    }

    private static IEnumerator LoadSceneAsync(int sceneBuildIndex)
    {
        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneBuildIndex);

        // 禁止自动激活场景
        asyncLoad.allowSceneActivation = false;

        // 等待加载进度达到90%（Unity的加载机制）
        while (asyncLoad.progress < 0.9f)
        {
            Debug.Log($"加载进度: {asyncLoad.progress * 100}%");
            yield return null;
        }

        // 这里可以执行加载完成前的准备工作
        Debug.Log("场景已加载完毕，准备切换");

        // 手动激活场景
        asyncLoad.allowSceneActivation = true;

        // 等待场景完全激活
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log("场景切换完成");
    }
}