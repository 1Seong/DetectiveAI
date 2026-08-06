using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResultData", menuName = "Scriptable Objects/ResultData")]
public class ResultData : ScriptableObject
{
    public float NormalScore;
    public float GoodScore;
    public Sprite BadSticker;
    public Sprite NormalSticker;
    public Sprite GoodSticker;
    public List<string> BadResponse;
    public List<string> NormalResponse;
    public List<string> GoodResponse;
}
