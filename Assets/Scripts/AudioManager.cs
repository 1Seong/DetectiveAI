using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [SerializeField] private AudioLibrary audioLibrary;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("Audio Mixer에 노출한 파라미터 이름")]
    [SerializeField] private string masterVolumeParameter = "Master";
    [SerializeField] private string bgmVolumeParameter = "BGM";
    [SerializeField] private string sfxVolumeParameter = "SFX";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Initial Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;
    
    [Header("BGM Fade")]
    [Min(0f)]
    [SerializeField] private float fadeInDuration = 0.5f;

    [Min(0f)]
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    

    private bool isMasterMuted;
    private bool isBGMMuted;
    private bool isSFXMuted;

    public float MasterVolume => masterVolume;
    public float BGMVolume => bgmVolume;
    public float SFXVolume => sfxVolume;

    public bool IsMasterMuted => isMasterMuted;
    public bool IsBGMMuted => isBGMMuted;
    public bool IsSFXMuted => isSFXMuted;

    private const float MinimumDecibel = -80f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioSources();
        ApplyAllVolumes();
    }

    private void InitializeAudioSources()
    {
        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    #region Volume

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);

        ApplyVolume(
            masterVolumeParameter,
            masterVolume,
            isMasterMuted
        );
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);

        ApplyVolume(
            bgmVolumeParameter,
            bgmVolume,
            isBGMMuted
        );
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);

        ApplyVolume(
            sfxVolumeParameter,
            sfxVolume,
            isSFXMuted
        );
    }

    private void ApplyAllVolumes()
    {
        ApplyVolume(
            masterVolumeParameter,
            masterVolume,
            isMasterMuted
        );

        ApplyVolume(
            bgmVolumeParameter,
            bgmVolume,
            isBGMMuted
        );

        ApplyVolume(
            sfxVolumeParameter,
            sfxVolume,
            isSFXMuted
        );
    }

    private void ApplyVolume(
        string parameter,
        float linearVolume,
        bool isMuted)
    {
        float decibel = isMuted
            ? MinimumDecibel
            : LinearToDecibel(linearVolume);

        audioMixer.SetFloat(parameter, decibel);
    }

    private float LinearToDecibel(float linearVolume)
    {
        if (linearVolume <= 0.0001f)
            return MinimumDecibel;

        return Mathf.Log10(linearVolume) * 20f;
    }

    #endregion

    #region Mute

    // Toggle의 On Value Changed(bool)에 직접 연결할 수 있습니다.
    // true일 때 음소거됩니다.

    public void SetMasterMute(bool isMuted)
    {
        isMasterMuted = isMuted;

        ApplyVolume(
            masterVolumeParameter,
            masterVolume,
            isMasterMuted
        );
    }

    public void SetBGMMute(bool isMuted)
    {
        isBGMMuted = isMuted;

        ApplyVolume(
            bgmVolumeParameter,
            bgmVolume,
            isBGMMuted
        );
    }

    public void SetSFXMute(bool isMuted)
    {
        isSFXMuted = isMuted;

        ApplyVolume(
            sfxVolumeParameter,
            sfxVolume,
            isSFXMuted
        );
    }

    #endregion

    #region SFX

    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, 1f);
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(
            clip,
            volumeScale
        );
    }

    public void StopAllSFX()
    {
        if (sfxSource == null)
            return;

        sfxSource.Stop();
    }

    #endregion

    #region BGM

    private Tween bgmFadeTween;

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null)
            return;

        bgmFadeTween?.Kill();
        bgmFadeTween = null;

        // 같은 BGM이면 재시작하지 않고 목표 음량으로 복구합니다.
        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            bgmFadeTween = bgmSource
                .DOFade(1f, fadeInDuration)
                .SetEase(Ease.OutSine)
                .SetUpdate(true);

            return;
        }

        // 기존 BGM이 재생 중이면 페이드 아웃 후 새 BGM을 재생합니다.
        if (bgmSource.isPlaying)
        {
            Sequence sequence = DOTween.Sequence()
                .SetUpdate(true);

            sequence.Append(
                bgmSource
                    .DOFade(0f, fadeOutDuration)
                    .SetEase(Ease.InSine)
            );

            sequence.AppendCallback(() =>
            {
                bgmSource.Stop();
                bgmSource.clip = clip;
                bgmSource.loop = true;
                bgmSource.volume = 0f;
                bgmSource.Play();
            });

            sequence.Append(
                bgmSource
                    .DOFade(1f, fadeInDuration)
                    .SetEase(Ease.OutSine)
            );

            sequence.OnComplete(() => bgmFadeTween = null);
            bgmFadeTween = sequence;
        }
        else
        {
            // 재생 중인 BGM이 없으면 바로 페이드 인합니다.
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = 0f;
            bgmSource.Play();

            bgmFadeTween = bgmSource
                .DOFade(1f, fadeInDuration)
                .SetEase(Ease.OutSine)
                .SetUpdate(true)
                .OnComplete(() => bgmFadeTween = null);
        }
    }

    public void StopBGM()
    {
        if (bgmSource == null)
            return;

        bgmFadeTween?.Kill();
        bgmFadeTween = null;

        if (!bgmSource.isPlaying)
        {
            bgmSource.clip = null;
            bgmSource.volume = 1f;
            return;
        }

        bgmFadeTween = bgmSource
            .DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InSine)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                bgmSource.Stop();
                bgmSource.clip = null;

                // 다음 재생을 위한 기본값 복구
                bgmSource.volume = 1f;
                bgmFadeTween = null;
            });
    }
    
    public void PauseBGM()
    {
        if (bgmSource == null)
            return;

        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (bgmSource == null || bgmSource.clip == null)
            return;

        bgmSource.UnPause();
    }

    #endregion
    
    public void PlaySFX(SFXType type)
    {
        if (!audioLibrary.TryGetSFX(type, out var data))
        {
            Debug.LogWarning($"등록되지 않은 SFX입니다: {type}");
            return;
        }

        PlaySFX(data.clip, data.volumeScale);
    }

    public void PlayBGM(BGMType type)
    {
        if (!audioLibrary.TryGetBGM(type, out var clip))
        {
            Debug.LogWarning($"등록되지 않은 BGM입니다: {type}");
            return;
        }

        PlayBGM(clip);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        bgmFadeTween?.Kill();
    }
}