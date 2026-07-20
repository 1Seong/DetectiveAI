using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MisleadingClaim
{
    public string claimId;
    public string claim;
    public float penalty = 0.2f;
}

[CreateAssetMenu(fileName = "CaseSolution", menuName = "Scriptable Objects/CaseSolution")]
[Serializable]
public class CaseSolution : ScriptableObject
{
    public string culprit;
    public string method;
    public string motive;

    public List<string> keyPoints;
    public List<MisleadingClaim> misleadingClaims;
}
