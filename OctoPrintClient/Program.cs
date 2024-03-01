using System.Diagnostics;
using Newtonsoft.Json.Linq;
using OctoprintClient;

class Program
{
    static async Task Main(string[] args)
    {
        // Set your OctoPrint server URL and API key
        string octoPrintUrl = "http://192.168.0.104/";
        string apiKey = "34399E4785154539964185967FBE3EC7";


        // Create an instance of your OctoprintConnection asynchronously
        var connectionTask = CreateOctoprintConnectionAsync(octoPrintUrl, apiKey);
        // Get the OctoprintConnection instance
        var connection = await connectionTask;
        if (connection == null)
        {
            Console.WriteLine("Failed to connect to printer. Exiting.");
            return;
        }

        // Create an OctoprintPrinterTracker instance and wait one second
        OctoprintPrinterTracker printerTracker = connection.Printers;
        printerTracker.BestBeforeMilisecs = 1000;

        OctoprintFileTracker fileTracker = connection.Files;

        // fileTracker.Select("cube_tiny.gcode");
        // fileTracker.Select("cube_small.gcode");
        // fileTracker.Select("cube_medium.gcode");

        OctoprintJobTracker jobTracker = connection.Jobs;

        // ShowPrinterStatus(printerTracker);
        // ShowFiles(fileTracker);
        ShowJobs(printerTracker, jobTracker);
    }

    /// <summary>
    /// Displays the current printer status.
    /// </summary>
    /// <param name="printerTracker">OctoprintPrinterTracker instance.</param>
    static void ShowPrinterStatus(OctoprintPrinterTracker printerTracker)
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

        Console.WriteLine($"Operational: {(isOperational ? "Yes" : "No")}");
        Console.WriteLine("State: " + state);
        Console.WriteLine($"Bed temperature(current): {bedTempCurrent}");
        Console.WriteLine($"Bed temperature(target): {bedTempTarget}");

        int toolNumber = 1;
        foreach (var tool in tools)
        {

            string tempCurrent = formatTemp(tool.Actual);
            string tempTarget = formatTemp(tool.Target);
            Console.WriteLine($"Tool {toolNumber}(current): {tempCurrent}");
            Console.WriteLine($"Tool {toolNumber++}(target): {tempTarget}");
        }






        // Console.WriteLine("__\n" + printerFullState.ToString() + "__");
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



    /// <summary>
    /// Displays the printers job.
    /// </summary>
    /// <param name="jobTracker">OctoprintJobTracker instance.</param>
    static void ShowJobs(OctoprintPrinterTracker printerTracker, OctoprintJobTracker jobTracker)
    {
        var info = jobTracker.GetInfo();
        var progress = jobTracker.GetProgress();

        var isPrinting = printerTracker.GetPrinterState().Flags.Printing;


        if (isPrinting)
        {
            Console.WriteLine(info);

        }

        Console.WriteLine(progress);
    }

    static void StartJob(OctoprintJobTracker jobTracker, String path)
    {
    }

    /// <summary>
    /// Event handler method for printer status changes.
    /// </summary>
    /// <param name="newPrinterState">New printer state.</param>
    static void PrinterStatusChanged(OctoprintPrinterState newPrinterState)
    {
        Console.WriteLine("Printer status changed:");
        Console.WriteLine("__\n" + newPrinterState.ToString() + "\n__");
    }

    /// <summary>
    /// Creates an OctoprintConnection instance asynchronously.
    /// </summary>
    /// <param name="octoPrintUrl">OctoPrint server URL.</param>
    /// <param name="apiKey">API key.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    static async Task<OctoprintConnection?> CreateOctoprintConnectionAsync(string octoPrintUrl, string apiKey)
    {
        Console.WriteLine("Connecting to printer...");
        OctoprintConnection? connection = null;
        try
        {
            // Create an instance of OctoprintConnection asynchronously
            connection = await Task.Run(() =>
            {
                return new OctoprintConnection(octoPrintUrl, apiKey);
            });
            Console.WriteLine("Connected!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to printer: {ex.Message}");
        }
        return connection;
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
