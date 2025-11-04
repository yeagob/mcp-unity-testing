using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace MCP.UnityTesting.Editor
{
    /// <summary>
    /// MCP Server implementation for Unity Editor.
    /// Implements the Model Context Protocol using JSON-RPC 2.0 over stdio.
    /// </summary>
    public class MCPServer : IDisposable
    {
        private const string PROTOCOL_VERSION = "2024-11-05";
        private const string SERVER_NAME = "unity-testing-server";
        private const string SERVER_VERSION = "0.1.0";

        private readonly UnityActionExecutor actionExecutor;
        private bool isInitialized = false;

        public MCPServer()
        {
            actionExecutor = new UnityActionExecutor();
        }

        public void Run(CancellationToken cancellationToken)
        {
            // Use Console for stdio (Standard Error for logs)
            var stdin = Console.OpenStandardInput();
            var stdout = Console.OpenStandardOutput();

            using (var reader = new StreamReader(stdin, Encoding.UTF8))
            using (var writer = new StreamWriter(stdout, Encoding.UTF8) { AutoFlush = true })
            {
                Debug.Log("[MCP] Server ready, waiting for messages on stdin...");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Read line from stdin
                        var line = reader.ReadLine();
                        if (line == null)
                        {
                            // EOF reached
                            Debug.Log("[MCP] EOF on stdin, stopping server");
                            break;
                        }

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        Debug.Log($"[MCP] Received: {line}");

                        // Process the message
                        var response = ProcessMessage(line);

                        if (response != null)
                        {
                            var responseJson = JsonUtility.ToJson(response);
                            Debug.Log($"[MCP] Sending: {responseJson}");
                            writer.WriteLine(responseJson);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[MCP] Error processing message: {ex.Message}");
                        Debug.LogException(ex);

                        // Send error response
                        var errorResponse = new JsonRpcResponse
                        {
                            id = null,
                            error = new JsonRpcError
                            {
                                code = JsonRpcErrorCodes.InternalError,
                                message = ex.Message
                            }
                        };

                        try
                        {
                            writer.WriteLine(JsonUtility.ToJson(errorResponse));
                        }
                        catch
                        {
                            // Failed to send error response
                        }
                    }
                }

                Debug.Log("[MCP] Server loop exited");
            }
        }

        private JsonRpcResponse ProcessMessage(string messageJson)
        {
            JsonRpcRequest request;

            try
            {
                request = JsonUtility.FromJson<JsonRpcRequest>(messageJson);
            }
            catch (Exception ex)
            {
                return new JsonRpcResponse
                {
                    id = null,
                    error = new JsonRpcError
                    {
                        code = JsonRpcErrorCodes.ParseError,
                        message = $"Parse error: {ex.Message}"
                    }
                };
            }

            if (request.method == null)
            {
                return new JsonRpcResponse
                {
                    id = request.id,
                    error = new JsonRpcError
                    {
                        code = JsonRpcErrorCodes.InvalidRequest,
                        message = "Missing method field"
                    }
                };
            }

            Debug.Log($"[MCP] Processing method: {request.method}");

            try
            {
                object result = request.method switch
                {
                    "initialize" => HandleInitialize(request),
                    "initialized" => HandleInitialized(request),
                    "tools/list" => HandleListTools(request),
                    "tools/call" => HandleCallTool(request),
                    _ => throw new Exception($"Method not found: {request.method}")
                };

                return new JsonRpcResponse
                {
                    id = request.id,
                    result = result
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP] Error handling method {request.method}: {ex.Message}");

                return new JsonRpcResponse
                {
                    id = request.id,
                    error = new JsonRpcError
                    {
                        code = JsonRpcErrorCodes.InternalError,
                        message = ex.Message
                    }
                };
            }
        }

        #region Protocol Handlers

        private InitializeResult HandleInitialize(JsonRpcRequest request)
        {
            Debug.Log("[MCP] Handling initialize");

            // Parse the initialize request (simplified, Unity's JsonUtility is limited)
            // In production, use a better JSON library like Newtonsoft.Json

            isInitialized = true;

            return new InitializeResult
            {
                protocolVersion = PROTOCOL_VERSION,
                serverInfo = new ServerInfo
                {
                    name = SERVER_NAME,
                    version = SERVER_VERSION
                },
                capabilities = new ServerCapabilities
                {
                    tools = new ToolsCapability()
                }
            };
        }

        private object HandleInitialized(JsonRpcRequest request)
        {
            Debug.Log("[MCP] Client confirmed initialization");
            // Notification, no response needed
            return null;
        }

        private ListToolsResult HandleListTools(JsonRpcRequest request)
        {
            Debug.Log("[MCP] Listing tools");

            var tools = new List<Tool>
            {
                new Tool
                {
                    name = "echo",
                    description = "Echoes the message back to verify connectivity",
                    inputSchema = new ToolInputSchema
                    {
                        properties = new Dictionary<string, PropertySchema>
                        {
                            { "message", new PropertySchema { type = "string", description = "The message to echo" } }
                        },
                        required = new[] { "message" }
                    }
                },
                new Tool
                {
                    name = "capture_screenshot",
                    description = "Captures a screenshot of the Unity Game View",
                    inputSchema = new ToolInputSchema
                    {
                        properties = new Dictionary<string, PropertySchema>
                        {
                            { "outputPath", new PropertySchema { type = "string", description = "Optional output file path" } }
                        },
                        required = new string[] { }
                    }
                },
                new Tool
                {
                    name = "click_at_position",
                    description = "Simulates a mouse click at specified screen coordinates",
                    inputSchema = new ToolInputSchema
                    {
                        properties = new Dictionary<string, PropertySchema>
                        {
                            { "x", new PropertySchema { type = "number", description = "X coordinate in pixels" } },
                            { "y", new PropertySchema { type = "number", description = "Y coordinate in pixels" } },
                            { "button", new PropertySchema { type = "string", description = "Mouse button: left, right, middle" } }
                        },
                        required = new[] { "x", "y" }
                    }
                },
                new Tool
                {
                    name = "send_key_input",
                    description = "Sends keyboard input to Unity (Unity KeyCode format)",
                    inputSchema = new ToolInputSchema
                    {
                        properties = new Dictionary<string, PropertySchema>
                        {
                            { "keyCode", new PropertySchema { type = "string", description = "Unity KeyCode name (e.g., 'UpArrow', 'Return')" } },
                            { "duration", new PropertySchema { type = "number", description = "Hold duration in seconds" } }
                        },
                        required = new[] { "keyCode" }
                    }
                },
                new Tool
                {
                    name = "get_game_state",
                    description = "Gets current game state including scene, UI elements, and status",
                    inputSchema = new ToolInputSchema
                    {
                        properties = new Dictionary<string, PropertySchema>(),
                        required = new string[] { }
                    }
                },
                new Tool
                {
                    name = "navigate_menu",
                    description = "Navigates UI menus using keyboard (up, down, left, right, enter, escape)",
                    inputSchema = new ToolInputSchema
                    {
                        properties = new Dictionary<string, PropertySchema>
                        {
                            { "direction", new PropertySchema { type = "string", description = "Navigation direction: up, down, left, right, enter, escape" } }
                        },
                        required = new[] { "direction" }
                    }
                },
                new Tool
                {
                    name = "send_gamepad_button",
                    description = "Simulates a gamepad button press (A, B, X, Y, etc.)",
                    inputSchema = new ToolInputSchema
                    {
                        properties = new Dictionary<string, PropertySchema>
                        {
                            { "button", new PropertySchema { type = "string", description = "Button name: A, B, X, Y, LB, RB, LT, RT, Start, Select, LeftStick, RightStick, DPadUp, DPadDown, DPadLeft, DPadRight" } },
                            { "duration", new PropertySchema { type = "number", description = "Hold duration in seconds" } },
                            { "joystickNum", new PropertySchema { type = "number", description = "Joystick number (1-4), default 1" } }
                        },
                        required = new[] { "button" }
                    }
                },
                new Tool
                {
                    name = "send_gamepad_axis",
                    description = "Simulates gamepad analog stick or trigger movement",
                    inputSchema = new ToolInputSchema
                    {
                        properties = new Dictionary<string, PropertySchema>
                        {
                            { "axis", new PropertySchema { type = "string", description = "Axis name: LeftStickX, LeftStickY, RightStickX, RightStickY, LeftTrigger, RightTrigger, DPadX, DPadY" } },
                            { "value", new PropertySchema { type = "number", description = "Axis value (-1.0 to 1.0)" } },
                            { "duration", new PropertySchema { type = "number", description = "Duration to hold the axis value in seconds" } },
                            { "joystickNum", new PropertySchema { type = "number", description = "Joystick number (1-4), default 1" } }
                        },
                        required = new[] { "axis", "value" }
                    }
                },
                new Tool
                {
                    name = "navigate_menu_gamepad",
                    description = "Navigates UI menus using gamepad (up, down, left, right, confirm, cancel)",
                    inputSchema = new ToolInputSchema
                    {
                        properties = new Dictionary<string, PropertySchema>
                        {
                            { "direction", new PropertySchema { type = "string", description = "Navigation: up, down, left, right, confirm, cancel" } },
                            { "joystickNum", new PropertySchema { type = "number", description = "Joystick number (1-4), default 1" } }
                        },
                        required = new[] { "direction" }
                    }
                },
                new Tool
                {
                    name = "get_gamepad_state",
                    description = "Gets the current state of connected gamepads",
                    inputSchema = new ToolInputSchema
                    {
                        properties = new Dictionary<string, PropertySchema>
                        {
                            { "joystickNum", new PropertySchema { type = "number", description = "Joystick number (1-4), or 0 for all" } }
                        },
                        required = new string[] { }
                    }
                }
            };

            return new ListToolsResult { tools = tools.ToArray() };
        }

        private CallToolResult HandleCallTool(JsonRpcRequest request)
        {
            if (!isInitialized)
            {
                throw new Exception("Server not initialized. Call initialize first.");
            }

            // Parse the tool call request
            // Note: Unity's JsonUtility can't handle nested objects well
            // In production, use Newtonsoft.Json or System.Text.Json

            var paramsJson = JsonUtility.ToJson(request.@params);
            var toolRequest = JsonUtility.FromJson<CallToolRequest>(paramsJson);

            Debug.Log($"[MCP] Calling tool: {toolRequest.name}");

            try
            {
                string resultText = actionExecutor.ExecuteTool(toolRequest.name, toolRequest.arguments);

                return new CallToolResult
                {
                    content = new[]
                    {
                        new ToolResultContent
                        {
                            type = "text",
                            text = resultText
                        }
                    },
                    isError = false
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP] Tool execution error: {ex.Message}");

                return new CallToolResult
                {
                    content = new[]
                    {
                        new ToolResultContent
                        {
                            type = "text",
                            text = $"Error: {ex.Message}"
                        }
                    },
                    isError = true
                };
            }
        }

        #endregion

        public void Dispose()
        {
            actionExecutor?.Dispose();
        }
    }
}
