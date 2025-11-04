using System;
using System.Collections.Generic;
using UnityEngine;

namespace MCP.UnityTesting.Editor
{
    /// <summary>
    /// JSON-RPC 2.0 protocol classes for MCP.
    /// Based on Model Context Protocol specification.
    /// </summary>

    #region JSON-RPC Base Types

    [Serializable]
    public class JsonRpcMessage
    {
        public string jsonrpc = "2.0";
    }

    [Serializable]
    public class JsonRpcRequest : JsonRpcMessage
    {
        public object id;
        public string method;
        public object @params;
    }

    [Serializable]
    public class JsonRpcResponse : JsonRpcMessage
    {
        public object id;
        public object result;
        public JsonRpcError error;
    }

    [Serializable]
    public class JsonRpcError
    {
        public int code;
        public string message;
        public object data;
    }

    [Serializable]
    public class JsonRpcNotification : JsonRpcMessage
    {
        public string method;
        public object @params;
    }

    #endregion

    #region MCP Protocol Types

    [Serializable]
    public class ServerInfo
    {
        public string name;
        public string version;
    }

    [Serializable]
    public class ServerCapabilities
    {
        public ToolsCapability tools;
        public PromptsCapability prompts;
        public ResourcesCapability resources;
    }

    [Serializable]
    public class ToolsCapability
    {
        // Empty object indicates tools are supported
    }

    [Serializable]
    public class PromptsCapability
    {
        // Empty object indicates prompts are supported
    }

    [Serializable]
    public class ResourcesCapability
    {
        // Empty object indicates resources are supported
    }

    [Serializable]
    public class InitializeRequest
    {
        public string protocolVersion;
        public ClientCapabilities capabilities;
        public ClientInfo clientInfo;
    }

    [Serializable]
    public class ClientInfo
    {
        public string name;
        public string version;
    }

    [Serializable]
    public class ClientCapabilities
    {
        // Client capabilities (roots, sampling, etc.)
    }

    [Serializable]
    public class InitializeResult
    {
        public string protocolVersion;
        public ServerCapabilities capabilities;
        public ServerInfo serverInfo;
    }

    [Serializable]
    public class Tool
    {
        public string name;
        public string description;
        public ToolInputSchema inputSchema;
    }

    [Serializable]
    public class ToolInputSchema
    {
        public string type = "object";
        public Dictionary<string, PropertySchema> properties;
        public string[] required;
    }

    [Serializable]
    public class PropertySchema
    {
        public string type;
        public string description;
    }

    [Serializable]
    public class ListToolsResult
    {
        public Tool[] tools;
    }

    [Serializable]
    public class CallToolRequest
    {
        public string name;
        public Dictionary<string, object> arguments;
    }

    [Serializable]
    public class CallToolResult
    {
        public ToolResultContent[] content;
        public bool isError;
    }

    [Serializable]
    public class ToolResultContent
    {
        public string type;
        public string text;
    }

    #endregion

    #region Error Codes

    public static class JsonRpcErrorCodes
    {
        public const int ParseError = -32700;
        public const int InvalidRequest = -32600;
        public const int MethodNotFound = -32601;
        public const int InvalidParams = -32602;
        public const int InternalError = -32603;
    }

    #endregion
}
