using System.Collections.Generic;
using UnityEngine;

public class EvidenceRecorder : MonoBehaviour
{
    private List<PhotoData> photos;
    private List<CollectiveEvidence> collectiveEvidences;
    private List<string> hypothesis;

    public void SavePhotos(List<PhotoData> datas)
    {
        photos.AddRange(datas);
    }

    public void SaveCollectiveEvidence(List<CollectiveEvidence> datas)
    {
        collectiveEvidences.AddRange(datas);
    }

    public void SaveHypothesis(List<string> datas)
    {
        hypothesis.AddRange(datas);
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
