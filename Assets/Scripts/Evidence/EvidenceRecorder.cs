using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OriginalEvidenceRecord
{
    public List<PhotoData> photos;
    public List<CollectiveEvidence> collectiveEvidences;
    public List<SoundSource> audios;
    public string playerDescription;
}

public class EvidenceRecorder : MonoBehaviour
{
    private List<OriginalEvidenceRecord> originalEvidenceRecords = new();

    public void AddRecord(OriginalEvidenceRecord record)
    {
        originalEvidenceRecords.AddRange(originalEvidenceRecords);
    }
}
