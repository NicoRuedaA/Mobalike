# Guía de Despliegue - MobaGameplay

Esta guía cubre la configuración para desplegar el proyecto en diferentes entornos.

> **Versión del proyecto:** Unity 6 (6000.3.11f1) con URP 17.3.0 | 62 tests unitarios pasando ✅

---

## 1. Configuración de Build

### 1.1 Configuración General

```
Edit → Project Settings → Player

Configuración para PC:
- Product Name: MobaGameplay
- Company Name: [TuEmpresa]
- Version: 1.0.0
- Default Cursor: [Tu cursor personalizado]
- Cursor Hotspot: (0, 0)

Configuración de API:
- API Compatibility Level: .NET Standard 2.1
- Suppress Deployment Checks: ✓ (para desarrollo)
```

### 1.2 Configuración de Build

```
Edit → Project Settings → Build Settings

Plataforma: Windows (para desarrollo)
- Architecture: x86_64
- Build Target: Standalone

Alternativas:
- macOS: x86_64 + ARM64 (Apple Silicon)
- Linux: x86_64
```

### 1.3 Configuración de Quality

```
Edit → Project Settings → Quality

Niveles sugeridos:
- Ultra: Para PCs de gama alta
- High: Default, equilibrado
- Medium: Laptops
- Low: Equipos antiguos

Apply Settings: All Platforms
```

---

## 2. Capas y Física

### 2.1 Configuración de Capas

```
Edit → Project Settings → Tags and Layers

Capas requeridas (crear si no existen):
┌─────────────────┬───────┬─────────────────────────────────────────┐
│ Nombre          │ #     │ Uso                                    │
├─────────────────┼───────┼─────────────────────────────────────────┤
│ Default         │ 0     │ Objetos sin clasificación              │
│ TransparentFX   │ 1     │ Efectos transparentes                │
│ Ignore Raycast  │ 2     │ Ignorar raycasts                     │
│ Ground          │ 3     │ Terreno, pisos                        │
│ Water           │ 4     │ Agua, superficies líquidas            │
│ UI              │ 5     │ Elementos de interfaz                  │
│ Player          │ 8     │ Personaje jugador                     │
│ Enemy           │ 9     │ Enemigos, NPCs hostiles               │
│ Projectile      │ 10    │ Proyectiles (jugador y enemigo)       │
│ Ability         │ 11    │ Áreas de efecto de habilidades        │
└─────────────────┴───────┴─────────────────────────────────────────┘
```

### 2.2 Matriz de Colisiones

```
Edit → Project Settings → Physics → Layer Collision Matrix

Configurar según sea necesario:

✓ Player ↔ Ground
✓ Player ↔ Enemy
✓ Player ↔ Projectile (Enemy)
✓ Enemy ↔ Ground
✓ Enemy ↔ Projectile (Player)
✓ Projectile ↔ Ground

✗ Player ↔ Player
✗ Enemy ↔ Enemy
✗ UI ↔ Everything (except triggers específicos)
```

---

## 3. Rendering y Gráficos

### 3.1 Configuración de URP

```
Edit → Project Settings → Graphics → Tier Settings

HDR: Enabled (para efectos de bloom)
LDR: Disabled (para móviles considerar)

URP Asset Configuration:
- Antialiasing: 4x MSAA
- Shadow Quality: High/Medium/Low según build
- Texture Quality: Full Res / Half Res
- VSync: Don't Sync (para competitivo) o Every V Blank
```

### 3.2 Shaders Requeridos

确保 los siguientes shaders existen en el proyecto:

```text
Assets/
├── Shaders/
│   ├── Custom/
│   │   └── Outline.shader    # Para HoverOutline
│   └── ...
```

Si el shader Outline no existe, crearlo:

```hlsl
Shader "Custom/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _Outline ("Outline Width", Float) = 0.02
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float _Outline;
            float4 _OutlineColor;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                v.vertex.xyz += v.normal * _Outline;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}
```

---

## 4. Configuración de Audio (Opcional)

```
Edit → Project Settings → Audio

- Default Speaker Mode: Stereo
- Sample Rate: 48000 Hz
- DSP Buffer Size: Best Performance
```

---

## 5. Red y Multiplayer (Futuro)

### 5.1 Preparación para Networking

Si planeas añadir multiplayer:

```
Paquetes sugeridos:
- com.unity.netcode.gameobjects@1.5.0 (NGO)
- com.unity.services.relay@1.0.5 (Unity Relay)
- com.unity.services.authentication@2.4.0 (Auth)
```

### 5.2 Consideraciones de Input

```
El sistema de input actual es LOCAL (single player).

Para multiplayer:
1. Input debe enviarse al servidor
2. Server authoritative movement
3. Client-side prediction
4. Server reconciliation
5. Interpolación de otros jugadores
```

---

## 6. Optimización de Build

### 6.1 Ajustes de Build

```csharp
// En Player Settings → Other Settings
- Managed Stripping Level: Medium
  (Low para desarrollo, High para producción)
  
- Optimize Mesh Data: ✓
  
- Il2CPP para Release builds
- Mono para desarrollo (compilación más rápida)
```

### 6.2 Settings for Size

```
- Compression Method: LZ4HC (buen balance)
- Build Compression: Enabled
- Use Deterministic Builds: ✓ (para caching)

Exclude from build (si no se usa):
- Windows Speech Recognition
```

### 6.3 Player Log Location (Debugging)

```
Windows: %LOCALAPPDATA%\CompanyName\ProductName\Player.log
macOS: ~/Library/Logs/CompanyName/ProductName/Player.log
Linux: ~/.config/unity3d/CompanyName/ProductName/Player.log
```

---

## 7. Configuración de Producción vs Desarrollo

### 7.1 Comparación

| Setting | Development | Production |
|---------|-------------|------------|
| Scripting Backend | Mono | IL2CPP |
| Managed Stripping | Low | High |
| Optimize Mesh Data | Off | On |
| Compression | None | LZ4HC |
| VSync | Off | On |
| Debug Logs | Enabled | Disabled |
| Crash Handler | On | On |

### 7.2 Compilación Condicional

```csharp
// En código: Deshabilitar logs en producción
void DebugLog(string message)
{
    #if UNITY_EDITOR
    Debug.Log(message);
    #else
    // En producción, solo logs de advertencia o error
    // Debug.LogWarning(message);
    #endif
}
```

### 7.3 Symbols para Build

```
Development Build:
- Define: DEVELOPMENT_BUILD

Release Build:
- Define: PRODUCTION

Testing:
- Define: QA_TESTING
```

---

## 8. Checklist Pre-Despliegue

### 8.1 Código

- [ ] No hay `Debug.Log` en código de producción (o están condicionalizados)
- [ ] No hay hardcoded URLs (usar ScriptableObject de configuración)
- [ ] No hay API keys en código (usar PlayerSettings)
- [ ] El código compila sin errores ni warnings
- [ ] Tests manuales completados

### 8.2 Escenas y Assets

- [ ] Todas las escenas guardadas
- [ ] No hay missing references en prefabs
- [ ] Texturas comprimidas apropiadamente
- [ ] Modelos optimizados (LODs si es necesario)
- [ ] Shaders funcionando en standalone build

### 8.3 Configuración

- [ ] Layers configurados correctamente
- [ ] Quality settings apropiados
- [ ] Capas de física configuradas
- [ ] Input System settings correctos
- [ ] Product/Company name correctos

### 8.4 Build

- [ ] Build succeeds
- [ ] .exe generado y ejecutable
- [ ] No crashes al iniciar
- [ ] Performance aceptable (30+ FPS mínimo)
- [ ] Logs limpios (no errores en Player.log)

### 8.5 Testing en Build

- [ ] Movimiento funciona
- [ ] Charged attack funciona
- [ ] Habilidades funcionan
- [ ] Dash funciona
- [ ] Hover outline funciona
- [ ] Muerte del personaje funciona
- [ ] Enemigos funcionan (siapplicable)

---

## 9. Post-Deploy

### 9.1 Monitoreo

考虑 implementar:
- Unity Analytics (gratuito, básico)
- Custom crash reporting
- Performance monitoring

### 9.2 Actualizaciones

```
Para actualizaciones:
1. Incrementar versión en PlayerSettings
2. Build nuevo
3. Distribution via:
   - Steam
   - Epic Games Store
   - Direct download
   - Unity Asset Store (si aplica)
```

---

## 10. Troubleshooting de Build

### Problema: "Build failed - missing script"

```
Solución:
1. Verificar que todos los scripts están en Scripts folder
2. Check if any scripts have compilation errors
3. Re-import all assets (Assets → Reimport All)
```

### Problema: "Shader not found in build"

```
Solución:
1. Shaders must be in Resources folder OR
2. Add to "Always Included Shaders" list in Graphics Settings
3. Check shader compilation in Edit → Graphics
```

### Problema: "Input System not working in build"

```
Solución:
1. Verify "Active Input Handling" is set correctly
2. Check InputSystem package is included in build
3. Some inputs may need remapping for standalone
```

### Problema: "Performance very low in build"

```
Solución:
1. Lower quality settings
2. Check for unnecessary real-time shadows
3. Profile with Unity Profiler
4. Check for unnecessary allocations in Update loops
```
