using UnityEngine;
using System.Threading.Tasks;
using TMPro; // Add the TextMesh Pro namespace
using OctoprintClient;

public class PrinterStatusDisplay : MonoBehaviour
{
    public TextMeshPro textMesh; // Reference to your Text Mesh Pro component

    // Set your OctoPrint server URL and API key
    private string octoPrintUrl = "http://192.168.0.104/";
    private string apiKey = "34399E4785154539964185967FBE3EC7";
    private OctoprintConnection connection;
    private OctoprintPrinterTracker printerTracker;

    async void Start()
    {
        connection = await CreateOctoprintConnectionAsync(octoPrintUrl, apiKey);
        if (connection == null)
        {
            UpdateText("Failed to connect to printer. Exiting.");
            return;
        }

        printerTracker = connection.Printers;
        printerTracker.BestBeforeMilisecs = 1000;
        printerTracker.PrinterstateHandlers += PrinterStatusChanged;
        ShowPrinterStatus(printerTracker);

        // Removed WebSocket handling
    }

    void ShowPrinterStatus(OctoprintPrinterTracker printerTracker)
    {
        OctoprintFullPrinterState printerState = printerTracker.GetFullPrinterState();
        if (printerState == null) return;
        OctoprintTemperatureState tempState = printerState.TempState;
        UpdateText("\nNew Message from Printer:\n" + tempState.ToString());
    }

    void PrinterStatusChanged(OctoprintPrinterState newPrinterState)
    {
        UpdateText("Printer status changed:\n" + newPrinterState.ToString());
    }

    async Task<OctoprintConnection> CreateOctoprintConnectionAsync(string octoPrintUrl, string apiKey)
    {
        UpdateText("Connecting to printer...");
        try
        {
            OctoprintConnection connection = await Task.Run(() =>
            {
                return new OctoprintConnection(octoPrintUrl, apiKey);
            });
            UpdateText("Connected!");
            return connection;
        }
        catch (System.Exception ex)
        {
            UpdateText($"Failed to connect to printer: {ex.Message}");
            return null;
        }
    }

    // Update the TextMeshPro text
    void UpdateText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }
}
