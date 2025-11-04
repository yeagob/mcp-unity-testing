# Installation Guide

Guía detallada de instalación del MCP Unity Testing Server.

## Requisitos

- **Unity 2021.3 o superior**
- **Claude Desktop** (u otro cliente MCP compatible)
- **Sistema Operativo**: macOS, Windows, o Linux

## Paso 1: Clonar o Descargar el Proyecto

### Opción A: Clonar con Git

```bash
git clone https://github.com/SantiagoGameLover/mcp-unity-testing.git
cd mcp-unity-testing
```

### Opción B: Descargar ZIP

1. Descarga el ZIP del repositorio
2. Extrae a una ubicación de tu elección

## Paso 2: Instalar el Paquete Unity

### Método 1: Copiar el Paquete Directamente

Copia la carpeta del paquete a tu proyecto Unity:

```bash
cp -r UnityProject/Packages/com.mcp.testing /path/to/your/unity/project/Packages/
```

### Método 2: Usar Unity Package Manager con Git

1. Abre tu proyecto Unity
2. Ve a **Window > Package Manager**
3. Haz clic en el botón **+** y selecciona **Add package from git URL**
4. Ingresa: `https://github.com/SantiagoGameLover/mcp-unity-testing.git?path=/UnityProject/Packages/com.mcp.testing`

### Método 3: Instalar desde Disco Local

1. Abre tu proyecto Unity
2. Ve a **Window > Package Manager**
3. Haz clic en **+** y selecciona **Add package from disk**
4. Navega a `mcp-unity-testing/UnityProject/Packages/com.mcp.testing/package.json`
5. Selecciona el archivo

## Paso 3: Verificar la Instalación

1. En Unity Editor, ve al menú **Tools**
2. Deberías ver un nuevo submenú: **MCP Testing**
3. Verifica que las opciones estén disponibles:
   - Start Server
   - Stop Server
   - Toggle Auto-Start
   - Server Status

## Paso 4: Configurar Claude Desktop

### macOS

1. Localiza el archivo de configuración:
   ```bash
   open ~/Library/Application\ Support/Claude/
   ```

2. Edita o crea `claude_desktop_config.json`:
   ```json
   {
     "mcpServers": {
       "unity-testing": {
         "command": "/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity",
         "args": [
           "-batchmode",
           "-nographics",
           "-projectPath",
           "/Users/TUUSUARIO/Projects/TuProyectoUnity",
           "-executeMethod",
           "MCP.UnityTesting.Editor.MCPServerManager.StartServer",
           "-logFile",
           "-"
         ]
       }
     }
   }
   ```

3. **IMPORTANTE**: Reemplaza las rutas:
   - `/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity` - Ruta a tu ejecutable de Unity
   - `/Users/TUUSUARIO/Projects/TuProyectoUnity` - Ruta a tu proyecto Unity

### Windows

1. Localiza el archivo de configuración:
   ```
   %APPDATA%\Claude\claude_desktop_config.json
   ```

2. Edita o crea el archivo:
   ```json
   {
     "mcpServers": {
       "unity-testing": {
         "command": "C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.0f1\\Editor\\Unity.exe",
         "args": [
           "-batchmode",
           "-nographics",
           "-projectPath",
           "C:\\Users\\TUUSUARIO\\Projects\\TuProyectoUnity",
           "-executeMethod",
           "MCP.UnityTesting.Editor.MCPServerManager.StartServer",
           "-logFile",
           "-"
         ]
       }
     }
   }
   ```

3. **IMPORTANTE**: Usa dobles barras invertidas (`\\`) en las rutas de Windows

### Linux

1. Localiza el archivo de configuración:
   ```bash
   ~/.config/Claude/claude_desktop_config.json
   ```

2. Edita o crea el archivo (similar a macOS pero con rutas de Linux)

## Paso 5: Primera Prueba

### Prueba Manual (Recomendado primero)

1. Abre tu proyecto Unity
2. Ve a **Tools > MCP Testing > Start Server**
3. Observa la consola de Unity:
   ```
   [MCP] MCPServerManager initialized
   [MCP] Starting MCP Server...
   [MCP] Server started successfully
   [MCP] Listening on stdin/stdout for MCP protocol messages
   ```

4. Ve a **Tools > MCP Testing > Server Status**
5. Deberías ver: "Server Status: Running"

6. Detén el servidor: **Tools > MCP Testing > Stop Server**

### Prueba con Claude Desktop

1. **Reinicia Claude Desktop** (importante para que cargue la nueva configuración)

2. Abre Claude Desktop

3. Inicia una conversación y escribe:
   ```
   Por favor, usa la herramienta 'echo' del servidor unity-testing
   para enviar el mensaje "Hello Unity MCP!"
   ```

4. Si todo está configurado correctamente, Claude debería:
   - Iniciar Unity en modo batch
   - Conectarse al servidor MCP
   - Ejecutar la herramienta echo
   - Devolver la respuesta

## Solución de Problemas

### Error: "Server is already running"

**Causa**: Ya hay una instancia del servidor corriendo.

**Solución**:
```
Tools > MCP Testing > Stop Server
```
Luego intenta iniciar nuevamente.

### Error: "Unity executable not found"

**Causa**: La ruta al ejecutable de Unity en `claude_desktop_config.json` es incorrecta.

**Solución**:
1. Encuentra la ubicación de Unity:
   - **macOS**: Abre Hub de Unity > Installs > Haz clic derecho > Show in Finder
   - **Windows**: Abre Hub de Unity > Installs > Botón de opciones > Show in Explorer

2. La ruta completa debe incluir el ejecutable:
   - **macOS**: `/Applications/Unity/Hub/Editor/[VERSION]/Unity.app/Contents/MacOS/Unity`
   - **Windows**: `C:\Program Files\Unity\Hub\Editor\[VERSION]\Editor\Unity.exe`

### Error: "Project path not found"

**Causa**: La ruta del proyecto en `claude_desktop_config.json` es incorrecta.

**Solución**:
1. Verifica la ruta completa de tu proyecto Unity
2. Usa rutas absolutas, no relativas
3. En Windows, usa `\\` en lugar de `\`

### Claude Desktop no ve el servidor

**Causa**: No reiniciaste Claude Desktop después de editar la configuración.

**Solución**:
1. Cierra completamente Claude Desktop (no solo la ventana)
2. En macOS: `Cmd+Q`
3. En Windows: Cierra desde el System Tray
4. Vuelve a abrir Claude Desktop

### El servidor se inicia pero las herramientas no funcionan

**Causa**: El proyecto Unity no está en Play Mode.

**Solución**:
- Muchas herramientas requieren que Unity esté en Play Mode
- Presiona Play en Unity Editor antes de usar las herramientas
- O agrega una herramienta para entrar automáticamente en Play Mode

### Logs de Unity no aparecen

**Solución**:
- Verifica que el servidor esté corriendo: **Tools > MCP Testing > Server Status**
- Revisa la consola de Unity (debe tener el prefijo `[MCP]`)
- Habilita "Collapse" en la consola para ver todos los mensajes

## Configuración Avanzada

### Auto-Start del Servidor

Para que el servidor se inicie automáticamente al abrir Unity:

1. **Tools > MCP Testing > Toggle Auto-Start** (debe aparecer con ✓)
2. Reinicia Unity
3. Verifica en la consola que el servidor se inició

### Configuración para Desarrollo

Si estás desarrollando y quieres mantener Unity Editor abierto:

1. Inicia Unity manualmente con tu proyecto
2. Inicia el servidor: **Tools > MCP Testing > Start Server**
3. Usa una configuración alternativa en Claude que conecte al proceso ya corriendo

(Esta es una configuración avanzada que requiere un bridge de red)

### Timeout Personalizado

Si tus operaciones tardan más de 5 segundos:

1. Edita `UnityActionExecutor.cs`
2. Busca `WaitForMainThreadAction`
3. Aumenta el valor de timeout (en milisegundos)

```csharp
WaitForMainThreadAction(ref completed, 10000); // 10 segundos
```

## Verificación Final

Checklist de verificación:

- [ ] Unity 2021.3+ instalado
- [ ] Paquete `com.mcp.testing` en tu proyecto Unity
- [ ] Menú **Tools > MCP Testing** visible en Unity
- [ ] Servidor puede iniciarse sin errores
- [ ] Claude Desktop instalado
- [ ] `claude_desktop_config.json` configurado con rutas correctas
- [ ] Claude Desktop reiniciado
- [ ] Herramienta `echo` funciona desde Claude

## Siguiente Paso

Una vez que la instalación esté completa y verificada:

1. Lee el [README del paquete](UnityProject/Packages/com.mcp.testing/README.md) para aprender todas las herramientas disponibles
2. Prueba los ejemplos de uso
3. Consulta [RESEARCH.md](RESEARCH.md) para entender la arquitectura

## Soporte

Si tienes problemas:

1. Revisa la sección "Troubleshooting" arriba
2. Busca en los Issues del repositorio
3. Crea un nuevo Issue con:
   - Tu sistema operativo
   - Versión de Unity
   - Logs completos de la consola de Unity
   - Contenido de `claude_desktop_config.json` (sin rutas privadas)

## Contacto

- **GitHub**: [@SantiagoGameLover](https://github.com/SantiagoGameLover)
- **Email**: contact@montsenyxr.com
