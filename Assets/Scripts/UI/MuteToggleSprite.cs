using UnityEngine;

public class MuteToggleSprite : MonoBehaviour
{
    [SerializeField] private GameObject unmuteImage;

    public void ChangeSprite(bool b)
    {
        unmuteImage.SetActive(!unmuteImage.activeSelf);
    }
}
