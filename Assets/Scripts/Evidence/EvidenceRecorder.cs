using System.Collections.Generic;
using UnityEngine;

public class EvidenceRecorder : MonoBehaviour
{
    private List<PhotoData> photos;
    private List<CollectiveEvidence> collectiveEvidences;
    private List<EvidenceRecord>  evidenceRecords;

    public void SavePhotos(List<PhotoData> datas)
    {
        photos.AddRange(datas);
    }

    public void SaveCollectiveEvidence(List<CollectiveEvidence> datas)
    {
        collectiveEvidences.AddRange(datas);
    }

    public void FinalDeduction()
    {
        
    }

    private FinalDeductionInput buildFinalDeductionInput()
    {
        return new FinalDeductionInput()
        {
            
        };
    }
}
