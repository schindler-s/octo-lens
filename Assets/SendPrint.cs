using OctoprintClient;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SendPrint : MonoBehaviour
{
    private string octoPrintUrl = "http://192.168.0.104/";
    private string apiKey = "34399E4785154539964185967FBE3EC7";
    private OctoprintConnection connection;
    OctoprintFileTracker fileTracker;

    // Start is called before the first frame update
    void Start()
    {
        // Initiating the connection can be done on Start or on a button press
        // It's better to have the connection ready before trying to print
        InitiateConnection();
    }

    public void InitiateConnection()
    {
        // We don't await here because Unity's Start method cannot be async
        Task.Run(async () =>
        {
            connection = await CreateOctoprintConnectionAsync(octoPrintUrl, apiKey);
            if (connection != null)
            {
                fileTracker = connection.Files;
                // Assuming you want to select the file on start, otherwise move this into the StartPrint method
                fileTracker.Select("cube_tiny.gcode");
            }
        });
    }

    async Task<OctoprintConnection> CreateOctoprintConnectionAsync(string octoPrintUrl, string apiKey)
    {
        try
        {
            return new OctoprintConnection(octoPrintUrl, apiKey);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error creating OctoPrint connection: {ex.Message}");
            return null;
        }
    }

    public void StartPrint()
    {
        // We don't await here because Unity's event system doesn't support async methods
        Task.Run(async () =>
        {
            if (connection != null && fileTracker != null)
            {
                // Add your logic here to start the print job
                // For example: await fileTracker.StartPrint("cube_tiny.gcode");
            }
            else
            {
                Debug.LogError("Connection to OctoPrint server not established.");
            }
        });
    }
}
