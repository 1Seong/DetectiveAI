using System;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType
{
    ButtonClick,
    Dialogue,
    EvidenceSelected,
    DeductionStart,
    DeductionSuccess,
    DeductionFail
}

public enum BGMType
{
    Main,
    Investigation,
    Deduction,
    Result
}

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Scriptable Objects/AudioLibrary")]
public class AudioLibrary : ScriptableObject
{
    [Serializable]
    public class SFXData
    {
        public SFXType type;
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volumeScale = 1f;
    }

    [Serializable]
    public class BGMData
    {
        public BGMType type;
        public AudioClip clip;
    }

    [SerializeField] private List<SFXData> sfxList = new();
    [SerializeField] private List<BGMData> bgmList = new();

    public bool TryGetSFX(
        SFXType type,
        out SFXData result)
    {
        result = sfxList.Find(data => data.type == type);
        return result != null && result.clip != null;
    }

    public bool TryGetBGM(
        BGMType type,
        out AudioClip clip)
    {
        BGMData result =
            bgmList.Find(data => data.type == type);

        clip = result?.clip;
        return clip != null;
    }
}
