using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Background Music")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private bool playBGMOnStart = true;
    [SerializeField] private bool loopBGM = true;
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.5f;
    
    [Header("Sound Effects")]
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;
    
    private int currentBGMIndex = 0;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (playBGMOnStart && bgmClips.Length > 0)
        {
            PlayBGM(0);
        }
    }
    
    private void InitializeAudioSources()
    {
        // Create BGM AudioSource if not assigned
        if (bgmSource == null)
        {
            GameObject bgmObject = new GameObject("BGM_Source");
            bgmObject.transform.SetParent(transform);
            bgmSource = bgmObject.AddComponent<AudioSource>();
        }
        
        // Create SFX AudioSource if not assigned
        if (sfxSource == null)
        {
            GameObject sfxObject = new GameObject("SFX_Source");
            sfxObject.transform.SetParent(transform);
            sfxSource = sfxObject.AddComponent<AudioSource>();
        }
        
        // Configure BGM source
        bgmSource.loop = loopBGM;
        bgmSource.volume = bgmVolume;
        bgmSource.playOnAwake = false;
        
        // Configure SFX source
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
        sfxSource.playOnAwake = false;
    }
    
    #region BGM Methods
    
    public void PlayBGM(int clipIndex)
    {
        if (bgmClips == null || clipIndex < 0 || clipIndex >= bgmClips.Length)
        {
            Debug.LogWarning($"Invalid BGM clip index: {clipIndex}");
            return;
        }
        
        currentBGMIndex = clipIndex;
        bgmSource.clip = bgmClips[clipIndex];
        bgmSource.Play();
        
        Debug.Log($"Playing BGM: {bgmClips[clipIndex].name}");
    }
    
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("BGM clip is null");
            return;
        }
        
        bgmSource.clip = clip;
        bgmSource.Play();
        
        Debug.Log($"Playing BGM: {clip.name}");
    }
    
    public void StopBGM()
    {
        bgmSource.Stop();
        Debug.Log("BGM stopped");
    }
    
    public void PauseBGM()
    {
        bgmSource.Pause();
        Debug.Log("BGM paused");
    }
    
    public void ResumeBGM()
    {
        bgmSource.UnPause();
        Debug.Log("BGM resumed");
    }
    
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }
    
    public void FadeBGM(float targetVolume, float duration)
    {
        StartCoroutine(FadeBGMCoroutine(targetVolume, duration));
    }
    
    private IEnumerator FadeBGMCoroutine(float targetVolume, float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        
        bgmSource.volume = targetVolume;
        bgmVolume = targetVolume;
    }
    
    #endregion
    
    #region SFX Methods
    
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("SFX clip is null");
            return;
        }
        
        sfxSource.PlayOneShot(clip);
    }
    
    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            Debug.LogWarning("SFX clip is null");
            return;
        }
        
        sfxSource.PlayOneShot(clip, volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
    
    #endregion
    
    #region Public Properties
    
    public bool IsBGMPlaying
    {
        get { return bgmSource.isPlaying; }
    }
    
    public float BGMVolume
    {
        get { return bgmVolume; }
        set { SetBGMVolume(value); }
    }
    
    public float SFXVolume
    {
        get { return sfxVolume; }
        set { SetSFXVolume(value); }
    }
    
    public AudioClip CurrentBGM
    {
        get { return bgmSource.clip; }
    }
    
    #endregion
} 