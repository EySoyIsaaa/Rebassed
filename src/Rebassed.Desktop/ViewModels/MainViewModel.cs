using System.ComponentModel;
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

namespace Rebassed.Desktop.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly RebassPipeline pipeline;
    private string inputFile = string.Empty;
    private string outputFile = string.Empty;
    private string status = "Load an MP3 file to start.";
    private double peak;
    private bool clipping;

    public MainViewModel()
    {
        pipeline = new RebassPipeline(new NaudioMp3Codec(), new RebassProcessor(new SafetyProcessor()));

        Parameters = new RebassParameters();
        PresetNames = PresetLibrary.All.Keys.ToList();
        SelectedPreset = PresetNames.First();

        BrowseInputCommand = new RelayCommand(_ => BrowseInput());
        BrowseOutputCommand = new RelayCommand(_ => BrowseOutput());
        ProcessCommand = new RelayCommand(_ => ProcessAsync(), _ => !string.IsNullOrWhiteSpace(InputFile) && !string.IsNullOrWhiteSpace(OutputFile));
        ApplyPresetCommand = new RelayCommand(_ => ApplyPreset());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public RebassParameters Parameters { get; }
    public List<string> PresetNames { get; }

    public ICommand BrowseInputCommand { get; }
    public ICommand BrowseOutputCommand { get; }
    public ICommand ProcessCommand { get; }
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

    public string BoxWarning => "Warning: Below your ported-box tuning frequency, excursion risk increases significantly.";

    private void BrowseInput()
    {
        var dialog = new OpenFileDialog { Filter = "MP3 Files|*.mp3" };
        if (dialog.ShowDialog() == true)
        {
            InputFile = dialog.FileName;
            if (string.IsNullOrWhiteSpace(OutputFile))
            {
                OutputFile = Path.ChangeExtension(InputFile, ".rebassed.mp3");
            }
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
        OnPropertyChanged(nameof(Parameters));
    }

    private async Task ProcessAsync()
    {
        try
        {
            Status = "Processing...";
            var result = await pipeline.ProcessFileAsync(InputFile, OutputFile, Parameters);
            Peak = result.PeakLinear;
            Clipping = result.ClippingDetected;
            Status = result.ClippingDetected
                ? "Done with clipping warning. Lower Bass Level or Dry/Wet Mix."
                : "Done. Exported MP3 with generated sub-bass.";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
            MessageBox.Show(Status, "Rebassed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        OnPropertyChanged(name);
        (ProcessCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
