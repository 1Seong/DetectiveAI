using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EvidenceData", menuName = "Scriptable Objects/EvidenceData")]
[Serializable]
public class EvidenceData : ScriptableObject
{
    public string evidenceId;
    public string fact;
}
