using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using OctoprintClient;


public class PrinterStatusDisplay : MonoBehaviour
{
    /// <summary>
    /// text mesh to show printer status (e.g. ready, pausing,...)
    /// </summary>
    public TextMeshPro textMeshStatus;
    /// <summary>
    /// text mesh to show various informatoin about printer (temperatur, jobs,...)
    /// </summary>
    public TextMeshPro textMeshData;
    /// <summary>
    /// text mesh to show printer files
    /// </summary>
    public TextMeshPro textMeshFiles;

    // TODO make OctoPrint connection a service to be available from whole application
    private string octoPrintUrl = "http://192.168.0.104/";
    private string apiKey = "34399E4785154539964185967FBE3EC7";
    private OctoprintConnection connection;
    private OctoprintPrinterTracker printerTracker;
    private OctoprintJobTracker jobTracker;
    private OctoprintFileTracker fileTracker;

    async void Start()
    {
        connection = await Connect(octoPrintUrl, apiKey);
        if (connection == null)
        {
            updateTextStatus("Failed to connect to printer. Exiting.");
            return;
        }

        printerTracker = connection.Printers;
        fileTracker = connection.Files;
        jobTracker = connection.Jobs;

        // needed to be able to get full printer state
        printerTracker.BestBeforeMilisecs = 1000;

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
        PrinterState state = getStateFromFlags(flags);

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
        // get actual and target temp for each tool
        foreach (var tool in tools)
        {
            string tempCurrent = formatTemp(tool.Actual);
            string tempTarget = formatTemp(tool.Target);
            text += $"Tool {toolNumber}(current): {tempCurrent}" + "\n";
            text += $"Tool {toolNumber++}(target): {tempTarget}" + "\n\n";
        }

        var info = jobTracker.GetInfo();
        var progress = jobTracker.GetProgress();
        var isPrinting = printerTracker.GetPrinterState().Flags.Printing;
        text += "Jobs:\n";

        // show current job info if printer is currently printing
        if (isPrinting)
        {
            text += "EstimatedPrinttime: " + info.EstimatedPrintTime + "\nAt File: " + info.File + "Using Fillament: \n" + info.Filament + "\n";
        }
        text += progress + "\n";


        var mainFolder = fileTracker.GetFiles();
        var files = mainFolder.octoprintFiles;

        // show each file available on the printer
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


        // update all text meshes
        updateTextFiles(textFiles);
        updateTextStatus(textStatus);
        updateText(text);
    }

    PrinterState getStateFromFlags(OctoprintPrinterFlags flags)
    {
        if (flags.Printing)
        {
            return PrinterState.Printing;
        }
        else if (flags.Pausing)
        {
            return PrinterState.Pausing;

        }
        else if (flags.Cancelling)
        {
            return PrinterState.Canceling;
        }
        else if (flags.Ready)
        {
            return PrinterState.Ready;
        }
        else if (flags.ClosedOrError)
        {
            return PrinterState.Disconnected;
        }
        else
        {
            return PrinterState.Unknown;
        }
    }


    /// <summary>
    /// Create a websocket connection to the printer
    /// </summary>
    /// <param name="host">host name of the printer</param>
    /// <param name="apiKey">api key needed to authenticate</param>
    /// <returns>connection or null if failed to connect</returns>
    async Task<OctoprintConnection> Connect(string host, string apiKey)
    {
        updateText("Connecting to printer...");
        try
        {
            OctoprintConnection connection = await Task.Run(() =>
            {
                return new OctoprintConnection(host, apiKey);
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

    /// <summary>
    /// updates data text mesh
    /// </summary>
    /// <param name="text"></param>
    void updateText(string text)
    {
        if (textMeshData != null)
        {
            textMeshData.text = text;
        }
    }

    /// <summary>
    /// updates status text mesh
    /// </summary>
    /// <param name="text"></param>
    void updateTextStatus(string text)
    {
        if (textMeshStatus != null)
        {
            textMeshStatus.text = text;
        }
    }

    /// <summary>
    /// updates file test mesh
    /// </summary>
    /// <param name="text"></param>
    void updateTextFiles(string text)
    {
        if (textMeshFiles != null)
        {
            textMeshFiles.text = text;
        }
    }


    /// <summary>
    /// Formats the temperature
    /// </summary>
    /// <param name="temp"></param>
    /// <returns></returns>
    static string formatTemp(double temp)
    {
        return string.Format("{0:N1}�C", temp);
    }
}


/// <summary>
/// enum to provide possible printer state
/// </summary>
enum PrinterState
{
    Ready,
    Printing,
    Canceling,
    Pausing,
    Disconnected,
    Unknown,
}