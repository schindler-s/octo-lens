using OctoprintClient;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SendPrint : MonoBehaviour
{

    // Set your OctoPrint server URL and API key
    private string octoPrintUrl = "http://192.168.0.104/";
    private string apiKey = "34399E4785154539964185967FBE3EC7";
    private OctoprintConnection connection;
    OctoprintFileTracker fileTracker;
    // Start is called before the first frame update
    async void Start()
    {
        connection = await CreateOctoprintConnectionAsync(octoPrintUrl, apiKey);
        if (connection == null)
        {
            return;
        }

        fileTracker = connection.Files;

        fileTracker.Select("cube_tiny.gcode");
    }

    async Task<OctoprintConnection> CreateOctoprintConnectionAsync(string octoPrintUrl, string apiKey)
    {
        try
        {
            OctoprintConnection connection = await Task.Run(() =>
            {
                return new OctoprintConnection(octoPrintUrl, apiKey);
            });
            return connection;
        }
        catch (System.Exception ex)
        {
            return null;
        }
    }
}
