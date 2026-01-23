using System.ComponentModel;
using ModelContextProtocol.Server;

namespace EchoMcpServer.Tools;

[McpServerToolType]
public class EchoTool
{
    [McpServerTool]
    public Task<string> Echo([Description("The message to echo back")] string message)
    {
        return Task.FromResult($"Hello {message}");
    }
}
