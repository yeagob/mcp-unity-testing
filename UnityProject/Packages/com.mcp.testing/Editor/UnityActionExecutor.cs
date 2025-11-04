using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace MCP.UnityTesting.Editor
{
    /// <summary>
    /// Executes Unity actions requested by MCP tools.
    /// Handles screenshot capture, input simulation, and game state queries.
    /// Author: Santiago Dopazo Hilario (@SantiagoGameLover)
    /// Company: Montseny XR
    /// </summary>
    public class UnityActionExecutor : IDisposable
    {
        private Queue<Action> mainThreadActions = new Queue<Action>();
        private object lockObject = new object();

        public UnityActionExecutor()
        {
            // Register for Unity Editor update to process queued actions
            EditorApplication.update += ProcessMainThreadActions;
        }

        public void Dispose()
        {
            EditorApplication.update -= ProcessMainThreadActions;
        }

        /// <summary>
        /// Executes a tool and returns the result as JSON string.
        /// </summary>
        public string ExecuteTool(string toolName, Dictionary<string, object> arguments)
        {
            arguments = arguments ?? new Dictionary<string, object>();

            return toolName switch
            {
                "echo" => ExecuteEcho(arguments),
                "capture_screenshot" => ExecuteCaptureScreenshot(arguments),
                "click_at_position" => ExecuteClickAtPosition(arguments),
                "send_key_input" => ExecuteSendKeyInput(arguments),
                "get_game_state" => ExecuteGetGameState(arguments),
                "navigate_menu" => ExecuteNavigateMenu(arguments),
                _ => throw new Exception($"Unknown tool: {toolName}")
            };
        }

        #region Tool Implementations

        private string ExecuteEcho(Dictionary<string, object> args)
        {
            var message = GetStringArg(args, "message", "");
            var result = new
            {
                success = true,
                echo = $"Unity MCP Server: {message}",
                timestamp = DateTime.UtcNow.ToString("o")
            };
            return JsonUtility.ToJson(result);
        }

        private string ExecuteCaptureScreenshot(Dictionary<string, object> args)
        {
            var outputPath = GetStringArg(args, "outputPath", null);

            // Generate default path if not provided
            if (string.IsNullOrEmpty(outputPath))
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                outputPath = Path.Combine(Application.dataPath, "..", "Screenshots", $"screenshot_{timestamp}.png");
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                // Capture screenshot on main thread
                string capturedPath = null;
                bool completed = false;
                Exception error = null;

                RunOnMainThread(() =>
                {
                    try
                    {
                        ScreenCapture.CaptureScreenshot(outputPath);
                        capturedPath = Path.GetFullPath(outputPath);
                        Debug.Log($"[MCP] Screenshot captured: {capturedPath}");
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                    finally
                    {
                        completed = true;
                    }
                });

                // Wait for completion
                WaitForMainThreadAction(ref completed, 5000);

                if (error != null)
                    throw error;

                var result = new
                {
                    success = true,
                    message = "Screenshot captured successfully",
                    outputPath = capturedPath,
                    timestamp = DateTime.UtcNow.ToString("o")
                };

                return JsonUtility.ToJson(result);
            }
            catch (Exception ex)
            {
                var result = new
                {
                    success = false,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                return JsonUtility.ToJson(result);
            }
        }

        private string ExecuteClickAtPosition(Dictionary<string, object> args)
        {
            var x = GetIntArg(args, "x", 0);
            var y = GetIntArg(args, "y", 0);
            var button = GetStringArg(args, "button", "left");

            try
            {
                GameObject clickedObject = null;
                bool completed = false;
                Exception error = null;

                RunOnMainThread(() =>
                {
                    try
                    {
                        // Perform raycast to find UI element
                        var pointerEventData = new PointerEventData(EventSystem.current)
                        {
                            position = new Vector2(x, y)
                        };

                        var raycastResults = new System.Collections.Generic.List<RaycastResult>();

                        if (EventSystem.current != null)
                        {
                            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

                            if (raycastResults.Count > 0)
                            {
                                clickedObject = raycastResults[0].gameObject;

                                // Try to execute click on the UI element
                                var clickHandler = clickedObject.GetComponent<UnityEngine.UI.Button>();
                                if (clickHandler != null)
                                {
                                    clickHandler.onClick.Invoke();
                                    Debug.Log($"[MCP] Clicked button: {clickedObject.name}");
                                }
                                else
                                {
                                    Debug.Log($"[MCP] Raycast hit: {clickedObject.name} (no button component)");
                                }
                            }
                            else
                            {
                                Debug.Log($"[MCP] No UI element at position ({x}, {y})");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[MCP] No EventSystem found in scene");
                        }
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                    finally
                    {
                        completed = true;
                    }
                });

                WaitForMainThreadAction(ref completed, 5000);

                if (error != null)
                    throw error;

                var result = new
                {
                    success = true,
                    message = "Click simulated",
                    x,
                    y,
                    button,
                    uiElementClicked = clickedObject != null ? clickedObject.name : "none",
                    timestamp = DateTime.UtcNow.ToString("o")
                };

                return JsonUtility.ToJson(result);
            }
            catch (Exception ex)
            {
                var result = new
                {
                    success = false,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                return JsonUtility.ToJson(result);
            }
        }

        private string ExecuteSendKeyInput(Dictionary<string, object> args)
        {
            var keyCode = GetStringArg(args, "keyCode", "");
            var duration = GetFloatArg(args, "duration", 0.1f);

            try
            {
                // Parse KeyCode
                if (!Enum.TryParse<KeyCode>(keyCode, out var key))
                {
                    throw new Exception($"Invalid KeyCode: {keyCode}");
                }

                bool completed = false;
                Exception error = null;

                RunOnMainThread(() =>
                {
                    try
                    {
                        // Note: Simulating key input in Editor is complex
                        // This is a placeholder that logs the action
                        // For actual input simulation, you'd need to use reflection
                        // to access Unity's internal event system or use Input System package

                        Debug.Log($"[MCP] Simulating key press: {keyCode} for {duration}s");

                        // If there's a selected UI element, try to navigate
                        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                        {
                            var selectable = EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.Selectable>();
                            if (selectable != null)
                            {
                                // Simulate navigation
                                var navigation = key switch
                                {
                                    KeyCode.UpArrow => selectable.FindSelectableOnUp(),
                                    KeyCode.DownArrow => selectable.FindSelectableOnDown(),
                                    KeyCode.LeftArrow => selectable.FindSelectableOnLeft(),
                                    KeyCode.RightArrow => selectable.FindSelectableOnRight(),
                                    _ => null
                                };

                                if (navigation != null)
                                {
                                    EventSystem.current.SetSelectedGameObject(navigation.gameObject);
                                    Debug.Log($"[MCP] Navigated to: {navigation.gameObject.name}");
                                }
                                else if (key == KeyCode.Return || key == KeyCode.KeypadEnter)
                                {
                                    // Try to click the selected button
                                    var button = selectable as UnityEngine.UI.Button;
                                    if (button != null)
                                    {
                                        button.onClick.Invoke();
                                        Debug.Log($"[MCP] Activated button: {button.gameObject.name}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                    finally
                    {
                        completed = true;
                    }
                });

                WaitForMainThreadAction(ref completed, 5000);

                if (error != null)
                    throw error;

                var result = new
                {
                    success = true,
                    message = "Key input simulated",
                    keyCode,
                    duration,
                    timestamp = DateTime.UtcNow.ToString("o")
                };

                return JsonUtility.ToJson(result);
            }
            catch (Exception ex)
            {
                var result = new
                {
                    success = false,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                return JsonUtility.ToJson(result);
            }
        }

        private string ExecuteGetGameState(Dictionary<string, object> args)
        {
            try
            {
                GameStateInfo state = null;
                bool completed = false;
                Exception error = null;

                RunOnMainThread(() =>
                {
                    try
                    {
                        var scene = SceneManager.GetActiveScene();
                        var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>();

                        var canvasNames = new List<string>();
                        foreach (var canvas in canvases)
                        {
                            if (canvas.gameObject.activeInHierarchy)
                            {
                                canvasNames.Add(canvas.gameObject.name);
                            }
                        }

                        string selectedElement = "none";
                        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                        {
                            selectedElement = EventSystem.current.currentSelectedGameObject.name;
                        }

                        state = new GameStateInfo
                        {
                            currentScene = scene.name,
                            isPlaying = EditorApplication.isPlaying,
                            isPaused = EditorApplication.isPaused,
                            activeCanvases = canvasNames.ToArray(),
                            selectedUIElement = selectedElement,
                            gameTime = (float)EditorApplication.timeSinceStartup
                        };
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                    finally
                    {
                        completed = true;
                    }
                });

                WaitForMainThreadAction(ref completed, 5000);

                if (error != null)
                    throw error;

                var result = new
                {
                    success = true,
                    gameState = state,
                    timestamp = DateTime.UtcNow.ToString("o")
                };

                return JsonUtility.ToJson(result);
            }
            catch (Exception ex)
            {
                var result = new
                {
                    success = false,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                return JsonUtility.ToJson(result);
            }
        }

        private string ExecuteNavigateMenu(Dictionary<string, object> args)
        {
            var direction = GetStringArg(args, "direction", "").ToLower();

            // Map direction to KeyCode
            var keyCode = direction switch
            {
                "up" => "UpArrow",
                "down" => "DownArrow",
                "left" => "LeftArrow",
                "right" => "RightArrow",
                "enter" => "Return",
                "escape" => "Escape",
                "tab" => "Tab",
                _ => null
            };

            if (keyCode == null)
            {
                var result = new
                {
                    success = false,
                    error = $"Invalid direction: {direction}. Use: up, down, left, right, enter, escape, tab",
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                return JsonUtility.ToJson(result);
            }

            // Execute key input
            var keyArgs = new Dictionary<string, object>
            {
                { "keyCode", keyCode },
                { "duration", 0.1f }
            };
            var keyResult = ExecuteSendKeyInput(keyArgs);

            // Get updated state
            var stateResult = ExecuteGetGameState(new Dictionary<string, object>());

            var combinedResult = new
            {
                success = true,
                message = $"Navigation '{direction}' executed",
                direction,
                keyCode,
                keyInputResult = keyResult,
                newGameState = stateResult,
                timestamp = DateTime.UtcNow.ToString("o")
            };

            return JsonUtility.ToJson(combinedResult);
        }

        #endregion

        #region Helper Methods

        private void RunOnMainThread(Action action)
        {
            lock (lockObject)
            {
                mainThreadActions.Enqueue(action);
            }
        }

        private void ProcessMainThreadActions()
        {
            lock (lockObject)
            {
                while (mainThreadActions.Count > 0)
                {
                    var action = mainThreadActions.Dequeue();
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[MCP] Error executing main thread action: {ex.Message}");
                        Debug.LogException(ex);
                    }
                }
            }
        }

        private void WaitForMainThreadAction(ref bool completed, int timeoutMs)
        {
            var startTime = DateTime.Now;
            while (!completed)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMs)
                {
                    throw new TimeoutException($"Main thread action timed out after {timeoutMs}ms");
                }
                System.Threading.Thread.Sleep(10);
            }
        }

        private string GetStringArg(Dictionary<string, object> args, string key, string defaultValue)
        {
            if (args.TryGetValue(key, out var value))
            {
                return value?.ToString() ?? defaultValue;
            }
            return defaultValue;
        }

        private int GetIntArg(Dictionary<string, object> args, string key, int defaultValue)
        {
            if (args.TryGetValue(key, out var value))
            {
                if (value is int intValue)
                    return intValue;
                if (int.TryParse(value?.ToString(), out var parsed))
                    return parsed;
            }
            return defaultValue;
        }

        private float GetFloatArg(Dictionary<string, object> args, string key, float defaultValue)
        {
            if (args.TryGetValue(key, out var value))
            {
                if (value is float floatValue)
                    return floatValue;
                if (float.TryParse(value?.ToString(), out var parsed))
                    return parsed;
            }
            return defaultValue;
        }

        #endregion
    }

    [Serializable]
    public class GameStateInfo
    {
        public string currentScene;
        public bool isPlaying;
        public bool isPaused;
        public string[] activeCanvases;
        public string selectedUIElement;
        public float gameTime;
    }
}
