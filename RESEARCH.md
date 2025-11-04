# Investigación: Servidor MCP para Testing Automatizado en Unity

## Resumen Ejecutivo

**Objetivo**: Crear un servidor Model Context Protocol (MCP) para Unity que permita a modelos de IA automatizar pruebas de videojuegos, específicamente flujos de menús y navegación.

**Conclusión**: **SÍ ES VIABLE** implementar un servidor MCP para Unity. La arquitectura recomendada es un **proceso servidor MCP externo** que se comunica con Unity Editor mediante stdio/JSON-RPC.

---

## 1. ¿Qué es Model Context Protocol (MCP)?

### Descripción
MCP es un protocolo estándar abierto desarrollado por Anthropic (anunciado en noviembre 2024) que permite conexiones seguras y bidireccionales entre sistemas de IA y fuentes de datos o herramientas.

### Características Técnicas
- **Protocolo base**: JSON-RPC 2.0
- **Inspirado en**: Language Server Protocol (LSP)
- **Transporte estándar**: stdio, HTTP con SSE
- **Estado**: Protocolo con estado (stateful connections)

### Componentes Principales
1. **Host**: Aplicación LLM que inicia la conexión (ej: Claude Desktop)
2. **Client**: Conector dentro del host
3. **Server**: Servicio que proporciona capacidades (nuestro servidor Unity)

### Capacidades que Expone un Servidor MCP
1. **Resources**: Datos contextuales para usuarios o modelos
2. **Prompts**: Mensajes y flujos de trabajo con plantillas
3. **Tools**: Funciones ejecutables para modelos de IA

### Adopción en 2025
- OpenAI adoptó MCP oficialmente en marzo 2025
- Integrado en ChatGPT Desktop, OpenAI Agents SDK, Responses API
- Adoptantes tempranos: Block, Apollo, Zed, Replit, Codeium, Sourcegraph
- Especificación activa con actualizaciones continuas (última versión: 2025-06-18)

---

## 2. SDK de C# para MCP

### Paquetes Disponibles
Microsoft y Anthropic mantienen oficialmente el SDK de C# con 3 paquetes NuGet:

1. **ModelContextProtocol** (Principal)
   - Hosting y dependency injection
   - Para mayoría de servidores console/hosted
   - Comando: `dotnet add package ModelContextProtocol --prerelease`

2. **ModelContextProtocol.AspNetCore**
   - Servidores HTTP con ASP.NET Core
   - Comunicación basada en HTTP

3. **ModelContextProtocol.Core**
   - Cliente ligero o APIs de bajo nivel
   - Mínimas dependencias

### Estado del SDK
- **Estado**: Preview (APIs pueden cambiar sin previo aviso)
- **Versión de protocolo**: 2025-06-18
- **Repositorio**: https://github.com/modelcontextprotocol/csharp-sdk

### Definición de Tools (Herramientas)

#### Patrón Básico
```csharp
[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"Hello from C#: {message}";
}
```

#### Características de Tools
- Atributos `[McpServerToolType]` en la clase
- Atributos `[McpServerTool]` en los métodos
- Soporte para métodos síncronos y asíncronos
- Inyección de dependencias automática
- Parámetros con `[Description]` para contexto AI
- Descubrimiento automático con `WithToolsFromAssembly()`

#### Ejemplo con Inyección de Dependencias
```csharp
[McpServerTool, Description("Get a monkey by name.")]
public static async Task<string> GetMonkey(
    MonkeyService monkeyService,
    [Description("The name of the monkey to get details for")] string name)
{
    var monkey = await monkeyService.GetMonkey(name);
    return JsonSerializer.Serialize(monkey);
}
```

---

## 3. Compatibilidad con Unity

### Versiones de .NET en Unity
- Unity usa **.NET Standard 2.0/2.1**
- Unity 2021+ soporta .NET Standard 2.1
- Mono runtime o IL2CPP para compilación

### Problemas de Compatibilidad Identificados

#### ❌ SDK MCP Oficial No Compatible Directamente
El SDK `ModelContextProtocol` depende de:
- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.DependencyInjection`
- Paquetes que pueden no funcionar en Unity runtime

#### ✅ Soluciones Disponibles
1. **Microsoft.Extensions.Hosting.Unity**
   - Proyecto: https://github.com/amelkor/Microsoft.Extensions.Hosting.Unity
   - Adapta .NET Generic Host para Unity3D
   - Permite usar Microsoft.Extensions.Hosting en Unity

2. **Unity.Microsoft.DependencyInjection**
   - Paquete NuGet compatible con .NET Standard 2.0
   - Permite usar dependency injection en Unity

---

## 4. Arquitecturas Posibles

### Opción A: Servidor MCP Externo (RECOMENDADO)

```
┌─────────────────────────────────────────┐
│   Claude Desktop / MCP Client          │
│   (Anthropic, OpenAI, etc.)            │
└────────────────┬────────────────────────┘
                 │ JSON-RPC over stdio
                 │
┌────────────────▼────────────────────────┐
│   MCP Server Process (.NET)            │
│   - SDK oficial ModelContextProtocol   │
│   - Implementa Tools:                  │
│     • CaptureScreenshot               │
│     • ClickAtPosition                 │
│     • SendKeyboardInput               │
│     • SendMouseInput                  │
│     • GetGameState                    │
└────────────────┬────────────────────────┘
                 │ JSON-RPC / IPC
                 │ Unity.RPC library
┌────────────────▼────────────────────────┐
│   Unity Editor (Runtime)               │
│   - Editor Script actúa como bridge    │
│   - Ejecuta acciones en Game View      │
│   - Captura screenshots                │
│   - Simula inputs                      │
└─────────────────────────────────────────┘
```

#### Ventajas
✅ Usa SDK oficial de MCP sin modificaciones
✅ Actualizaciones del protocolo fáciles de aplicar
✅ Separación clara de responsabilidades
✅ El proceso MCP corre fuera de Unity (más estable)
✅ Puede usar bibliotecas .NET modernas
✅ Unity.RPC oficial facilita comunicación

#### Desventajas
⚠️ Dos procesos separados (mayor complejidad)
⚠️ Necesita gestión de procesos
⚠️ Latencia adicional en comunicación IPC

### Opción B: Servidor MCP Nativo en Unity

```
┌─────────────────────────────────────────┐
│   Claude Desktop / MCP Client          │
└────────────────┬────────────────────────┘
                 │ JSON-RPC over stdio
                 │
┌────────────────▼────────────────────────┐
│   Unity Editor Process                 │
│                                        │
│   ┌──────────────────────────────────┐ │
│   │ MCP Server (implementación       │ │
│   │ custom en Unity)                 │ │
│   │ - JSON-RPC handling manual       │ │
│   │ - Tools implementadas en C#      │ │
│   └──────────────────────────────────┘ │
│                                        │
│   ┌──────────────────────────────────┐ │
│   │ Unity Game Logic                 │ │
│   │ - Screenshot capture             │ │
│   │ - Input simulation               │ │
│   └──────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

#### Ventajas
✅ Un solo proceso (más simple de deployar)
✅ Menor latencia (todo en memoria)
✅ Acceso directo a APIs de Unity

#### Desventajas
❌ No puede usar SDK oficial (incompatibilidad)
❌ Implementación manual de JSON-RPC 2.0
❌ Mantenimiento manual de cambios de protocolo
❌ Complejidad de stdio en Unity Editor
❌ Posibles problemas con threading de Unity

---

## 5. Arquitectura Recomendada: Opción A (Servidor Externo)

### Componentes

#### 5.1. MCP Server Process (.NET Console App)

**Ubicación**: `MCP.UnityServer/` (proyecto .NET separado)

**Responsabilidades**:
- Implementar protocolo MCP usando SDK oficial
- Exponer tools para testing de Unity
- Comunicarse con Unity Editor vía IPC

**Tecnologías**:
- .NET 8+ Console Application
- `ModelContextProtocol` NuGet package
- `Unity.RPC` para comunicación con Unity (o Named Pipes / gRPC)

**Tools a Implementar (Fase 1)**:
```csharp
[McpServerToolType]
public static class UnityTestingTools
{
    [McpServerTool, Description("Captures a screenshot of the Unity Game View")]
    public static async Task<string> CaptureScreenshot(
        [Description("Output file path for screenshot")] string? outputPath = null)
    {
        // Comunica con Unity Editor para capturar screenshot
        var result = await unityClient.CallMethod("CaptureScreenshot", outputPath);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Clicks at a specific screen position in Unity Game View")]
    public static async Task<string> ClickAtPosition(
        [Description("X coordinate in pixels")] int x,
        [Description("Y coordinate in pixels")] int y,
        [Description("Mouse button: left, right, middle")] string button = "left")
    {
        var result = await unityClient.CallMethod("SimulateClick", x, y, button);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Sends keyboard input to Unity")]
    public static async Task<string> SendKeyInput(
        [Description("Key code to send (Unity KeyCode format)")] string keyCode,
        [Description("Hold duration in seconds")] float duration = 0.1f)
    {
        var result = await unityClient.CallMethod("SimulateKeyPress", keyCode, duration);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Gets current game state information")]
    public static async Task<string> GetGameState()
    {
        var result = await unityClient.CallMethod("GetGameState");
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Navigates menu using keyboard arrows and enter")]
    public static async Task<string> NavigateMenu(
        [Description("Direction: up, down, left, right, enter, escape")] string direction)
    {
        var result = await unityClient.CallMethod("SimulateMenuNavigation", direction);
        return JsonSerializer.Serialize(result);
    }
}
```

#### 5.2. Unity Editor Bridge (Unity Package)

**Ubicación**: `Packages/com.yourcompany.mcp-testing/`

**Responsabilidades**:
- Recibir llamadas desde MCP Server Process
- Ejecutar acciones en Unity (screenshots, inputs)
- Reportar estado del juego
- Gestionar el proceso del servidor MCP

**Componentes**:

##### `MCPServerManager.cs` (Editor Script)
```csharp
using UnityEditor;
using UnityEngine;
using System.Diagnostics;

[InitializeOnLoad]
public class MCPServerManager
{
    private static Process mcpServerProcess;
    private static RpcClient rpcClient;

    static MCPServerManager()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem("Tools/MCP Testing/Start MCP Server")]
    public static void StartServer()
    {
        // Iniciar proceso del servidor MCP
        // Establecer comunicación RPC
    }

    [MenuItem("Tools/MCP Testing/Stop MCP Server")]
    public static void StopServer()
    {
        // Detener proceso del servidor MCP
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // Servidor puede operar durante Play Mode
        }
    }
}
```

##### `UnityActionExecutor.cs`
```csharp
using UnityEngine;
using UnityEngine.TestTools.Utils;

public class UnityActionExecutor : MonoBehaviour
{
    public void CaptureScreenshot(string outputPath)
    {
        // Implementación de captura de pantalla
        ScreenCapture.CaptureScreenshot(outputPath);
    }

    public void SimulateClick(int x, int y, string button)
    {
        // Simular click usando Input System o legacy Input
    }

    public void SimulateKeyPress(string keyCode, float duration)
    {
        // Simular presión de tecla
    }

    public string GetGameState()
    {
        // Recopilar información del estado actual
        return JsonUtility.ToJson(new GameStateInfo
        {
            currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            // Más información...
        });
    }
}
```

#### 5.3. Comunicación entre Procesos

**Opciones evaluadas**:

1. **Unity.RPC** (RECOMENDADO)
   - Biblioteca oficial de Unity Technologies
   - JSON-RPC + MessagePack
   - Diseñada para plugins de Editor
   - Repositorio: https://github.com/Unity-Technologies/com.unity.rpc

2. **Named Pipes**
   - Bajo nivel, mayor control
   - Requiere implementación manual

3. **gRPC**
   - Más robusto para producción
   - Overhead mayor para prototipo

---

## 6. Tools Requeridos para Fase 1

### 6.1. Gestión de Capturas de Pantalla

**Tool**: `CaptureScreenshot`
- **Input**: Ruta opcional de salida
- **Output**: Base64 de imagen o ruta del archivo
- **Implementación Unity**: `ScreenCapture.CaptureScreenshot()`

### 6.2. Interacción con UI

**Tool**: `ClickAtPosition`
- **Input**: Coordenadas X, Y, botón del mouse
- **Output**: Confirmación + elemento UI clickeado
- **Implementación**: Event System raycasting

**Tool**: `SendKeyInput`
- **Input**: KeyCode, duración
- **Output**: Confirmación
- **Implementación**: Input.simulateMouseInput o nuevo Input System

### 6.3. Navegación de Menús

**Tool**: `NavigateMenu`
- **Input**: Dirección (up/down/left/right/enter/escape)
- **Output**: Estado actual del menú
- **Implementación**: Simular inputs de navegación + detectar selección actual

### 6.4. Información de Estado

**Tool**: `GetGameState`
- **Output**: JSON con:
  - Escena actual
  - Canvas activos
  - Elementos UI visibles
  - Estado de navegación
  - Último elemento seleccionado

**Tool**: `GetUIHierarchy`
- **Output**: Árbol de UI con posiciones, nombres, estados

---

## 7. Flujo de Testing Automatizado (Fase 1)

### Caso de Uso: Test de Navegación de Menú Principal

```
1. MCP Client (Claude) recibe prompt:
   "Testea la navegación del menú principal usando teclado"

2. Claude llama a tools:
   - GetGameState() → detecta que está en MainMenu
   - CaptureScreenshot() → analiza visualmente el menú
   - GetUIHierarchy() → entiende estructura de botones

3. Claude ejecuta navegación:
   - SendKeyInput("DownArrow") → navega a siguiente opción
   - CaptureScreenshot() → verifica selección visual
   - SendKeyInput("Enter") → confirma selección
   - GetGameState() → verifica transición de escena

4. Claude repite para todas las opciones del menú

5. Claude genera informe:
   - ✅ Navegación por teclado funciona
   - ✅ Todas las opciones son accesibles
   - ❌ Botón "Settings" no responde a Enter
   - Capturas de pantalla adjuntas como evidencia
```

---

## 8. Plan de Implementación (Fase 1)

### Milestone 1: Proof of Concept (1-2 semanas)

**Objetivo**: Validar arquitectura con tool básico

**Tareas**:
1. Crear proyecto .NET para MCP Server
2. Configurar SDK de MCP con stdio transport
3. Implementar tool `Echo` de prueba
4. Crear Unity package vacío
5. Integrar `Unity.RPC` o Named Pipes
6. Implementar comunicación bidireccional básica
7. **Validación**: Claude puede llamar Echo y recibir respuesta desde Unity

### Milestone 2: Screenshot & State (1 semana)

**Tareas**:
1. Implementar `CaptureScreenshot` tool en MCP Server
2. Implementar captura en Unity Editor Script
3. Implementar `GetGameState` tool
4. Crear sistema de serialización de estado
5. **Validación**: Claude puede capturar pantallas y leer estado

### Milestone 3: Input Simulation (1-2 semanas)

**Tareas**:
1. Investigar Input System vs Legacy Input
2. Implementar `ClickAtPosition` tool
3. Implementar raycasting de UI
4. Implementar `SendKeyInput` tool
5. Implementar `NavigateMenu` helper
6. **Validación**: Claude puede navegar un menú simple

### Milestone 4: Testing Framework (1 semana)

**Tareas**:
1. Crear escena de prueba con menú multi-nivel
2. Implementar `GetUIHierarchy` tool
3. Crear prompts optimizados para testing
4. Documentar patrones de testing
5. **Validación**: Claude completa test de flujo de menú completo

### Milestone 5: Reporting (1 semana)

**Tareas**:
1. Implementar tool `GenerateTestReport`
2. Formato de informe con screenshots embebidos
3. Integración con Claude para auto-generar informes
4. **Validación**: Informe completo de test generado automáticamente

---

## 9. Desafíos Técnicos Anticipados

### 9.1. Threading en Unity
**Problema**: Unity APIs solo funcionan en el main thread
**Solución**: Queue de acciones para ejecutar en `EditorApplication.update`

### 9.2. Play Mode vs Edit Mode
**Problema**: Screenshots y algunas acciones requieren Play Mode
**Solución**:
- Detectar modo automáticamente
- Auto-entrar en Play Mode si es necesario
- Usar `[ExecuteInEditMode]` donde sea posible

### 9.3. Input System
**Problema**: Nuevo Input System requiere enfoque diferente
**Solución**:
- Soportar ambos (Legacy y nuevo)
- Detectar automáticamente qué sistema está activo
- Proveer abstracción

### 9.4. Timing & Sincronización
**Problema**: Acciones necesitan tiempo para completarse
**Solución**:
- Sistema de callbacks/awaits
- Tools retornan cuando acción está completa
- Timeout configurables

### 9.5. Visualización de Debugging
**Problema**: Difícil saber qué está haciendo el AI
**Solución**:
- Editor Window con log de acciones
- Visualización en Game View de clicks
- Timeline de acciones ejecutadas

---

## 10. Extensiones Futuras (Fase 2+)

### Streaming en Tiempo Real
- WebRTC o similar para video stream
- Tool `StartVideoStream` / `StopVideoStream`
- Modelos de visión en local procesando frames

### Testing de Gameplay
- Detección de colisiones
- Validación de física
- Performance metrics

### Testing Multiplataforma
- Build automation
- Testing en diferentes resoluciones
- Testing en dispositivos móviles (via Unity Remote)

### Integración CI/CD
- GitHub Actions ejecutando tests
- Reportes automáticos en PRs
- Test regression suite

---

## 11. Conclusión y Próximos Pasos

### ✅ VIABILIDAD CONFIRMADA

Es totalmente viable crear un servidor MCP para Unity con las capacidades solicitadas. La arquitectura de **servidor externo con comunicación IPC** es la más robusta y mantenible.

### Próximos Pasos Recomendados

1. **Crear estructura de proyecto**:
   ```
   mcp-unity-testing/
   ├── MCP.UnityServer/          # .NET Console App
   │   ├── MCP.UnityServer.csproj
   │   ├── Program.cs
   │   └── Tools/
   │       └── UnityTestingTools.cs
   ├── UnityProject/              # Proyecto Unity
   │   └── Packages/
   │       └── com.mcp.testing/   # Unity Package
   │           └── Editor/
   │               ├── MCPServerManager.cs
   │               └── UnityActionExecutor.cs
   └── docs/
       └── ARCHITECTURE.md
   ```

2. **Implementar Proof of Concept** (Milestone 1)

3. **Validar con caso de uso real** (menú simple)

4. **Iterar basándose en feedback**

### Estimación de Tiempo Total (Fase 1)
- **Optimista**: 4-5 semanas
- **Realista**: 6-8 semanas
- **Pesimista**: 10-12 semanas

### Riesgos Principales
- 🟡 Complejidad de Input Simulation (mitigable con investigación)
- 🟡 Problemas de threading (bien documentado, soluciones conocidas)
- 🟢 SDK de MCP es preview (bajo riesgo, comunidad activa)

---

## 12. Referencias

### Documentación Oficial
- MCP Specification: https://modelcontextprotocol.io/specification/2025-03-26
- MCP C# SDK: https://github.com/modelcontextprotocol/csharp-sdk
- Unity.RPC: https://github.com/Unity-Technologies/com.unity.rpc

### Artículos Técnicos
- Build MCP server in C#: https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp/
- MCP C# SDK Update 2025-06-18: https://devblogs.microsoft.com/dotnet/mcp-csharp-sdk-2025-06-18-update/

### Bibliotecas Auxiliares
- Microsoft.Extensions.Hosting.Unity: https://github.com/amelkor/Microsoft.Extensions.Hosting.Unity
- Unity.Microsoft.DependencyInjection: https://www.nuget.org/packages/Unity.Microsoft.DependencyInjection

---

**Documento creado**: 2025-11-04
**Autor**: Claude (Investigación automatizada)
**Versión**: 1.0
