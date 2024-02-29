using UnityEngine;
using TMPro; // For TextMeshPro
using System.Threading.Tasks;
using OctoprintClient; // Make sure your custom DLL is properly referenced

public class PrinterStatusDisplay : MonoBehaviour
{
    public TextMeshPro textMesh; // Reference to your Text Mesh Pro component
    private string octoPrintUrl = "http://192.168.0.104/";
    private string apiKey = "34399E4785154539964185967FBE3EC7";
    private OctoprintConnection connection;

    // Start is called before the first frame update
    async void Start()
    {
        // Attempt to connect to the printer
        connection = await CreateOctoprintConnectionAsync(octoPrintUrl, apiKey);

        // Check if connection is successful
        if (connection == null)
        {
            UpdateTextMesh("Failed to connect to printer.");
            return;
        }
        // Subscribe to printer state changes
        connection.Printers.PrinterstateHandlers += PrinterStatusChanged;

        // Start WebSocket to listen for updates
        connection.WebsocketStart();
    }

    private void OnDestroy()
    {
        // Clean up the connection and stop the WebSocket
        if (connection != null)
        {
            connection.WebsocketStop();
        }
    }

    private void PrinterStatusChanged(OctoprintPrinterState newPrinterState)
    {
        // Update the Text Mesh with the new printer state
        UpdateTextMesh($"Printer status changed:\n{newPrinterState}");
    }

    private void UpdateTextMesh(string text)
    {
        // Ensure we are on the main thread when updating the UI
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }

    private async Task<OctoprintConnection> CreateOctoprintConnectionAsync(string url, string key)
    {
        try
        {
            var connection = new OctoprintConnection(url, key);
            // Additional setup or validation of connection if necessary
            UpdateTextMesh("Connected to printer.");
            return connection;
        }
        catch (System.Exception ex)
        {
            UpdateTextMesh($"Failed to connect to printer: {ex.Message}");
            return null;
        }
    }
}
