using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EvidenceData", menuName = "Scriptable Objects/EvidenceData")]
[Serializable]
public class EvidenceData : ScriptableObject
{
    public string evidenceId;
    [TextArea]
    public string fact;
}
