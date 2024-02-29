using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DynamicTextChanger : MonoBehaviour
{
    public TextMeshPro textMesh; // Reference to your Text Mesh Pro component
    private float timer = 10f; // Time in seconds to wait before changing text

    private void Start()
    {
        // Set initial text
        if (textMesh != null)
        {
            textMesh.text = "Hello World";
        }

        // Start the coroutine to change text after 5 seconds
        StartCoroutine(ChangeTextAfterTime(timer));
    }

    private IEnumerator ChangeTextAfterTime(float time)
    {
        // Wait for the specified time
        yield return new WaitForSeconds(time);

        // Change the text
        if (textMesh != null)
        {
            textMesh.text = "Hello Unity";
        }
    }
}
