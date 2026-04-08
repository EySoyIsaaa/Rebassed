# Rebassed (Windows Desktop)

Aplicación Windows para crear versiones **rebassed** de canciones MP3 con síntesis de subgrave (no solo boost).

## Arquitectura

```text
Rebassed.sln
├─ src/
│  ├─ Rebassed.Core/
│  │  ├─ AudioIO/         # Decode/encode MP3
│  │  ├─ DspEngine/       # Filtros, detección de graves, generación subarmónica, mezcla
│  │  ├─ Safety/          # Validación, HPF subsónico, limitador
│  │  ├─ Presets/         # Safe 30Hz, Balanced 33Hz, Deep 35Hz
│  │  ├─ Models/          # DTOs de audio y parámetros
│  │  └─ RebassPipeline.cs
│  └─ Rebassed.Desktop/
│     ├─ ViewModels/      # MainViewModel, comandos
│     ├─ Views/           # Convertidores UI
│     ├─ Services/        # Ruta de preview corta
│     └─ MainWindow.xaml  # Ventana principal y controles
├─ scripts/
│  └─ run_windows.bat     # Atajo para restore/build/run
└─ tests/
   └─ Rebassed.Core.Tests/
```

## Instalación y ejecución en tu PC (Windows)

### Opción A (recomendada): usar Visual Studio

1. Instala **Visual Studio 2022** con la carga de trabajo:
   - `Desarrollo de escritorio con .NET`.
2. Asegúrate de tener **.NET 8 SDK** instalado.
3. Descarga/clona este repo.
4. Abre `Rebassed.sln` en Visual Studio.
5. Selecciona el proyecto de inicio: `Rebassed.Desktop`.
6. Pulsa `F5` (Debug) o `Ctrl+F5` (sin depuración).


### Si Visual Studio muestra "biblioteca de clases no se puede iniciar"

Ese mensaje aparece cuando el proyecto de inicio quedó en `Rebassed.Core` (librería).

Solución:
1. En el explorador de soluciones, clic derecho en `Rebassed.Desktop`.
2. Selecciona **Establecer como proyecto de inicio**.
3. Ejecuta con `F5`.

### Opción B: por terminal (PowerShell/CMD)

1. Instala .NET 8 SDK.
2. En la carpeta del repo, ejecuta:

```bash
dotnet restore Rebassed.sln
dotnet build Rebassed.sln -c Release
dotnet run --project src/Rebassed.Desktop/Rebassed.Desktop.csproj -c Release
```

### Opción C: script listo

En CMD:

```bat
scripts\run_windows.bat
```

## ¿Cómo usar la app? (paso a paso)

1. Clic en **Open MP3** y selecciona tu canción.
2. Clic en **Export MP3** y elige nombre de salida.
3. Elige preset:
   - `Safe 30Hz` (más conservador),
   - `Balanced 33Hz` (recomendado),
   - `Deep 35Hz` (más agresivo).
4. Ajusta controles si quieres:
   - `Target Frequency (Hz)` (30–35 típico; 25–45 avanzado),
   - `Sweep` (zona de detección de graves),
   - `Wide`,
   - `Bass Level`,
   - `Subsonic HPF`,
   - `Dry/Wet Mix`,
   - `Output Ceiling`.
5. Pulsa **Process**.
6. Revisa:
   - medidor `Peak`,
   - alerta de clipping.
7. Si aparece clipping:
   - baja `Bass Level`,
   - baja `Dry/Wet Mix`,
   - o reduce `Output Ceiling` (más negativo).

⚠️ Seguridad: por debajo de la frecuencia de sintonía de una caja portada aumenta el riesgo de excursión del subwoofer.

## Flujo DSP implementado

1. MP3 -> PCM flotante (`NaudioMp3Codec.DecodeMp3Async`).
2. Extracción de graves por band-pass (biquad).
3. Generación de subgrave (3 métodos):
   - seguimiento por período (`PeriodTracker`),
   - rectificación / no linealidad (`RectifiedNld`),
   - seno guiado por envolvente (`EnvelopeSine`).
4. Limpieza con low-pass + compuerta por umbral.
5. Ganancia de subgrave (`Bass Level`).
6. Mezcla con original (`Dry/Wet Mix`).
7. Protección: `Subsonic HPF` + limitador suave con `Output Ceiling`.
8. Exportación MP3.

## Casos de prueba básicos

`tests/Rebassed.Core.Tests/RebassProcessorTests.cs` cubre:
- senoide (60 Hz) con aumento de energía subgrave,
- validación de parámetros peligrosos,
- señal tipo barrido con salida estable y sin clipping en preset base.

Prueba manual recomendada con canción real:
1. Cargar MP3 comercial 44.1/48 kHz.
2. Aplicar preset `Balanced 33Hz`.
3. Exportar y comparar A/B con original en auriculares + sistema con subwoofer.
