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
    public string motive;
    public string scene;
    public string time;
    public string accessMethod;
    public string coreAction;
    public string originalStatus;
    public string copyDestination;
    public string tasteGapReason;
    public List<MisleadingClaim> misleadingClaims;
}
