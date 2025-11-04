# MCP Unity Testing Server

**Model Context Protocol server for automated Unity game testing**

Author: Santiago Dopazo Hilario ([@SantiagoGameLover](https://github.com/SantiagoGameLover))
Company: Montseny XR

## Overview

Este proyecto implementa un servidor MCP (Model Context Protocol) que corre directamente en Unity Editor, permitiendo que modelos de IA como Claude o ChatGPT interactúen con tu juego de Unity para automatizar pruebas de interfaz de usuario, navegación de menús y flujos de juego.

## Estado del Proyecto

✅ **Fase 1 Completada** - Prototipo funcional con capacidades básicas de testing

### Funcionalidades Implementadas

- ✅ Servidor MCP integrado en Unity Editor
- ✅ Protocolo JSON-RPC 2.0 implementado nativamente
- ✅ Captura de pantallas del Game View
- ✅ Simulación de clicks del mouse en posiciones específicas
- ✅ Simulación de inputs de teclado (KeyCodes de Unity)
- ✅ **Simulación de inputs de mando/gamepad** (botones y sticks analógicos)
- ✅ Consultas de estado del juego (escena, UI activa, elementos seleccionados)
- ✅ **Detección y consulta de mandos conectados**
- ✅ Navegación automática de menús (teclado, mouse y gamepad)
- ✅ Gestión del ciclo de vida del servidor desde menús de Unity

### Próximos Pasos (Fase 2)

- 🔄 Streaming de video en tiempo real
- 🔄 Testing de gameplay (física, colisiones)
- 🔄 Métricas de rendimiento
- 🔄 Testing multiplataforma
- 🔄 Integración con CI/CD

## Estructura del Proyecto

```
mcp-unity-testing/
├── RESEARCH.md                    # Investigación detallada y diseño arquitectónico
├── UnityProject/                  # Proyecto Unity
│   └── Packages/
│       └── com.mcp.testing/       # Paquete Unity del servidor MCP
│           ├── package.json
│           ├── README.md          # Documentación completa de uso
│           └── Editor/
│               ├── MCPServerManager.cs      # Gestión del servidor
│               ├── MCPServer.cs             # Implementación del servidor MCP
│               ├── MCPProtocol.cs           # Clases del protocolo JSON-RPC
│               └── UnityActionExecutor.cs   # Ejecutor de acciones en Unity
└── MCP.UnityServer/               # (Referencia) Implementación alternativa con SDK oficial
    ├── MCP.UnityServer.csproj
    ├── Program.cs
    └── Tools/
        └── UnityTestingTools.cs
```

## Quick Start

### 1. Instalación

**Opción A: Clonar el repositorio**
```bash
git clone https://github.com/yourusername/mcp-unity-testing.git
cd mcp-unity-testing
```

**Opción B: Copiar el paquete a tu proyecto Unity**
```bash
cp -r UnityProject/Packages/com.mcp.testing /path/to/your/unity/project/Packages/
```

### 2. Iniciar el servidor en Unity

1. Abre tu proyecto Unity
2. Ve a **Tools > MCP Testing > Start Server**
3. Verifica en la consola: `[MCP] Server started successfully`

### 3. Conectar con Claude Desktop

Edita el archivo de configuración de Claude:

**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "unity-testing": {
      "command": "/path/to/unity/executable",
      "args": [
        "-projectPath", "/path/to/your/UnityProject",
        "-executeMethod", "MCP.UnityTesting.Editor.MCPServerManager.StartServer"
      ]
    }
  }
}
```

### 4. Probar con Claude

```
Prompt de ejemplo:

"Estoy probando un juego de Unity. Por favor:
1. Toma una captura de pantalla para ver el menú actual
2. Navega al menú de configuración usando el teclado
3. Haz click en el botón de 'Audio'
4. Genera un informe de la prueba"
```

## Herramientas Disponibles

| Herramienta | Descripción |
|-------------|-------------|
| `echo` | Verifica conectividad con el servidor |
| `capture_screenshot` | Captura pantalla del Game View |
| `click_at_position` | Simula click del mouse en coordenadas específicas |
| `send_key_input` | Envía input de teclado (KeyCodes de Unity) |
| `send_gamepad_button` | **Simula presionar botones del mando (A, B, X, Y, etc.)** |
| `send_gamepad_axis` | **Simula movimiento de sticks analógicos y triggers** |
| `get_game_state` | Obtiene estado actual del juego (escena, UI, etc.) |
| `get_gamepad_state` | **Detecta y obtiene info de mandos conectados** |
| **`get_scene_snapshot`** | **📊 Captura completa del estado de la escena (jerarquía, UI, componentes, performance, etc.)** |
| `navigate_menu` | Navega menús usando teclado (up/down/left/right/enter) |
| `navigate_menu_gamepad` | **Navega menús usando mando (stick + botones A/B)** |

Ver documentación completa en [`UnityProject/Packages/com.mcp.testing/README.md`](UnityProject/Packages/com.mcp.testing/README.md)

## Documentación

- **[RESEARCH.md](RESEARCH.md)** - Investigación completa sobre MCP, análisis de viabilidad, arquitectura y diseño técnico
- **[Package README](UnityProject/Packages/com.mcp.testing/README.md)** - Guía completa de uso, API de herramientas y ejemplos

## Arquitectura

El servidor MCP corre dentro del proceso de Unity Editor:

```
Claude Desktop (MCP Client)
    ↓ JSON-RPC via stdio
Unity Editor
    ├─ MCPServer (thread separado)
    │   ├─ Manejo de protocolo JSON-RPC 2.0
    │   └─ Exposición de tools
    └─ UnityActionExecutor (main thread)
        ├─ Captura de screenshots
        ├─ Simulación de inputs
        └─ Consultas de estado
```

Ver diseño detallado en [RESEARCH.md](RESEARCH.md)

## Casos de Uso

### Testing Automatizado de UI
- Navegación completa de menús
- Verificación de flujos de pantallas
- Testing de accesibilidad por teclado/mouse/gamepad

### Validación de Flujos
- Onboarding de usuario
- Tutorial flows
- Configuración y settings

### Regression Testing
- Verificar que UI sigue funcionando tras cambios
- Testing de builds automático
- Capturas de pantalla para comparación visual

## Tecnologías

- **Unity 2021.3+** (.NET Standard 2.1)
- **Model Context Protocol** (Anthropic)
- **JSON-RPC 2.0**
- **C# Editor Scripting**

## Contribuir

Las contribuciones son bienvenidas! Por favor:

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## Licencia

MIT License - ver archivo [LICENSE](LICENSE) para detalles

## Contacto

- **Autor**: Santiago Dopazo Hilario
- **GitHub**: [@SantiagoGameLover](https://github.com/SantiagoGameLover)
- **Compañía**: Montseny XR
- **Email**: contact@montsenyxr.com

## Referencias

- [Model Context Protocol](https://modelcontextprotocol.io/)
- [MCP Specification](https://modelcontextprotocol.io/specification/2025-03-26)
- [Claude Desktop](https://claude.ai/download)
- [Unity Editor Scripting](https://docs.unity3d.com/Manual/ExtendingTheEditor.html)
