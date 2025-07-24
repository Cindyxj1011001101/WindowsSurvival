using System;
using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance => instance;

    private AudioSource audioSource;
    private AudioSource sfxSource; // 专用于音效
    private Coroutine fadeCoroutine;// 用于淡入淡出音频
    private float targetVolume;// 目标音量，用于淡入淡出

    private void Awake()
    {
        instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;


        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        GameDataManager.Instance.onBGMVolumeChanged.AddListener(OnBGMVolumeChanged);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        GameDataManager.Instance.onBGMVolumeChanged.RemoveListener(OnBGMVolumeChanged);
    }

    private void OnBGMVolumeChanged()
    {
        float newVolume = GetNormalizedBGMVolume();

        if (fadeCoroutine != null)
        {
            // 渐进式调整目标值（保持当前渐变进度）
            targetVolume = newVolume;
        }
        else
        {
            audioSource.volume = targetVolume = newVolume;
        }
    }





    public void PlayBGM(string clipName, bool loop = true, float fadeDuration = 1f, float volumeMultiplier = 1f)
    {
        var clip = GetClip(clipName, "Music");

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeSwitchBGM(clip, loop, fadeDuration, volumeMultiplier));
    }

    private IEnumerator FadeSwitchBGM(AudioClip clip, bool loop, float fadeDuration, float volumeMultiplier)
    {
        targetVolume = GetNormalizedBGMVolume();

        // 如果当前没有播放音乐，直接淡入新音乐
        if (!audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();

            // 淡入
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0f, targetVolume * volumeMultiplier, timer / fadeDuration);
                yield return null;
            }
            audioSource.volume = targetVolume * volumeMultiplier;
        }
        else
        {
            // 先淡出当前音乐
            float startVolume = audioSource.volume;
            float timer = 0f;

            while (timer < fadeDuration / 2f)
            {
                timer += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / (fadeDuration / 2f));
                yield return null;
            }

            // 切换音乐并淡入
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();

            timer = 0f;
            while (timer < fadeDuration / 2f)
            {
                timer += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0f, targetVolume * volumeMultiplier, timer / (fadeDuration / 2f));
                yield return null;
            }
            audioSource.volume = targetVolume * volumeMultiplier;
        }

        fadeCoroutine = null;
    }

    public void StopBGM(float fadeDuration = 1f)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeOutBGM(fadeDuration));
    }
    private IEnumerator FadeOutBGM(float fadeDuration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // 恢复原始音量设置，以便下次播放
        fadeCoroutine = null;
    }

    public void PlaySound(AudioClip clip, bool isRandom)
    {
        // 获取基础音量
        float baseVolume = GetNormalizedSFXVolume();
        if (isRandom == true)
        {
            // 随机音量波动（±10%）
            float volumeVariation = 1f + UnityEngine.Random.Range(-0.1f, 0.1f);
            float finalVolume = Mathf.Clamp(baseVolume * volumeVariation, 0f, 1f);

            // 随机音调波动（±5%）
            sfxSource.pitch = 1f + UnityEngine.Random.Range(-0.1f, 0.1f);

            // 播放音效
            sfxSource.PlayOneShot(clip, finalVolume);


        }
        else
        {
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(clip, baseVolume);
        }

    }


    public void PlaySound(string clipName, bool isRandom = false)
    {
        var clip = GetClip(clipName, "SFX");
        PlaySound(clip, isRandom);
    }

    private AudioClip GetClip(string clipName, string type)
    {
        var clip = Resources.Load<AudioClip>("Audio/" + type + "/" + clipName);
        if (clip == null) throw new ArgumentException($"音频切片文件为空。切片名为: {clipName}，切片类型为: {type}。" +
            $"请确保切片文件\"Resources\\Audio\\{type}\\{clipName}\"存在");
        return clip;
    }

    private float GetNormalizedBGMVolume()
    {
        return GameDataManager.Instance.AudioData.masterVolume * GameDataManager.Instance.AudioData.bgmVolume;
    }

    private float GetNormalizedSFXVolume()
    {
        return GameDataManager.Instance.AudioData.masterVolume * GameDataManager.Instance.AudioData.sfxVolume;
    }
    /// <summary>
    /// 在考虑上一个地点的情况下播放当前环境的背景音乐
    /// </summary>
    public void PlayPlaceMusic(EnvironmentBag nextEnvironmentBag)
    {
        switch (nextEnvironmentBag.PlaceData.placeType)
        {

            case PlaceEnum.PowerCabin:
                if (GameManager.Instance.CurEnvironmentBag.PlaceData.isInSpacecraft)
                    //无事发生
                    break;
                else
                    SoundManager.Instance.StopBGM();
                SoundManager.Instance.PlayBGM("飞船内_01", true);
                break;
            case PlaceEnum.Cockpit:
                if (GameManager.Instance.CurEnvironmentBag.PlaceData.isInSpacecraft)
                    //无事发生
                    break;
                else
                    SoundManager.Instance.StopBGM();
                SoundManager.Instance.PlayBGM("飞船内_01", true);
                break;
            case PlaceEnum.LifeSupportCabin:
                if (GameManager.Instance.CurEnvironmentBag.PlaceData.isInSpacecraft)
                    //无事发生
                    break;
                else
                    SoundManager.Instance.StopBGM();
                SoundManager.Instance.PlayBGM("飞船内_01", true);
                break;
            case PlaceEnum.CoralCoast:
                SoundManager.Instance.StopBGM();
                //珊瑚礁海域的音乐还没制作
                //SoundManager.Instance.PlayBGM("珊瑚礁海域_01", true);
                break;
        };
    }
    
    /// <summary>
    /// 不考虑上一个场景，直接播当前地点音乐
    /// </summary>
    public void PlayCurEnvironmentMusic()
    {
        // 播放环境音乐
        switch (GameDataManager.Instance.LastPlace)
        {
            case PlaceEnum.PowerCabin:
                SoundManager.Instance.PlayBGM("飞船内_01", true);
                break;
            case PlaceEnum.Cockpit:
                SoundManager.Instance.PlayBGM("飞船内_01", true);
                break;
            case PlaceEnum.LifeSupportCabin:
                SoundManager.Instance.PlayBGM("飞船内_01", true);
                break;
            case PlaceEnum.CoralCoast:
                //珊瑚礁海域的音乐还没制作
                //SoundManager.Instance.PlayBGM("珊瑚礁海域_01, true);
                break;
        }
    }
}