using UnityEngine;
using System.Threading.Tasks;
using TMPro; // Add the TextMesh Pro namespace
using OctoprintClient;
using System.Linq;
using System;

public class PrinterStatusDisplay : MonoBehaviour
{
    public TextMeshPro textMeshLeft; // Reference to your Text Mesh Pro component
    public TextMeshPro textMeshRight; // Reference to your Text Mesh Pro component

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
            UpdateTextLeft("Failed to connect to printer. Exiting.");
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





        // Get the current printer status
        OctoprintFullPrinterState printerFullState = printerTracker.GetFullPrinterState();
        if (printerFullState == null) return;
        var printerState = printerFullState.PrinterState;
        var tempState = printerFullState.TempState;

        var flags = printerState.Flags;

        bool isOperational = flags.Operational;
        bool hasError = flags.ClosedOrError;
        PrinterState state;

        if (flags.Printing)
        {
            state = PrinterState.printing;
        }
        else if (flags.Pausing)
        {
            state = PrinterState.pausing;

        }
        else if (flags.Cancelling)
        {
            state = PrinterState.canceling;
        }
        else if (flags.Ready)
        {
            state = PrinterState.ready;
        }
        else if (flags.ClosedOrError)
        {
            state = PrinterState.disconnected;
        }
        else
        {
            state = PrinterState.unknown;
        }

        var tools = tempState.Tools;
        var bed = tempState.Bed;
        var bedTempCurrent = formatTemp(bed.Actual);
        var bedTempTarget = formatTemp(bed.Target);

        string textLeft = "";
        string textRight = "";

        textLeft += $"Operational: {(isOperational ? "Yes" : "No")}" + "\n";
        textLeft += "State: " + state + "\n";
        textRight += $"Bed temperature(current): {bedTempCurrent}" + "\n";
        textRight += $"Bed temperature(target): {bedTempTarget}" + "\n";

        int toolNumber = 1;
        foreach (var tool in tools)
        {
            string tempCurrent = formatTemp(tool.Actual);
            string tempTarget = formatTemp(tool.Target);
            textLeft +=  $"Tool {toolNumber}(current): {tempCurrent}" + "\n";
            textLeft += $"Tool {toolNumber++}(target): {tempTarget}" + "\n";
        }

        UpdateTextLeft(textLeft);
        UpdateTextRight(textRight);
    }

    void PrinterStatusChanged(OctoprintPrinterState newPrinterState)
    {
        UpdateTextLeft("Printer status changed:\n" + newPrinterState.ToString());
    }

    async Task<OctoprintConnection> CreateOctoprintConnectionAsync(string octoPrintUrl, string apiKey)
    {
        UpdateTextRight("Connecting to printer...");
        try
        {
            OctoprintConnection connection = await Task.Run(() =>
            {
                return new OctoprintConnection(octoPrintUrl, apiKey);
            });
            UpdateTextLeft("Connected!");
            return connection;
        }
        catch (System.Exception ex)
        {
            UpdateTextLeft($"Failed to connect to printer: {ex.Message}");
            return null;
        }
    }

    // Update the TextMeshPro text
    void UpdateTextRight(string text)
    {
        if (textMeshRight != null)
        {
            textMeshRight.text = text;
        }
    }

    void UpdateTextLeft(string text)
    {
        if (textMeshLeft != null)
        {
            // Split the text into an array of lines
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Take the first 12 lines, if there are that many
            var first12Lines = lines.Take(12);

            // Join the lines back into a single string with newline characters
            var newText = string.Join(Environment.NewLine, first12Lines);

            textMeshLeft.text = newText;
        }
    }

    static string formatTemp(double temp)
    {
        return string.Format("{0:N1}°C", temp);
    }
}


enum PrinterState
{
    ready,
    printing,
    canceling,
    pausing,
    disconnected,
    unknown,
}