using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundFacts", menuName = "Scriptable Objects/BackgroundFacts")]
public class BackgroundFacts : ScriptableObject
{
    public List<string> Facts;
}
