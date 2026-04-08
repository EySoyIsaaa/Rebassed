using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Rebassed.Core;
using Rebassed.Core.AudioIO;
using Rebassed.Core.DspEngine;
using Rebassed.Core.Models;
using Rebassed.Core.Presets;
using Rebassed.Core.Safety;
using Rebassed.Desktop.Services;

namespace Rebassed.Desktop.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly RebassPipeline pipeline;
    private readonly PreviewService previewService;

    private string inputFile = string.Empty;
    private string outputFile = string.Empty;
    private string status = "Load an MP3 file to start.";
    private double peak;
    private bool clipping;
    private bool busy;

    public MainViewModel()
    {
        pipeline = new RebassPipeline(new NaudioMp3Codec(), new RebassProcessor(new SafetyProcessor()));
        previewService = new PreviewService();

        Parameters = new RebassParameters();
        Parameters.PropertyChanged += (_, _) => (ProcessCommand as RelayCommand)?.RaiseCanExecuteChanged();

        PresetNames = PresetLibrary.All.Keys.ToList();
        GenerationMethods = Enum.GetValues<SubGenerationMethod>().ToList();
        SelectedPreset = PresetNames.First();

        BrowseInputCommand = new RelayCommand(_ => BrowseInput(), _ => !Busy);
        BrowseOutputCommand = new RelayCommand(_ => BrowseOutput(), _ => !Busy);
        ProcessCommand = new RelayCommand(_ => ProcessAsync(), _ => CanProcess());
        PreviewCommand = new RelayCommand(_ => PreviewAsync(), _ => !Busy && !string.IsNullOrWhiteSpace(InputFile));
        ApplyPresetCommand = new RelayCommand(_ => ApplyPreset(), _ => !Busy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public RebassParameters Parameters { get; }
    public List<string> PresetNames { get; }
    public List<SubGenerationMethod> GenerationMethods { get; }

    public ICommand BrowseInputCommand { get; }
    public ICommand BrowseOutputCommand { get; }
    public ICommand ProcessCommand { get; }
    public ICommand PreviewCommand { get; }
    public ICommand ApplyPresetCommand { get; }

    public string SelectedPreset { get; set; }

    public string InputFile
    {
        get => inputFile;
        set => Set(ref inputFile, value);
    }

    public string OutputFile
    {
        get => outputFile;
        set => Set(ref outputFile, value);
    }

    public string Status
    {
        get => status;
        set => Set(ref status, value);
    }

    public double Peak
    {
        get => peak;
        set => Set(ref peak, value);
    }

    public bool Clipping
    {
        get => clipping;
        set => Set(ref clipping, value);
    }

    public bool Busy
    {
        get => busy;
        set => Set(ref busy, value);
    }

    public string BoxWarning => "Warning: Below your ported-box tuning frequency, excursion risk increases significantly.";

    private bool CanProcess() => !Busy && !string.IsNullOrWhiteSpace(InputFile) && !string.IsNullOrWhiteSpace(OutputFile);

    private void BrowseInput()
    {
        var dialog = new OpenFileDialog { Filter = "MP3 Files|*.mp3" };
        if (dialog.ShowDialog() == true)
        {
            InputFile = dialog.FileName;
            if (string.IsNullOrWhiteSpace(OutputFile))
                OutputFile = Path.ChangeExtension(InputFile, ".rebassed.mp3");
        }
    }

    private void BrowseOutput()
    {
        var dialog = new SaveFileDialog { Filter = "MP3 Files|*.mp3" };
        if (dialog.ShowDialog() == true)
            OutputFile = dialog.FileName;
    }

    private void ApplyPreset()
    {
        var source = PresetLibrary.All[SelectedPreset];
        Parameters.TargetFrequencyHz = source.TargetFrequencyHz;
        Parameters.SweepLowHz = source.SweepLowHz;
        Parameters.SweepHighHz = source.SweepHighHz;
        Parameters.Wide = source.Wide;
        Parameters.BassLevelDb = source.BassLevelDb;
        Parameters.SubsonicHpfHz = source.SubsonicHpfHz;
        Parameters.DryWetMix = source.DryWetMix;
        Parameters.OutputCeilingDb = source.OutputCeilingDb;
        Parameters.GenerationMethod = source.GenerationMethod;
    }

    private async Task PreviewAsync()
    {
        await RunBusyOperation(async () =>
        {
            Status = "Rendering 10s preview...";
            var preview = await previewService.ProcessPreviewAsync(InputFile, Parameters, 10);
            Peak = preview.PeakLinear;
            Clipping = preview.ClippingDetected;
            Status = preview.ClippingDetected
                ? "Preview done with clipping warning. Reduce Bass Level or Dry/Wet Mix."
                : "Preview done (10s analyzed).";
        });
    }

    private async Task ProcessAsync()
    {
        await RunBusyOperation(async () =>
        {
            Status = "Processing complete file...";
            var result = await pipeline.ProcessFileAsync(InputFile, OutputFile, Parameters);
            Peak = result.PeakLinear;
            Clipping = result.ClippingDetected;
            Status = result.ClippingDetected
                ? "Done with clipping warning. Lower Bass Level or Dry/Wet Mix."
                : "Done. Exported MP3 with generated sub-bass.";
        });
    }

    private async Task RunBusyOperation(Func<Task> action)
    {
        try
        {
            Busy = true;
            await action();
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
            MessageBox.Show(Status, "Rebassed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Busy = false;
        }
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        OnPropertyChanged(name);
        (ProcessCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (PreviewCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (BrowseInputCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (BrowseOutputCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ApplyPresetCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
