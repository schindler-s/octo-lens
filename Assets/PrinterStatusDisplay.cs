using UnityEngine;
using System.Threading.Tasks;
using TMPro; // Add the TextMesh Pro namespace
using OctoprintClient;


public class PrinterStatusDisplay : MonoBehaviour
{
    public TextMeshPro textMeshStatus; // Reference to your Text Mesh Pro component
    public TextMeshPro textMeshData; // Reference to your Text Mesh Pro component
    public TextMeshPro textMeshFiles; // Reference to your Text Mesh Pro component

    // Set your OctoPrint server URL and API key
    private string octoPrintUrl = "http://192.168.0.104/";
    private string apiKey = "34399E4785154539964185967FBE3EC7";
    private OctoprintConnection connection;
    private OctoprintPrinterTracker printerTracker;
    private OctoprintJobTracker jobTracker;
    OctoprintFileTracker fileTracker;

    async void Start()
    {
        connection = await CreateOctoprintConnectionAsync(octoPrintUrl, apiKey);
        if (connection == null)
        {
            updateTextStatus("Failed to connect to printer. Exiting.");
            return;
        }

        printerTracker = connection.Printers;
        fileTracker = connection.Files;
        jobTracker = connection.Jobs;

        printerTracker.BestBeforeMilisecs = 1000;
        printerTracker.PrinterstateHandlers += PrinterStatusChanged;

        // Start the loop to repeatedly call ShowPrinterStatus every 5 seconds
        while (true)
        {
            ShowPrinterStatus(printerTracker, fileTracker, jobTracker);
            // Wait for 5 seconds before the next call
            await Task.Delay(5000);
        }
    }

    void ShowPrinterStatus(OctoprintPrinterTracker printerTracker, OctoprintFileTracker fileTracker, OctoprintJobTracker jobTracker)
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
            state = PrinterState.Printing;
        }
        else if (flags.Pausing)
        {
            state = PrinterState.Pausing;

        }
        else if (flags.Cancelling)
        {
            state = PrinterState.Canceling;
        }
        else if (flags.Ready)
        {
            state = PrinterState.Ready;
        }
        else if (flags.ClosedOrError)
        {
            state = PrinterState.Disconnected;
        }
        else
        {
            state = PrinterState.Unknown;
        }

        var tools = tempState.Tools;
        var bed = tempState.Bed;
        var bedTempCurrent = formatTemp(bed.Actual);
        var bedTempTarget = formatTemp(bed.Target);

        string textStatus = "";
        string text = "";
        string textFiles = "";

        textStatus += $"Operational: {(isOperational ? "Yes" : "No")}      ";
        textStatus += "State: " + state;
        text += $"Bed temperature(current): {bedTempCurrent}" + "\n";
        text += $"Bed temperature(target): {bedTempTarget}" + "\n";

        int toolNumber = 1;
        foreach (var tool in tools)
        {
            string tempCurrent = formatTemp(tool.Actual);
            string tempTarget = formatTemp(tool.Target);
            text +=  $"Tool {toolNumber}(current): {tempCurrent}" + "\n";
            text += $"Tool {toolNumber++}(target): {tempTarget}" + "\n\n\n";
        }

        var info = jobTracker.GetInfo();
        var progress = jobTracker.GetProgress();
        var isPrinting = printerTracker.GetPrinterState().Flags.Printing;
        text += "Jobs:\n";
        if (isPrinting)
        {
            text += info + "\n";
        }
        text += progress + "\n";

        var mainFolder = fileTracker.GetFiles();
        var files = mainFolder.octoprintFiles;

        foreach (var file in files)
        {
            
            var name = file.Name;
            var estimatedTimeInSeconds = file.GcodeAnalysis_estimatedPrintTime;
            var successfulPrints = file.Print_success;
            var prints = file.Print_failure + successfulPrints;
            var lastTimePrinted = file.Print_last_date != 0 ? file.Print_last_date.ToString() : "-";
            var minutes = estimatedTimeInSeconds / 60;
            var seconds = estimatedTimeInSeconds % 60;
            textFiles += $"{name}\n";
            textFiles += $"    Estimated Time: {minutes}:{seconds} min" + "\n";
            textFiles += $"    Successful Prints: {successfulPrints}/{prints}\n\n";
        };

        updateTextFiles(textFiles);
        updateTextStatus(textStatus);
        updateText(text);
    }


    void PrinterStatusChanged(OctoprintPrinterState newPrinterState)
    {
        updateTextStatus("Printer status changed:\n" + newPrinterState.ToString());
    }

    async Task<OctoprintConnection> CreateOctoprintConnectionAsync(string octoPrintUrl, string apiKey)
    {
        updateText("Connecting to printer...");
        try
        {
            OctoprintConnection connection = await Task.Run(() =>
            {
                return new OctoprintConnection(octoPrintUrl, apiKey);
            });
            updateTextStatus("Connected!");
            return connection;
        }
        catch (System.Exception ex)
        {
            updateTextStatus($"Failed to connect to printer: {ex.Message}");
            return null;
        }
    }

    // Update the TextMeshPro text
    void updateText(string text)
    {
        if (textMeshData != null)
        {
            textMeshData.text = text;
        }
    }

    void updateTextStatus(string text)
    {
        if (textMeshStatus != null)
        {
            textMeshStatus.text = text;
        }
    }

    void updateTextFiles(string text)
    {
        if (textMeshFiles != null)
        {
            textMeshFiles.text = text;
        }
    }

    static string formatTemp(double temp)
    {
        return string.Format("{0:N1}°C", temp);
    }
}


enum PrinterState
{
    Ready,
    Printing,
    Canceling,
    Pausing,
    Disconnected,
    Unknown,
}