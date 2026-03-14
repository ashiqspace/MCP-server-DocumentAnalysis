using System.ComponentModel;
using ModelContextProtocol.Server;

namespace EchoMcpServer.Tools;

[McpServerToolType]
public class RandomNumberTool
{
    /// <summary>
    /// Generates a random number between the specified minimum and maximum values
    /// </summary>
    [McpServerTool(Name = "randomNumber")]
    [Description("Generates a random number between the specified minimum and maximum values")]
    public Task<string> GenerateRandomNumber(
        [Description("Minimum value for the random number (default: 1)")] int min = 1,
        [Description("Maximum value for the random number (default: 100)")] int max = 100)
    {
        var random = new Random();
        var number = random.Next(min, max + 1);
        return Task.FromResult($"Random number between {min} and {max}: {number}");
    }
}
