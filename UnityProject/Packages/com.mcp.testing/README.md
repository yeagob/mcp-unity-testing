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

### `send_gamepad_button`
Simulates a gamepad button press.

**Parameters**:
- `button` (string, required): Button name - "A", "B", "X", "Y", "LB", "RB", "Start", "Select", "LeftStick", "RightStick", "DPadUp", "DPadDown", "DPadLeft", "DPadRight"
- `duration` (number, optional): Hold duration in seconds. Default: 0.1
- `joystickNum` (number, optional): Joystick number (1-4). Default: 1

**Returns**: JSON with button press confirmation

**Example**:
```json
{
  "button": "A",
  "duration": 0.1,
  "joystickNum": 1
}
```

**Button Mapping**:
- **Xbox**: A, B, X, Y, LB (Left Bumper), RB (Right Bumper), Start, Select, LeftStick (L3), RightStick (R3)
- **PlayStation**: Cross (A), Circle (B), Square (X), Triangle (Y), L1 (LB), R1 (RB), Options (Start), Share (Select), L3, R3
- **D-Pad**: DPadUp, DPadDown, DPadLeft, DPadRight

### `send_gamepad_axis`
Simulates gamepad analog stick or trigger movement.

**Parameters**:
- `axis` (string, required): Axis name - "LeftStickX", "LeftStickY", "RightStickX", "RightStickY", "LeftTrigger", "RightTrigger", "DPadX", "DPadY"
- `value` (number, required): Axis value from -1.0 to 1.0
- `duration` (number, optional): Duration to hold the axis value in seconds. Default: 0.1
- `joystickNum` (number, optional): Joystick number (1-4). Default: 1

**Returns**: JSON with axis simulation confirmation

**Example**:
```json
{
  "axis": "LeftStickX",
  "value": 0.8,
  "duration": 0.2,
  "joystickNum": 1
}
```

**Axis Values**:
- `-1.0` = Full left/down
- `0.0` = Neutral/centered
- `+1.0` = Full right/up

**Common Axes**:
- **LeftStickX**: -1 (left) to +1 (right)
- **LeftStickY**: -1 (down) to +1 (up)
- **RightStickX**: -1 (left) to +1 (right)
- **RightStickY**: -1 (down) to +1 (up)
- **LeftTrigger/RightTrigger**: 0 (released) to 1 (fully pressed)
- **DPadX/DPadY**: -1, 0, or +1

### `navigate_menu_gamepad`
Convenience wrapper for menu navigation using gamepad.

**Parameters**:
- `direction` (string, required): Navigation command - "up", "down", "left", "right", "confirm", "cancel"
- `joystickNum` (number, optional): Joystick number (1-4). Default: 1

**Returns**: JSON with navigation result and updated game state

**Example**:
```json
{
  "direction": "confirm",
  "joystickNum": 1
}
```

**Navigation Mapping**:
- **up/down/left/right**: Uses left analog stick
- **confirm**: A button (Xbox) / Cross (PlayStation)
- **cancel**: B button (Xbox) / Circle (PlayStation)

### `get_gamepad_state`
Gets information about connected gamepads.

**Parameters**:
- `joystickNum` (number, optional): Specific joystick number (1-4), or 0 for all. Default: 0 (all)

**Returns**: JSON with connected gamepad information

**Example**:
```json
{
  "joystickNum": 0
}
```

**Example Response**:
```json
{
  "success": true,
  "gamepadState": {
    "connectedGamepads": [
      {
        "joystickNum": 1,
        "name": "Xbox One Controller",
        "isConnected": true
      },
      {
        "joystickNum": 2,
        "name": "DualShock 4",
        "isConnected": true
      }
    ],
    "totalConnected": 2
  },
  "timestamp": "2025-11-04T12:34:56.789Z"
}
```

### `get_scene_snapshot`
Captures a comprehensive snapshot of the current scene state.

**This is the most powerful diagnostic tool** - it provides a complete picture of everything happening in your Unity scene at a specific moment in time.

**Parameters**:
- `includeHierarchy` (boolean, optional): Include complete GameObject hierarchy. Default: true
- `includeComponents` (boolean, optional): Include component information for each GameObject. Default: true
- `includeUI` (boolean, optional): Include detailed UI hierarchy with all Canvas elements. Default: true
- `includePerformance` (boolean, optional): Include performance metrics and system info. Default: true
- `maxDepth` (number, optional): Maximum hierarchy depth to capture (prevents overflow on deep scenes). Default: 10

**Returns**: Comprehensive JSON snapshot containing:

**Example**:
```json
{
  "includeHierarchy": true,
  "includeComponents": true,
  "includeUI": true,
  "includePerformance": true,
  "maxDepth": 10
}
```

**Example Response Structure**:
```json
{
  "success": true,
  "snapshot": {
    "timestamp": "2025-11-04T12:34:56.789Z",

    "sceneInfo": {
      "name": "MainMenu",
      "path": "Assets/Scenes/MainMenu.unity",
      "buildIndex": 0,
      "isLoaded": true,
      "isDirty": false,
      "rootCount": 12
    },

    "playMode": {
      "isPlaying": true,
      "isPaused": false,
      "isCompiling": false,
      "timeSinceStartup": 123.45
    },

    "hierarchy": [
      {
        "name": "GameManager",
        "tag": "GameController",
        "layer": "Default",
        "isActive": true,
        "isStatic": false,
        "position": { "x": 0, "y": 0, "z": 0 },
        "rotation": { "x": 0, "y": 0, "z": 0 },
        "scale": { "x": 1, "y": 1, "z": 1 },
        "components": ["Transform", "GameManager", "AudioSource"],
        "childCount": 3,
        "children": [...]
      }
    ],

    "uiHierarchy": {
      "canvases": [
        {
          "name": "MainMenuCanvas",
          "renderMode": "ScreenSpaceOverlay",
          "sortingOrder": 0,
          "isRootCanvas": true,
          "worldCamera": "none",
          "elements": [
            {
              "name": "PlayButton",
              "type": "Button",
              "isInteractable": true,
              "isVisible": true,
              "position": { "x": 0, "y": 50 },
              "size": { "width": 200, "height": 60 },
              "text": "Play Game",
              "childCount": 2
            }
          ]
        }
      ]
    },

    "cameras": [
      {
        "name": "Main Camera",
        "isActive": true,
        "fieldOfView": 60,
        "orthographic": false,
        "depth": -1,
        "clearFlags": "Skybox",
        "targetDisplay": 0
      }
    ],

    "lights": [
      {
        "name": "Directional Light",
        "type": "Directional",
        "intensity": 1.0,
        "range": 10,
        "color": { "r": 1, "g": 0.96, "b": 0.84 }
      }
    ],

    "audioSources": [
      {
        "name": "BackgroundMusic",
        "isPlaying": true,
        "clip": "MenuTheme",
        "volume": 0.7,
        "loop": true,
        "mute": false
      }
    ],

    "eventSystem": {
      "isActive": true,
      "currentSelectedGameObject": "PlayButton",
      "firstSelectedGameObject": "PlayButton"
    },

    "inputState": {
      "mousePosition": { "x": 960, "y": 540, "z": 0 },
      "mousePresent": true,
      "touchSupported": false,
      "touchCount": 0,
      "anyKey": false,
      "anyKeyDown": false
    },

    "connectedGamepads": [
      {
        "joystickNum": 1,
        "name": "Xbox One Controller"
      }
    ],

    "performance": {
      "targetFrameRate": 60,
      "vSyncCount": 1,
      "qualityLevel": 2,
      "qualityLevelName": "High",
      "pixelLightCount": 4,
      "shadowDistance": 150,
      "systemInfo": {
        "deviceModel": "MacBookPro18,1",
        "deviceType": "Desktop",
        "graphicsDeviceName": "AMD Radeon Pro",
        "graphicsMemorySize": 8192,
        "systemMemorySize": 32768,
        "processorType": "Apple M1 Pro",
        "processorCount": 10
      }
    }
  }
}
```

**What This Tool Captures**:

1. **Scene Information**: Name, path, build index, load state
2. **Play Mode State**: Whether playing, paused, compiling
3. **Complete GameObject Hierarchy**: All active objects with their transforms, components, and children
4. **Detailed UI Hierarchy**: All Canvas elements with buttons, text, images, input fields, toggles, sliders, etc.
5. **Cameras**: All active cameras with their settings (FOV, orthographic, depth, clear flags)
6. **Lights**: All lights with type, intensity, range, color
7. **Audio Sources**: All audio sources with playing state, clips, volume, loop settings
8. **EventSystem**: Current selected UI element
9. **Input State**: Current mouse position, touch support, key presses
10. **Connected Gamepads**: All connected controllers
11. **Performance Metrics**: Frame rate, quality settings, system information

**Use Cases**:

- **Debugging**: Get complete scene state when bug occurs
- **Test Reports**: Comprehensive snapshot of test conditions
- **State Verification**: Confirm scene is in expected state
- **Performance Analysis**: Check quality settings and system capabilities
- **UI Analysis**: Detailed breakdown of all UI elements and their states
- **Before/After Comparisons**: Capture state before and after actions

**Tips**:
- Use `maxDepth` parameter to limit hierarchy depth on very complex scenes
- Set `includeComponents: false` for faster snapshots when you only need hierarchy
- Combine with `capture_screenshot` for visual + data analysis
- Use this tool at the start of test sequences to understand initial state

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

### Testing Gamepad Navigation

```
Prompt for Claude:

"Test gamepad controller navigation:
1. Check if a gamepad is connected using get_gamepad_state
2. Navigate the menu using the left stick (gamepad)
3. Select options using the A button
4. Test D-pad navigation
5. Compare gamepad navigation with keyboard navigation
6. Report any issues or inconsistencies"
```

### Multi-Input Method Testing

```
Prompt for Claude:

"Test all input methods (keyboard, mouse, gamepad):
1. Navigate menu with keyboard arrows and Enter
2. Navigate same menu by clicking with mouse
3. Navigate same menu with gamepad left stick and A button
4. Verify all three methods reach the same destinations
5. Test edge cases (rapid inputs, simultaneous inputs)
6. Generate comparison report with screenshots for each method"
```

### Comprehensive Scene Analysis

```
Prompt for Claude:

"Perform a comprehensive analysis of the current scene:
1. Use get_scene_snapshot to capture the complete scene state
2. Analyze the GameObject hierarchy and identify any issues
3. Check all UI elements for proper configuration
4. Review camera settings
5. Check audio sources and their states
6. Analyze performance metrics
7. Generate a detailed report of findings with recommendations"
```

### Bug Reporting with Full Context

```
Prompt for Claude:

"I encountered a bug. Please help me document it:
1. Use get_scene_snapshot to capture the exact scene state when the bug occurs
2. Take a screenshot for visual reference
3. Analyze the snapshot for potential causes (missing components, incorrect states, etc.)
4. Generate a detailed bug report including:
   - Scene state at time of bug
   - All relevant GameObjects and their states
   - UI element states
   - Input state
   - Performance metrics
   - Visual screenshot
   - Potential root causes identified"
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
