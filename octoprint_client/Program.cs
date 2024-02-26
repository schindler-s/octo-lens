
class Program
{
    static async Task Main(string[] args)
    {
        // Set your OctoPrint server URL and API key
        string octoPrintUrl = "http://your_octoprint_server_address/api";
        string apiKey = "your_api_key";

        // Create an instance of OctoPrintClient
        OctoPrintClient client = new OctoPrintClient(octoPrintUrl, apiKey);

        // Call the GetPrinterStatusAsync method
        string printerStatus = await client.GetPrinterStatusAsync();

        // Display the printer status
        Console.WriteLine("Printer Status:");
        Console.WriteLine(printerStatus);
    }
}