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

        // Wait for user input asynchronously
        Task userInputTask = WaitForUserInputAsync();

        // Wait for either the connection task or user input task to complete
        await Task.WhenAny(connectionTask, userInputTask);

        // If the user input task completed, cancel the connection attempt
        if (!connectionTask.IsCompleted)
        {
            Console.WriteLine("Connection attempt canceled by user.");
            return;
        }

        // Get the OctoprintConnection instance
        var connection = await connectionTask;

        if (connection == null) return;

        Console.WriteLine("Connected!");

        // Create an OctoprintPrinterTracker instance
        OctoprintPrinterTracker printerTracker = new OctoprintPrinterTracker(connection);

        // Subscribe to the event for printer status changes
        printerTracker.PrinterstateHandlers += PrinterStatusChanged;

        // Call method to display printer status
        ShowPrinterStatus(printerTracker);

        Console.WriteLine("Connecting to Printer. Listening to printer state changes...");
        connection.WebsocketStart();

        // Wait for user input (press Enter to exit)
        Console.ReadLine();

        // Stop the WebSocket when the program ends
        connection.WebsocketStop();
        Console.WriteLine("Closing connection...");
    }

    /// <summary>
    /// Displays the current printer status.
    /// </summary>
    /// <param name="printerTracker">OctoprintPrinterTracker instance.</param>
    static void ShowPrinterStatus(OctoprintPrinterTracker printerTracker)
    {
        // Get the current printer status
        OctoprintPrinterState printerState = printerTracker.GetPrinterState();
        Console.WriteLine("Current Printer Status:");
        Console.WriteLine(printerState.ToString());
    }

    /// <summary>
    /// Event handler method for printer status changes.
    /// </summary>
    /// <param name="newPrinterState">New printer state.</param>
    static void PrinterStatusChanged(OctoprintPrinterState newPrinterState)
    {
        Console.WriteLine("Printer status changed:");
        Console.WriteLine(newPrinterState.ToString());
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

    static Task WaitForUserInputAsync()
    {
        // Use a TaskCompletionSource to enable asynchronous user input
        var tcs = new TaskCompletionSource<object?>();

        // Start a task that waits for user input
        Task.Run(() =>
        {
            // When the user inputs something, mark the task as completed
            Console.ReadLine();
            tcs.SetResult(null);
        });

        // Return the TaskCompletionSource.Task to wait for user input
        return tcs.Task;
    }

}
