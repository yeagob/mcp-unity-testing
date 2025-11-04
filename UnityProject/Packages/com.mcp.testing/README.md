# MCP Unity Testing Server

**Model Context Protocol server for Unity automated game testing**

Author: Santiago Dopazo Hilario ([@SantiagoGameLover](https://github.com/SantiagoGameLover))
Company: Montseny XR

## Overview

This Unity package implements a Model Context Protocol (MCP) server that runs directly in Unity Editor. It allows AI models (like Claude, ChatGPT, etc.) to interact with your Unity game for automated testing of UI flows, menu navigation, and game interactions.

## Features

### Phase 1 (Current)

- **Screenshot Capture**: Take screenshots of the Game View for visual analysis
- **Mouse Click Simulation**: Click at specific screen coordinates to interact with UI
- **Keyboard Input**: Send keyboard inputs to navigate menus and UI
- **Game State Queries**: Get current scene, active UI elements, and game status
- **Menu Navigation**: Automated menu navigation using keyboard controls

### Roadmap

- Real-time video streaming
- Advanced input simulation (gamepad support)
- Gameplay testing (physics, collisions, performance)
- Multi-platform testing
- CI/CD integration

## Installation

### Option 1: Unity Package Manager (Local)

1. Copy the `com.mcp.testing` folder to your Unity project's `Packages/` directory
2. Unity will automatically detect and import the package

### Option 2: Unity Package Manager (Git)

Add this line to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.mcp.testing": "https://github.com/yourusername/mcp-unity-testing.git?path=/UnityProject/Packages/com.mcp.testing"
  }
}
```

## Quick Start

### 1. Start the MCP Server in Unity

In Unity Editor:
- Go to **Tools > MCP Testing > Start Server**
- The server will start listening on stdin/stdout
- Check the Console for confirmation: `[MCP] Server started successfully`

### 2. Configure Your MCP Client

For **Claude Desktop**, add this to your MCP settings file:

**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`
**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "mcpServers": {
    "unity-testing": {
      "command": "unity",
      "args": [
        "-batchmode",
        "-projectPath",
        "/path/to/your/UnityProject",
        "-executeMethod",
        "MCP.UnityTesting.Editor.MCPServerManager.StartServer",
        "-logFile",
        "-"
      ]
    }
  }
}
```

**Note**: The above configuration launches Unity in batch mode. For development, you can also:

1. Start Unity Editor normally with your project
2. Manually start the server: **Tools > MCP Testing > Start Server**
3. Configure Claude to connect to the already-running Unity instance (advanced)

### 3. Use the Server from Claude

Example prompt:

```
I'm testing a Unity game. Can you:
1. Take a screenshot to see the current menu
2. Navigate to the Settings menu using keyboard
3. Generate a test report
```

## Available Tools

### `echo`
Tests connectivity with the MCP server.

**Parameters**:
- `message` (string, required): Message to echo back

**Example**:
```json
{
  "message": "Hello Unity!"
}
```

### `capture_screenshot`
Captures a screenshot of the Unity Game View.

**Parameters**:
- `outputPath` (string, optional): File path for the screenshot. Defaults to `Screenshots/screenshot_[timestamp].png`

**Returns**: JSON with success status and file path

**Example**:
```json
{
  "outputPath": "Screenshots/main_menu.png"
}
```

### `click_at_position`
Simulates a mouse click at screen coordinates.

**Parameters**:
- `x` (number, required): X coordinate in pixels (0 = left edge)
- `y` (number, required): Y coordinate in pixels (0 = top edge)
- `button` (string, optional): Mouse button - "left", "right", or "middle". Default: "left"

**Returns**: JSON with clicked UI element name

**Example**:
```json
{
  "x": 960,
  "y": 540,
  "button": "left"
}
```

### `send_key_input`
Sends keyboard input to Unity.

**Parameters**:
- `keyCode` (string, required): Unity KeyCode name (e.g., "UpArrow", "Return", "Escape", "Space")
- `duration` (number, optional): Hold duration in seconds. Default: 0.1

**Returns**: JSON with input confirmation

**Example**:
```json
{
  "keyCode": "UpArrow",
  "duration": 0.1
}
```

**Common KeyCodes**:
- Navigation: `UpArrow`, `DownArrow`, `LeftArrow`, `RightArrow`
- Confirm: `Return`, `Space`
- Cancel: `Escape`
- Tab: `Tab`
- Letters: `A`, `B`, `C`, etc.
- Numbers: `Alpha0`, `Alpha1`, etc.

Full list: [Unity KeyCode Documentation](https://docs.unity3d.com/ScriptReference/KeyCode.html)

### `get_game_state`
Retrieves current game state information.

**Parameters**: None

**Returns**: JSON with:
- `currentScene`: Active scene name
- `isPlaying`: Whether Editor is in Play mode
- `isPaused`: Whether game is paused
- `activeCanvases`: Array of active Canvas names
- `selectedUIElement`: Currently selected UI element
- `gameTime`: Editor time since startup

**Example Response**:
```json
{
  "success": true,
  "gameState": {
    "currentScene": "MainMenu",
    "isPlaying": true,
    "isPaused": false,
    "activeCanvases": ["MainMenuCanvas", "HUDCanvas"],
    "selectedUIElement": "PlayButton",
    "gameTime": 123.45
  }
}
```

### `navigate_menu`
Convenience wrapper for menu navigation using keyboard.

**Parameters**:
- `direction` (string, required): Navigation command - "up", "down", "left", "right", "enter", "escape", "tab"

**Returns**: JSON with navigation result and updated game state

**Example**:
```json
{
  "direction": "down"
}
```

## Usage Patterns

### Testing Menu Navigation

```
Prompt for Claude:

"Test the main menu navigation:
1. Get the current game state
2. Take a screenshot
3. Navigate down through all menu options
4. Take a screenshot at each option
5. Try clicking each option
6. Report what you found"
```

### Testing Keyboard vs Mouse Navigation

```
Prompt for Claude:

"Compare keyboard and mouse navigation:
1. Test navigating the menu using arrow keys
2. Test navigating by clicking each button
3. Verify both methods work correctly
4. Report any differences or issues"
```

### Automated Test Suite

```
Prompt for Claude:

"Run a complete UI test suite:
1. Start from the main menu
2. Navigate to Settings
3. Change a setting
4. Return to main menu
5. Start the game
6. Pause and access pause menu
7. Resume and quit to main menu
8. Generate a detailed test report with screenshots"
```

## Editor Menu Options

### Tools > MCP Testing > Start Server
Starts the MCP server. Server will listen on stdin/stdout for protocol messages.

### Tools > MCP Testing > Stop Server
Stops the running MCP server.

### Tools > MCP Testing > Toggle Auto-Start
Enable/disable automatic server startup when Unity Editor launches.

### Tools > MCP Testing > Server Status
Displays current server status and configuration.

## Architecture

```
┌─────────────────────────────────┐
│  MCP Client (Claude Desktop)   │
└────────────┬────────────────────┘
             │ JSON-RPC 2.0
             │ via stdio
┌────────────▼────────────────────┐
│  Unity Editor Process           │
│                                 │
│  ┌───────────────────────────┐ │
│  │ MCPServer                 │ │
│  │ - JSON-RPC handling       │ │
│  │ - Protocol implementation │ │
│  └───────────┬───────────────┘ │
│              │                  │
│  ┌───────────▼───────────────┐ │
│  │ UnityActionExecutor       │ │
│  │ - Screenshot capture      │ │
│  │ - Input simulation        │ │
│  │ - State queries           │ │
│  └───────────────────────────┘ │
│                                 │
│  Unity Game View                │
└─────────────────────────────────┘
```

## Technical Details

### Threading
- Server runs on a background thread to handle blocking stdio operations
- Unity API calls are marshalled to the main thread using `EditorApplication.update`
- Thread-safe queue system ensures proper synchronization

### Play Mode
- Server works in both Edit Mode and Play Mode
- Most testing tools require Play Mode to be active
- Screenshots and state queries work in both modes

### Input Simulation
- Uses Unity's EventSystem for UI navigation
- Direct button click invocation for reliable testing
- Keyboard navigation follows Unity's Selectable system

## Limitations & Known Issues

### Current Limitations

1. **JSON Serialization**: Uses Unity's `JsonUtility` which has limitations with nested objects. Complex data structures are serialized as strings.

2. **Input Simulation**: Full input simulation (like actual keypresses) is limited in Editor. The current implementation uses EventSystem navigation which works well for UI but may not trigger all game input handlers.

3. **Play Mode Required**: Most interactive features require Play Mode. The tools don't automatically enter Play Mode (to give you control).

### Recommended Solutions

1. **For Production**: Replace `JsonUtility` with `Newtonsoft.Json` or `System.Text.Json` for better JSON handling.

2. **For Advanced Input**: Integrate with the new Unity Input System package for more comprehensive input simulation.

3. **For Batch Testing**: Create automation scripts that enter/exit Play Mode as needed.

## Troubleshooting

### Server doesn't start
- Check Unity Console for errors
- Ensure no other MCP server is running on the same stdio
- Try restarting Unity Editor

### Tools timeout
- Increase timeout values in `UnityActionExecutor.cs`
- Check that Unity isn't frozen or busy
- Verify Play Mode is active if required

### Screenshot folder issues
- Ensure the Screenshots folder exists or will be created
- Check file permissions
- Verify the output path is valid

### Input simulation not working
- Verify EventSystem exists in the scene
- Check that UI elements have proper Selectable components
- Ensure Play Mode is active

## Development

### Adding New Tools

1. Add tool definition to `MCPServer.HandleListTools()`:
```csharp
new Tool
{
    name = "my_new_tool",
    description = "Description of the tool",
    inputSchema = new ToolInputSchema
    {
        properties = new Dictionary<string, PropertySchema>
        {
            { "param1", new PropertySchema { type = "string", description = "Parameter description" } }
        },
        required = new[] { "param1" }
    }
}
```

2. Add handler in `MCPServer.HandleCallTool()`:
```csharp
case "my_new_tool": return HandleMyNewTool(request);
```

3. Implement executor in `UnityActionExecutor.ExecuteTool()`:
```csharp
case "my_new_tool": return ExecuteMyNewTool(arguments);
```

4. Implement the tool method:
```csharp
private string ExecuteMyNewTool(Dictionary<string, object> args)
{
    // Your implementation
    var result = new { success = true, data = "..." };
    return JsonUtility.ToJson(result);
}
```

### Testing

1. Start the server in Unity
2. Use Claude Desktop or another MCP client
3. Test each tool individually
4. Check Unity Console for detailed logs with `[MCP]` prefix

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please submit issues and pull requests to the repository.

## Contact

- Author: Santiago Dopazo Hilario ([@SantiagoGameLover](https://github.com/SantiagoGameLover))
- Company: Montseny XR
- Email: contact@montsenyxr.com

## References

- [Model Context Protocol](https://modelcontextprotocol.io/)
- [MCP Specification](https://modelcontextprotocol.io/specification/)
- [Claude Desktop](https://claude.ai/download)
- [Unity KeyCode Reference](https://docs.unity3d.com/ScriptReference/KeyCode.html)
