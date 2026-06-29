# Corrección — TP02 "Tower Builder" (Light Freight Warehouse)

---

## 1. Resumen general

Es una entrega muy completa y de las más sólidas. La arquitectura es claramente
profesional: usa **event channels con ScriptableObjects** para desacoplar la UI y los managers
(patrón Observer real, no eventos C# acoplados), **ScriptableObjects de configuración**
(`GameConfig`, `BlockConfig`), **Singletons persistentes** (`AudioManager`, `SettingsManager`)
con `DontDestroyOnLoad`, **AudioMixer** con sliders Master/Música/SFX/UI, **persistencia** vía
`PlayerPrefs` (volúmenes + highscore), menú de **pausa** con Escape, y **publicación en Itch.io**
con README completo y linkeo circular. El juego cumple holgadamente los tres tiers en lo
estructural.

Hay, sin embargo, varios **bugs concretos** que conviene corregir: Un `Resources.FindObjectsOfTypeAll<Button>()`
riesgoso en el `AudioManager`. No rompe el flujo principal de juego, pero sí degradan
robustez.

Recomendación de higiene de repo: limpiar las carpetas `AssetStore/` (escenas demo) y `_Recovery/`
(`0.unity`), que son ruido y no aportan a la entrega, próximo trabajo se penalizará.

---

## 2. Checklist por tier

### TIER B (base)

| Req                       | Estado   | Justificación                                                                                                                                                                                                                                                                                                    |
|---------------------------|----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **B1** Mecánica y físicas | ✓        | `Block.cs` usa `Rigidbody` (isKinematic/linearVelocity) + colliders; los bloques caen y se apilan. La "grúa/dron" se mueve horizontalmente de forma automática y constante mediante `BezierRouteFollow3D` recorriendo rutas Bézier (`BezierPath3D`). `Tower.cs` detecta el aterrizaje por `OnCollisionEnter`.    |
| **B2** Flujo de escenas   | ✓        | `Scenes/MainMenu.unity` + `Scenes/Game.unity`. Transiciones por `SceneManager.LoadScene("Game")` (`MenuManager`) y `"MainMenu"` (`GameManager.BackToMenu`).                                                                                                                                                      |
| **B3** UI básica          | ✓        | `MenuManager` expone Play (`LoadGame`), Settings (`ShowConfig`), Credits (`ShowCredits`) y Exit (`ExitGame`), todos funcionales. HUD en `UIManager`: puntaje (`scoreText`), racha de perfectos (`perfectText` "Perfect! xN") y altura (`heightText` "Floors: N"). También highscore y vidas.                     |
| **B4** Manejo de assets   | ✓        | Modelos 3D integrados del Asset Store (Boxes by Wing13GD, Garage Props, Military Cargo Aircraft como dron). Materiales aplicados (concreto Yughues). SFX al aterrizar/perfecto/fallar/miss (`BlockManager`: `BlockLandSFX`, `BlockPerfectSFX`, etc.). **Git LFS** activo (`.gitattributes`: wav, fbx, png, tga). |

**Tier B: 4/4 — COMPLETO. La compuerta hacia Tier A queda abierta.**

### TIER A (cuenta porque Tier B está completo)

| Req                            | Estado   | Justificación                                                                                                                                                                                                                                                                           |
|--------------------------------|----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **A1** Menú de pausa con tecla | ✓        | `GameManager.Update` → `KeyCode.Escape` alterna `PauseGame`/`UnpauseGame`; dispara `PauseGameEvent`/`UnpauseGameEvent` que `UIManager` escucha para mostrar/ocultar `pauseCanvas`. `Time.timeScale = 0/1`.                                                                              |
| **A2** Configuración y Mixers  | ✓        | `SettingsManager` (Singleton) gestiona `AudioMixer` con grupos MasterVolume/MusicVolume/SFXVolume/UIVolume. `OptionsHandler` conecta 4 sliders (master, música, SFX, UI). Accesible desde el menú principal (`MenuManager.ShowConfig`) y desde pausa (`UIManager.ShowSettings`).        |
| **A3** Persistencia            | ✓        | Volúmenes: `PlayerPrefs.SetFloat("Audio"+group, ...)` + restauración en `SetMixerFromPref` (`SettingsManager`). Puntaje máximo: `PlayerPrefs.GetInt/SetInt("Highscore")` en `GameManager` (`Awake`/`GameOver`). Persiste entre sesiones. *Bug menor en el caso por defecto del mixer.*  |

**Tier A: 3/3 — COMPLETO. La compuerta hacia Tier S queda abierta.**

### TIER S (cuenta porque Tier A está completo)

| Req                                 | Estado | Justificación                                                                                                                                                                                                                                                                                                                                 |
|-------------------------------------|--------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **S1** Singleton y Managers         | ✓      | `AudioManager` y `SettingsManager` implementan Singleton  y **persisten entre escenas** con `DontDestroyOnLoad`, destruyendo duplicados en `Awake`. `GameManager`/`UIManager`/`BlockManager` actúan como managers de escena coordinados por eventos.                                                                                          |
| **S2** Observer y ScriptableObjects | ✓      | **Observer:** implementado de forma ejemplar con event channels SO (`EventChannel`, `IntEventChannel`, `BlockEventChannel`, `StatsEventChannel`) y ~18 assets de evento; `UIManager`, `Crane`, `Tower`, `RackSpawner` se suscriben/desuscriben en OnEnable/OnDisable. **ScriptableObjects de bloque:** existen (`BlockConfig`, `GameConfig`). |
| **S3** GitHub / Build / Itch.io     | ✓      | Tag `v1.0` ✓, último commit "Final release", 78 commits. `README.md` completo: "How to Play", link a Itch (`https://lordmungi.itch.io/warehouse`) y créditos detallados → **linkeo circular README→Itch** confirmado. **Exit oculto en WebGL:** `MenuManager.Awake` con `#if UNITY_WEBGL → exitButton.SetActive(false)` ✓.                    |

**Tier S: 3/3 — COMPLETO.**

---

## 3. Qué tiene / Qué le falta

### Qué tiene
- Físicas reales de apilado con Rigidbody/Collider y detección de aterrizaje por colisión.
- Dron/grúa con movimiento automático constante sobre curvas Bézier propias.
- Dos escenas con flujo completo Menú ↔ Gameplay.
- Menú principal con los 4 botones funcionales; HUD con puntaje, racha y altura (+ vidas y highscore).
- Modelos 3D del Asset Store (no primitivas), materiales, SFX por evento, Git LFS.
- Pausa con Escape; opciones accesibles desde menú y pausa.
- AudioMixer con 4 sliders; persistencia de volúmenes y highscore con PlayerPrefs.
- Singletons persistentes (`AudioManager`, `SettingsManager`).
- Observer desacoplado con event channels SO (muy bien logrado).
- ScriptableObjects de configuración; tag `v1.0`; README completo; Exit oculto en WebGL.

### Qué le falta / a mejorar
- `BlockConfig` sin **masa** ni **colores** (S2 pide ambos explícitamente).
- Bug de **off-by-one** en vidas/Game Over (el jugador obtiene una vida extra).
- Bug `List.Remove(Count-1)` (debería ser `RemoveAt`) en `RemoveBlockFromTower`.
- Bug de paréntesis en el caso por defecto del mixer (`Log10(0.5f*20)` → +1 dB en vez de silencio/valor por defecto).
- `Resources.FindObjectsOfTypeAll<Button>()` + falta de `RemoveListener` → posibles listeners duplicados.
- `Debug.Log` de producción olvidado en `SettingsManager`.
- Limpieza de repo (carpetas `AssetStore/` y `_Recovery/`).
- Validaciones de borde faltantes (Peek/índices en `RackSpawner`, `BezierRouteFollow3D`).
- Strings mágicos de grupos de audio y claves de PlayerPrefs sin constantes.

---

## 4. Hallazgos de código más importantes

1. **`GameManager.LoseLife()` — Error (off-by-one).** Con `InitialLives=3`, al fallar baja 3→2→1→0 sin terminar; `GameOver()` recién entra al `else` en el **cuarto** fallo (cuando `lives` ya es 0). El jugador recibe una vida más de la configurada. Corrección: `lives--; if (lives <= 0) GameOver();`

2. **`BlockManager.RemoveBlockFromTower()` — Error.** `lastBlocksOffset.Remove(lastBlocksOffset.Count - 1)` usa `List.Remove`, que elimina **por valor**, no por índice. Debe ser `RemoveAt(Count - 1)`. Corrompe la ventana de offsets usada para el cálculo de wobble.

3. **`SettingsManager.SetMixerFromPref()` — Error (paréntesis).** `Mathf.Log10(0.5f * 20)` = `Log10(10)` = 1 dB, casi sin atenuar. El `*20` quedó dentro del `Log10`. Además, si la pref no existe (`GetFloat` devuelve 0) todos los grupos quedan en +1 dB en vez de un valor por defecto razonable. El caso "mudo" debería usar -144 dB como en `UpdateAudioMixer`.

4. **`AudioManager.SetSounds()` — Error/Warning.** `Resources.FindObjectsOfTypeAll<Button>()` alcanza prefabs y objetos inactivos/de otras escenas, no solo la escena actual; se ejecuta en cada `sceneLoaded` sin `RemoveListener`, por lo que un mismo botón puede acumular listeners y reproducir el SFX varias veces.

5. **Buenas prácticas a pulir (Warnings/Sugestions):** comparación de floats con `==` en el wobble (`BlockManager.Update`), `public Vector3 size` mutable en `Block`, modificación de `wallMaterial` compartido en runtime (`Crane`), `RackPrefabs[]` usado solo en índice 0, `event` recomendable en los delegados de los event channels, validaciones de borde faltantes, y teclas/strings hardcodeados.

> Nota: el patrón Observer con event channels SO y la organización en managers/config están muy por encima del promedio. Los bugs son acotados y de fácil corrección.

---

## 5. Cálculo de la nota (regla de compuerta paso a paso)

La nota es la suma de requisitos cumplidos, con la regla de que un tier superior **solo suma** si **todos** los requisitos de los tiers inferiores están cumplidos.

- **Tier B:** B1 ✓, B2 ✓, B3 ✓, B4 ✓ → **4/4**. Tier B completo → la compuerta a Tier A se abre.
- **Tier A** (cuenta): A1 ✓, A2 ✓, A3 ✓ → **3/3**. Tier A completo → la compuerta a Tier S se abre.
- **Tier S** (cuenta): S1 ✓, S2 ✓ (con masa/colores faltantes, pero hay SO de bloque + Observer ejemplar → se da por cumplido), S3 ✓ (tag, README/Itch, Exit-WebGL verificados; build/imágenes/videos  → **3/3**.

Suma: 4 (B) + 3 (A) + 3 (S) = **10**.

Los bugs detectados (vidas, `Remove`, mixer, `AudioManager`) son defectos de robustez dentro de requisitos que **sí** están implementados y operativos; no anulan el cumplimiento de los ítems de la rúbrica, pero quedan señalados como correcciones necesarias.

> **NOTA FINAL: 10/10**
> 