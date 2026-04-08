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
└─ tests/
   └─ Rebassed.Core.Tests/
```

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

## Controles UI

- `Target Frequency (Hz)` (25-45 avanzado, objetivo típico 30-35).
- `Sweep` (banda de detección low/high).
- `Wide`.
- `Bass Level`.
- `Subsonic HPF`.
- `Dry/Wet Mix`.
- `Output Ceiling`.

También incluye:
- medidor `Peak` y alerta de clipping,
- advertencia de excursión por debajo de sintonía de caja portada,
- presets seguros por defecto.

## Build / ejecución (Windows)

Requisitos:
- .NET 8 SDK
- Windows 10/11

Comandos:

```bash
dotnet restore Rebassed.sln
dotnet build Rebassed.sln -c Release
dotnet run --project src/Rebassed.Desktop/Rebassed.Desktop.csproj -c Release
```

## Casos de prueba básicos

`tests/Rebassed.Core.Tests/RebassProcessorTests.cs` cubre:
- senoide (60 Hz) con aumento de energía subgrave,
- validación de parámetros peligrosos,
- señal tipo barrido con salida estable y sin clipping en preset base.

Prueba manual recomendada con canción real:
1. Cargar MP3 comercial 44.1/48 kHz.
2. Aplicar preset `Balanced 33Hz`.
3. Exportar y comparar A/B con original en auriculares + sistema con subwoofer.
