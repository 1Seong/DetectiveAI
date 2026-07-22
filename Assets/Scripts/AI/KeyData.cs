using UnityEngine;

[CreateAssetMenu(fileName = "KeyData", menuName = "Scriptable Objects/KeyData")]
public class KeyData : ScriptableObject
{
    [TextArea(10, 1000)]
    public string key;
}
