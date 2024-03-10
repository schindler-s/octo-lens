using UnityEngine;

/// <summary>
/// Allows to toggle between two text objects
/// </summary>
public class ToggleText : MonoBehaviour
{
    public GameObject fileText;
    public GameObject temperatureText;

    public void ToggleTextObjects()
    {
        if (fileText.activeSelf)
        {
            fileText.SetActive(false);
            temperatureText.SetActive(true);
        }
        else
        {
            fileText.SetActive(true);
            temperatureText.SetActive(false);
        }
    }
}
