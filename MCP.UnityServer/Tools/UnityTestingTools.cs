using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MCP.UnityServer.Tools;

/// <summary>
/// MCP Tools for Unity automated testing.
/// These tools allow AI models to interact with Unity for game testing automation.
/// </summary>
[McpServerToolType]
public static class UnityTestingTools
{
    // TODO: In production, this will be injected via DI and communicate with Unity via IPC
    // For now, we'll simulate responses for testing the MCP server itself

    /// <summary>
    /// Simple echo tool for testing MCP connectivity.
    /// </summary>
    [McpServerTool]
    [Description("Echoes the message back to verify MCP server connectivity.")]
    public static string Echo(
        [Description("The message to echo back")] string message)
    {
        return $"Unity Testing Server Echo: {message}";
    }

    /// <summary>
    /// Captures a screenshot of the Unity Game View.
    /// </summary>
    [McpServerTool]
    [Description("Captures a screenshot of the Unity Game View and returns the file path or base64 encoded image.")]
    public static async Task<string> CaptureScreenshot(
        [Description("Optional output file path. If not provided, returns base64 encoded PNG.")]
        string? outputPath = null)
    {
        // TODO: Implement IPC call to Unity Editor
        // For now, simulate the response

        var result = new
        {
            success = true,
            message = "Screenshot capture simulated (Unity integration pending)",
            outputPath = outputPath ?? "screenshot_temp.png",
            timestamp = DateTime.UtcNow,
            resolution = new { width = 1920, height = 1080 }
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Simulates a mouse click at specific screen coordinates.
    /// </summary>
    [McpServerTool]
    [Description("Simulates a mouse click at the specified screen position in Unity Game View.")]
    public static async Task<string> ClickAtPosition(
        [Description("X coordinate in pixels (0 = left edge)")] int x,
        [Description("Y coordinate in pixels (0 = top edge)")] int y,
        [Description("Mouse button to click: left, right, or middle")] string button = "left")
    {
        // Validate inputs
        if (x < 0 || y < 0)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Coordinates must be non-negative"
            });
        }

        if (button != "left" && button != "right" && button != "middle")
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Button must be 'left', 'right', or 'middle'"
            });
        }

        // TODO: Implement IPC call to Unity Editor
        var result = new
        {
            success = true,
            message = "Click simulation pending Unity integration",
            x,
            y,
            button,
            timestamp = DateTime.UtcNow,
            uiElementClicked = "Unknown (simulation mode)"
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Sends keyboard input to Unity.
    /// </summary>
    [McpServerTool]
    [Description("Sends a keyboard key press to Unity. Use Unity KeyCode names (e.g., 'UpArrow', 'Return', 'Escape').")]
    public static async Task<string> SendKeyInput(
        [Description("Unity KeyCode name (e.g., 'UpArrow', 'DownArrow', 'Return', 'Escape', 'Space')")]
        string keyCode,
        [Description("How long to hold the key in seconds")]
        float duration = 0.1f)
    {
        // Validate duration
        if (duration < 0 || duration > 10)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Duration must be between 0 and 10 seconds"
            });
        }

        // TODO: Implement IPC call to Unity Editor
        var result = new
        {
            success = true,
            message = "Key input simulation pending Unity integration",
            keyCode,
            duration,
            timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Gets the current game state from Unity.
    /// </summary>
    [McpServerTool]
    [Description("Retrieves current game state information including scene name, active UI elements, and game status.")]
    public static async Task<string> GetGameState()
    {
        // TODO: Implement IPC call to Unity Editor
        var result = new
        {
            success = true,
            message = "Game state retrieval simulated",
            gameState = new
            {
                currentScene = "MainMenu",
                isPlaying = true,
                isPaused = false,
                activeCanvases = new[] { "MainMenuCanvas", "HUDCanvas" },
                selectedUIElement = "PlayButton",
                gameTime = 123.45f,
                fps = 60
            },
            timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Gets the UI hierarchy for analysis.
    /// </summary>
    [McpServerTool]
    [Description("Retrieves the complete UI hierarchy including all canvases, panels, buttons, and their properties.")]
    public static async Task<string> GetUIHierarchy()
    {
        // TODO: Implement IPC call to Unity Editor
        var result = new
        {
            success = true,
            message = "UI hierarchy retrieval simulated",
            uiHierarchy = new
            {
                canvases = new[]
                {
                    new
                    {
                        name = "MainMenuCanvas",
                        isActive = true,
                        renderMode = "ScreenSpaceOverlay",
                        children = new[]
                        {
                            new
                            {
                                name = "PlayButton",
                                type = "Button",
                                isInteractable = true,
                                isVisible = true,
                                position = new { x = 960, y = 540 },
                                size = new { width = 200, height = 60 },
                                text = "Play Game"
                            },
                            new
                            {
                                name = "SettingsButton",
                                type = "Button",
                                isInteractable = true,
                                isVisible = true,
                                position = new { x = 960, y = 460 },
                                size = new { width = 200, height = 60 },
                                text = "Settings"
                            }
                        }
                    }
                }
            },
            timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Navigates menu using keyboard input (convenience wrapper).
    /// </summary>
    [McpServerTool]
    [Description("Navigates UI menus using keyboard. Supports: up, down, left, right, enter, escape, tab.")]
    public static async Task<string> NavigateMenu(
        [Description("Navigation command: up, down, left, right, enter, escape, tab")]
        string direction)
    {
        // Map direction to KeyCode
        var keyCodeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "up", "UpArrow" },
            { "down", "DownArrow" },
            { "left", "LeftArrow" },
            { "right", "RightArrow" },
            { "enter", "Return" },
            { "escape", "Escape" },
            { "tab", "Tab" }
        };

        if (!keyCodeMap.TryGetValue(direction, out var keyCode))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Invalid direction '{direction}'. Must be: up, down, left, right, enter, escape, tab"
            });
        }

        // Use SendKeyInput internally
        var keyResult = await SendKeyInput(keyCode, 0.1f);

        // Also get the updated state after navigation
        var stateResult = await GetGameState();

        var result = new
        {
            success = true,
            message = $"Menu navigation '{direction}' executed",
            direction,
            keyCode,
            keyInputResult = JsonSerializer.Deserialize<object>(keyResult),
            newGameState = JsonSerializer.Deserialize<object>(stateResult),
            timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Starts Unity in Play Mode.
    /// </summary>
    [McpServerTool]
    [Description("Starts Unity Editor in Play Mode. Required for testing runtime functionality.")]
    public static async Task<string> StartPlayMode()
    {
        // TODO: Implement IPC call to Unity Editor
        var result = new
        {
            success = true,
            message = "Play mode start pending Unity integration",
            isPlaying = true,
            timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Stops Unity Play Mode.
    /// </summary>
    [McpServerTool]
    [Description("Stops Unity Editor Play Mode and returns to Edit Mode.")]
    public static async Task<string> StopPlayMode()
    {
        // TODO: Implement IPC call to Unity Editor
        var result = new
        {
            success = true,
            message = "Play mode stop pending Unity integration",
            isPlaying = false,
            timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
