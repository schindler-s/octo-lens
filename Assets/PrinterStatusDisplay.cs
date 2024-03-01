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

        printerTracker.BestBeforeMilisecs = 1000;
        printerTracker.PrinterstateHandlers += PrinterStatusChanged;

        ShowPrinterStatus(printerTracker);
        ShowFiles(fileTracker);
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

        textStatus += $"Operational: {(isOperational ? "Yes" : "No")}  ";
        textStatus += "State: " + state;
        text += $"Bed temperature(current): {bedTempCurrent}" + "\n";
        text += $"Bed temperature(target): {bedTempTarget}" + "\n";

        int toolNumber = 1;
        foreach (var tool in tools)
        {
            string tempCurrent = formatTemp(tool.Actual);
            string tempTarget = formatTemp(tool.Target);
            text +=  $"Tool {toolNumber}(current): {tempCurrent}" + "\n";
            text += $"Tool {toolNumber++}(target): {tempTarget}" + "\n";
        }

        updateTextStatus(textStatus);
        updateText(text);
    }

    /// <summary>
    /// Displays the printers files.
    /// </summary>
    /// <param name="fileTracker">OctoprintFileTracker instance.</param>
    static void ShowFiles(OctoprintFileTracker fileTracker)
    {
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
            Console.WriteLine($"Estimated Time: {minutes}:{seconds} min");
            Console.WriteLine($"Name: {name}\nSuccessful Prints: {successfulPrints}\nPrints: {prints}\nLast Time Printed: {lastTimePrinted}");
        };
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
        if (textMeshRight != null)
        {
            textMeshRight.text = text;
        }
    }

    void updateTextStatus(string text)
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
    Ready,
    Printing,
    Canceling,
    Pausing,
    Disconnected,
    Unknown,
}