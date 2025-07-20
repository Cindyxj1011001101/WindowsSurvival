using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance => instance;

    private AudioSource audioSource;
    private AudioSource sfxSource; // 专用于音效

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
        audioSource.volume = GetNormalizedBGMVolume();
    }

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.volume = GetNormalizedBGMVolume();
        audioSource.loop = loop;
        audioSource.Play();
    }

    public void PlayBGM(string clipName, bool loop = true)
    {
        var clip = GetClip(clipName, "Music");
        PlayBGM(clip, loop);
    }

    public void StopBGM()
    {
        audioSource.Stop();
    }

    public void PlaySound(AudioClip clip, bool isRandom)
    {
        // 获取基础音量（原逻辑）
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
}