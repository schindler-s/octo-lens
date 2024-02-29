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
        if (connection == null) {
            Console.WriteLine("Failed to connect to printer. Exiting.");
            return;
        }

        // Create an OctoprintPrinterTracker instance and wait one second
        OctoprintPrinterTracker printerTracker = connection.Printers;
        printerTracker.BestBeforeMilisecs = 1000;

        // Subscribe to the event for printer status changes
        printerTracker.PrinterstateHandlers += PrinterStatusChanged;
        ShowPrinterStatus(printerTracker);

        // Connect to the WebSocket to listen for printer status changes (program will block here until the WebSocket is closed)
        connection.WebsocketStart();
    }

    /// <summary>
    /// Displays the current printer status.
    /// </summary>
    /// <param name="printerTracker">OctoprintPrinterTracker instance.</param>
    static  void ShowPrinterStatus(OctoprintPrinterTracker printerTracker)
    {
        // Get the current printer status
        OctoprintFullPrinterState printerState = printerTracker.GetFullPrinterState();
        if (printerState == null) return;
        Console.WriteLine("\nNew Message from Printer:");
        Console.WriteLine("__\n"+printerState.ToString() + "__");
    }

    /// <summary>
    /// Event handler method for printer status changes.
    /// </summary>
    /// <param name="newPrinterState">New printer state.</param>
    static void PrinterStatusChanged(OctoprintPrinterState newPrinterState)
    {
        Console.WriteLine("Printer status changed:");
        Console.WriteLine("__\n"+newPrinterState.ToString() + "\n__");
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
}

