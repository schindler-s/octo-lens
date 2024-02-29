using UnityEngine;

public class OpenSlate : MonoBehaviour
{
    public GameObject slateBlank;

    public void ToggleSlate()
    {
        
        slateBlank.SetActive(!slateBlank.activeSelf);
    }
}
