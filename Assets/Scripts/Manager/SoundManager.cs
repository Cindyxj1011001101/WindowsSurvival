using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance => instance;

    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

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

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip, GetNormalizedSFXVolume());
    }

    public void PlaySound(string clipName)
    {
        var clip = GetClip(clipName, "SFX");
        PlaySound(clip);
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