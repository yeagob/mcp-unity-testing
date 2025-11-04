using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace MCP.UnityServer;

/// <summary>
/// Entry point for the MCP Unity Testing Server.
/// This server exposes tools that allow AI models to interact with Unity for automated testing.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure logging to stderr (stdout is used for MCP protocol)
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        // Add MCP Server with stdio transport
        builder.Services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new ServerInfo
                {
                    Name = "unity-testing-server",
                    Version = "0.1.0"
                };
                options.Capabilities = new ServerCapabilities
                {
                    Tools = new ToolsCapability { }
                };
            })
            .WithStdioServerTransport()
            .WithToolsFromAssembly(); // Auto-discover tools with [McpServerTool] attribute

        var app = builder.Build();

        // Log startup
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("MCP Unity Testing Server starting...");
        logger.LogInformation("Server: unity-testing-server v0.1.0");
        logger.LogInformation("Transport: stdio");
        logger.LogInformation("Waiting for MCP client connection...");

        await app.RunAsync();
    }
}
