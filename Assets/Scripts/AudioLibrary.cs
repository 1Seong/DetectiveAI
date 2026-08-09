using System;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType
{
    Fridge, Gossip, Complaint,
    AngeleNotFound,
    ChamielCapture, ChamielDelete,
    BagOpen, BagClose,
    BookOpen, BookClose, BagTab,
    Move,
    PaperButton, NormalButton,
    EndingStamp,
    GoodEnding, NormalEnding, BadEnding,
    FridgeHighPitch, GossipHighPitch, ComplaintHighPitch
}

public enum BGMType
{
    MainMenu,
    Opening,
    Ppuang, Chocolat, Alley,
    Deduction,
    Credit
}

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Scriptable Objects/AudioLibrary")]
public class AudioLibrary : ScriptableObject
{
    [Serializable]
    public class SFXData
    {
        public SFXType type;
        public AudioClip clip;

        [Range(0f, 2f)]
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
