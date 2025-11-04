using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MCP.UnityTesting.Editor
{
    /// <summary>
    /// Manages the MCP server lifecycle within Unity Editor.
    /// The server runs on a background thread and communicates via stdio (standard input/output).
    /// </summary>
    [InitializeOnLoad]
    public class MCPServerManager
    {
        private static MCPServer server;
        private static Thread serverThread;
        private static bool isRunning = false;
        private static CancellationTokenSource cancellationTokenSource;

        // Auto-start option (can be toggled via menu)
        private const string AUTO_START_PREF_KEY = "MCP.AutoStart";
        private static bool AutoStartEnabled
        {
            get => EditorPrefs.GetBool(AUTO_START_PREF_KEY, false);
            set => EditorPrefs.SetBool(AUTO_START_PREF_KEY, value);
        }

        static MCPServerManager()
        {
            UnityEngine.Debug.Log("[MCP] MCPServerManager initialized");

            // Hook into editor events
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnEditorQuitting;

            // Auto-start if enabled
            if (AutoStartEnabled)
            {
                UnityEngine.Debug.Log("[MCP] Auto-start enabled, starting server...");
                StartServer();
            }
        }

        #region Menu Items

        [MenuItem("Tools/MCP Testing/Start Server", false, 1)]
        public static void StartServerMenu()
        {
            StartServer();
        }

        [MenuItem("Tools/MCP Testing/Start Server", true)]
        public static bool StartServerMenuValidate()
        {
            return !isRunning;
        }

        [MenuItem("Tools/MCP Testing/Stop Server", false, 2)]
        public static void StopServerMenu()
        {
            StopServer();
        }

        [MenuItem("Tools/MCP Testing/Stop Server", true)]
        public static bool StopServerMenuValidate()
        {
            return isRunning;
        }

        [MenuItem("Tools/MCP Testing/Toggle Auto-Start", false, 20)]
        public static void ToggleAutoStartMenu()
        {
            AutoStartEnabled = !AutoStartEnabled;
            UnityEngine.Debug.Log($"[MCP] Auto-start {(AutoStartEnabled ? "enabled" : "disabled")}");
        }

        [MenuItem("Tools/MCP Testing/Toggle Auto-Start", true)]
        public static bool ToggleAutoStartMenuValidate()
        {
            Menu.SetChecked("Tools/MCP Testing/Toggle Auto-Start", AutoStartEnabled);
            return true;
        }

        [MenuItem("Tools/MCP Testing/Server Status", false, 40)]
        public static void ShowServerStatus()
        {
            string status = isRunning ? "Running" : "Stopped";
            EditorUtility.DisplayDialog(
                "MCP Server Status",
                $"Server Status: {status}\n" +
                $"Auto-Start: {(AutoStartEnabled ? "Enabled" : "Disabled")}\n" +
                $"Transport: stdio (Standard Input/Output)",
                "OK"
            );
        }

        #endregion

        #region Server Lifecycle

        public static void StartServer()
        {
            if (isRunning)
            {
                UnityEngine.Debug.LogWarning("[MCP] Server is already running");
                return;
            }

            try
            {
                UnityEngine.Debug.Log("[MCP] Starting MCP Server...");

                cancellationTokenSource = new CancellationTokenSource();
                server = new MCPServer();

                // Start server on background thread (stdio blocks)
                serverThread = new Thread(() => RunServer(cancellationTokenSource.Token))
                {
                    IsBackground = true,
                    Name = "MCP Server Thread"
                };

                serverThread.Start();
                isRunning = true;

                UnityEngine.Debug.Log("[MCP] Server started successfully");
                UnityEngine.Debug.Log("[MCP] Listening on stdin/stdout for MCP protocol messages");
                UnityEngine.Debug.Log("[MCP] Connect using Claude Desktop or other MCP clients");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MCP] Failed to start server: {ex.Message}");
                UnityEngine.Debug.LogException(ex);
                isRunning = false;
            }
        }

        public static void StopServer()
        {
            if (!isRunning)
            {
                UnityEngine.Debug.LogWarning("[MCP] Server is not running");
                return;
            }

            try
            {
                UnityEngine.Debug.Log("[MCP] Stopping MCP Server...");

                cancellationTokenSource?.Cancel();

                // Give the server thread time to gracefully shut down
                if (serverThread != null && serverThread.IsAlive)
                {
                    if (!serverThread.Join(TimeSpan.FromSeconds(5)))
                    {
                        UnityEngine.Debug.LogWarning("[MCP] Server thread did not stop gracefully, aborting");
                        serverThread.Abort();
                    }
                }

                server?.Dispose();
                server = null;
                serverThread = null;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;

                isRunning = false;
                UnityEngine.Debug.Log("[MCP] Server stopped");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MCP] Error stopping server: {ex.Message}");
                UnityEngine.Debug.LogException(ex);
            }
        }

        private static void RunServer(CancellationToken cancellationToken)
        {
            try
            {
                UnityEngine.Debug.Log("[MCP] Server thread started");
                server.Run(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                UnityEngine.Debug.Log("[MCP] Server cancelled");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MCP] Server error: {ex.Message}");
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                UnityEngine.Debug.Log("[MCP] Server thread exiting");
            }
        }

        #endregion

        #region Editor Events

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // Server can continue running during play mode
            // Tools will handle play mode requirements internally
            UnityEngine.Debug.Log($"[MCP] Play mode changed: {state}");
        }

        private static void OnEditorQuitting()
        {
            if (isRunning)
            {
                UnityEngine.Debug.Log("[MCP] Editor quitting, stopping server...");
                StopServer();
            }
        }

        #endregion
    }
}
