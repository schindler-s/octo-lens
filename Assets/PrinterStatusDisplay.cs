using UnityEngine;
using System.Threading.Tasks;
using TMPro; // Add the TextMesh Pro namespace
using OctoprintClient;
using System.Linq;
using System;

public class PrinterStatusDisplay : MonoBehaviour
{
    public TextMeshPro textMeshStatus; // Reference to your Text Mesh Pro component
    public TextMeshPro textMeshTemp; // Reference to your Text Mesh Pro component

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
            UpdateTextTemp("Failed to connect to printer. Exiting.");
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
        UpdateTextTemp("Temperatures:\n" + tempState.ToString());
        OctoprintPrinterState status = printerState.PrinterState;
        UpdateTextStatus(printerState.ToString());
    }

    void PrinterStatusChanged(OctoprintPrinterState newPrinterState)
    {
        UpdateTextTemp("Printer status changed:\n" + newPrinterState.ToString());
    }

    async Task<OctoprintConnection> CreateOctoprintConnectionAsync(string octoPrintUrl, string apiKey)
    {
        UpdateTextTemp("Connecting to printer...");
        try
        {
            OctoprintConnection connection = await Task.Run(() =>
            {
                return new OctoprintConnection(octoPrintUrl, apiKey);
            });
            UpdateTextTemp("Connected!");
            return connection;
        }
        catch (System.Exception ex)
        {
            UpdateTextTemp($"Failed to connect to printer: {ex.Message}");
            return null;
        }
    }

    // Update the TextMeshPro text
    void UpdateTextTemp(string text)
    {
        if (textMeshTemp != null)
        {
            textMeshTemp.text = text;
        }
    }

    void UpdateTextStatus(string text)
    {
        if (textMeshStatus != null)
        {
            // Split the text into an array of lines
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Take the first 12 lines, if there are that many
            var first12Lines = lines.Take(12);

            // Join the lines back into a single string with newline characters
            var newText = string.Join(Environment.NewLine, first12Lines);

            textMeshStatus.text = newText;
        }
    }
}
