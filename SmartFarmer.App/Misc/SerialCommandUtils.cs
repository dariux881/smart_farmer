using System.Linq;
using System.Text;
using SmartFarmer.Helpers;

namespace SmartFarmer.Misc;

public class SerialCommandUtils
{
    public static byte[] ComposeRequest(string command, object[] parameters)
    {
        var requestId = SerialCommandUtils.GetRequestId(command, parameters);
        var request = SerialCommandUtils.GetRequest(requestId, command, parameters);

        return Encoding.ASCII.GetBytes(request);
    }

    public static void ParseRequest(byte[] request, out string requestId, out string command, out string parameters)
    {
        var commandParts = FromByteArray(request).Split(Constants.COMMAND_REQUEST_SEPARATOR);
        requestId = commandParts[0];
        command = commandParts[1];
        parameters = commandParts.Length > 1 
            ? commandParts[2]
            : null;
    }

    public static void ParseResponse(string response, out string requestId, out string result)
    {
        var commandParts = response.Split(Constants.COMMAND_REQUEST_SEPARATOR);
        requestId = commandParts[0];
        result = commandParts[1];
    }

    public static string GetRequestId(string command, object[] parameters)
    {
        return (command + SerializeParameters(parameters)).Encode();
    }

    public static string SerializeParameters(object[] parameters)
    {
        return
            parameters == null ? 
                "" :
                parameters.Any() ? 
                    parameters.Aggregate((p1, p2) => p1 + Constants.COMMAND_PARAM_SEPARATOR + p2).ToString() :
                    "";
    }

    public static string GetRequest(string requestId, string command, object[] parameters)
    {
        return 
            requestId + Constants.COMMAND_REQUEST_SEPARATOR + 
            command + Constants.COMMAND_REQUEST_SEPARATOR + 
            SerializeParameters(parameters);
    }

    public static byte[] ToByteArray(string request)
    {
        return Encoding.ASCII.GetBytes(request);
    }
    
    public static string FromByteArray(byte[] request)
    {
        return Encoding.ASCII.GetString(request, 0, request.Length);
    }
}