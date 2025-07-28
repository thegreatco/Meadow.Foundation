using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Meadow.CLI;
using Meadow.CLI.Commands.DeviceManagement;
using Meadow.Foundation.Scheduling;
using ReactiveUI;
using ScheduleEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScheduleEditor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private const string DefaultFileName = "schedules.json";

    private ScheduleCollectionModel _scheduleCollection;
    private ScheduleModel? _selectedSchedule;
    private ScheduleEventModel? _selectedEvent;
    private bool _isFileModified;
    private bool _hasUnsavedChanges;
    private string? _selectedSerialPort;
    private readonly MeadowConnectionManager? _connectionManager;

    public event Action<List<SerialPortModel>, string?>? SerialPortsUpdated;

    public MainWindowViewModel()
    {
        _scheduleCollection = new ScheduleCollectionModel();
        SerialPorts = new ObservableCollection<SerialPortModel>();

        try
        {
            _connectionManager = new MeadowConnectionManager(new SettingsManager());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not initialize Meadow connection manager: {ex.Message}");
        }

        // Commands
        NewFileCommand = ReactiveCommand.CreateFromTask(NewFile);
        OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFile);
        SaveFileCommand = ReactiveCommand.CreateFromTask(SaveFile);
        SaveAsFileCommand = ReactiveCommand.CreateFromTask(SaveAsFile);
        AddScheduleCommand = ReactiveCommand.CreateFromTask(AddSchedule);
        RemoveScheduleCommand = ReactiveCommand.Create(RemoveSchedule, this.WhenAnyValue(x => x.SelectedSchedule).Select(schedule => schedule != null));
        AddEventCommand = ReactiveCommand.CreateFromTask(AddEvent, this.WhenAnyValue(x => x.SelectedSchedule).Select(schedule => schedule != null));
        RemoveEventCommand = ReactiveCommand.Create(RemoveEvent, this.WhenAnyValue(x => x.SelectedEvent).Select(evt => evt != null));
        EditEventCommand = ReactiveCommand.CreateFromTask(EditEvent, this.WhenAnyValue(x => x.SelectedEvent).Select(evt => evt != null));
        SaveChangesCommand = ReactiveCommand.CreateFromTask(SaveChanges, this.WhenAnyValue(x => x.HasUnsavedChanges));
        RefreshSerialPortsCommand = ReactiveCommand.CreateFromTask(RefreshSerialPorts);
        SelectSerialPortCommand = ReactiveCommand.Create<string>(SelectSerialPort);
        LoadFromDeviceCommand = ReactiveCommand.CreateFromTask(LoadFromDevice, this.WhenAnyValue(x => x.SelectedSerialPort).Select(port => !string.IsNullOrEmpty(port)));
        SaveToDeviceCommand = ReactiveCommand.CreateFromTask(SaveToDevice, this.WhenAnyValue(x => x.SelectedSerialPort).Select(port => !string.IsNullOrEmpty(port)));

        // Initialize serial ports asynchronously
        _ = RefreshSerialPorts();

        // Subscribe to property changes to track modifications
        _scheduleCollection.PropertyChanged += OnScheduleCollectionPropertyChanged;
    }

    public ScheduleCollectionModel ScheduleCollection
    {
        get => _scheduleCollection;
        set
        {
            if (_scheduleCollection != null)
            {
                _scheduleCollection.PropertyChanged -= OnScheduleCollectionPropertyChanged;
            }

            this.RaiseAndSetIfChanged(ref _scheduleCollection, value);

            if (_scheduleCollection != null)
            {
                _scheduleCollection.PropertyChanged += OnScheduleCollectionPropertyChanged;
            }
        }
    }

    public ScheduleModel? SelectedSchedule
    {
        get => _selectedSchedule;
        set
        {
            if (_selectedSchedule != null)
            {
                _selectedSchedule.PropertyChanged -= OnSelectedSchedulePropertyChanged;
            }

            this.RaiseAndSetIfChanged(ref _selectedSchedule, value);

            if (_selectedSchedule != null)
            {
                _selectedSchedule.PropertyChanged += OnSelectedSchedulePropertyChanged;
            }
        }
    }

    public ScheduleEventModel? SelectedEvent
    {
        get => _selectedEvent;
        set => this.RaiseAndSetIfChanged(ref _selectedEvent, value);
    }

    public bool IsFileModified
    {
        get => _isFileModified;
        set => this.RaiseAndSetIfChanged(ref _isFileModified, value);
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set => this.RaiseAndSetIfChanged(ref _hasUnsavedChanges, value);
    }

    public string WindowTitle =>
        $"Schedule Editor - {(string.IsNullOrEmpty(ScheduleCollection.FileName) ? "Untitled" : Path.GetFileName(ScheduleCollection.FileName))}" +
        (IsFileModified ? "*" : "");

    public ObservableCollection<SerialPortModel> SerialPorts { get; }

    public string? SelectedSerialPort
    {
        get => _selectedSerialPort;
        set => this.RaiseAndSetIfChanged(ref _selectedSerialPort, value);
    }

    // Commands
    public ICommand NewFileCommand { get; }
    public ICommand OpenFileCommand { get; }
    public ICommand SaveFileCommand { get; }
    public ICommand SaveAsFileCommand { get; }
    public ICommand AddScheduleCommand { get; }
    public ICommand RemoveScheduleCommand { get; }
    public ICommand AddEventCommand { get; }
    public ICommand RemoveEventCommand { get; }
    public ICommand EditEventCommand { get; }
    public ICommand SaveChangesCommand { get; }
    public ICommand RefreshSerialPortsCommand { get; }
    public ICommand SelectSerialPortCommand { get; }
    public ICommand LoadFromDeviceCommand { get; }
    public ICommand SaveToDeviceCommand { get; }

    private async Task NewFile()
    {
        if (IsFileModified)
        {
            // TODO: Show confirmation dialog
        }

        ScheduleCollection = new ScheduleCollectionModel();
        IsFileModified = false;
        this.RaisePropertyChanged(nameof(WindowTitle));
    }

    private async Task OpenFile()
    {
        var topLevel = TopLevel.GetTopLevel(App.MainWindow);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Schedule File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
            }
        });

        if (files.Count > 0)
        {
            try
            {
                var filePath = files[0].Path.LocalPath;
                var fileInfo = new FileInfo(filePath);
                var collection = Meadow.Foundation.Scheduling.ScheduleCollection.LoadFrom(fileInfo);

                ScheduleCollection = new ScheduleCollectionModel(collection)
                {
                    FileName = filePath
                };
                IsFileModified = false;
                this.RaisePropertyChanged(nameof(WindowTitle));
            }
            catch (Exception ex)
            {
                // TODO: Show error dialog
                Console.WriteLine($"Error opening file: {ex.Message}");
            }
        }
    }

    private async Task SaveFile()
    {
        if (string.IsNullOrEmpty(ScheduleCollection.FileName))
        {
            await SaveAsFile();
            return;
        }

        try
        {
            // Apply any pending changes before saving
            ApplyAllPendingChanges();

            var json = ScheduleSerializer.SerializeScheduleCollection(ScheduleCollection.ScheduleCollection);
            if (json != null)
            {
                await File.WriteAllTextAsync(ScheduleCollection.FileName, json);
                IsFileModified = false;
                HasUnsavedChanges = false;
                this.RaisePropertyChanged(nameof(WindowTitle));
            }
        }
        catch (Exception ex)
        {
            // TODO: Show error dialog
            Console.WriteLine($"Error saving file: {ex.Message}");
        }
    }

    private async Task SaveAsFile()
    {
        var topLevel = TopLevel.GetTopLevel(App.MainWindow);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Schedule File",
            DefaultExtension = "json",
            SuggestedFileName = DefaultFileName,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
            }
        });

        if (file != null)
        {
            try
            {
                var filePath = file.Path.LocalPath;

                // Apply any pending changes before saving
                ApplyAllPendingChanges();

                var json = ScheduleSerializer.SerializeScheduleCollection(ScheduleCollection.ScheduleCollection);
                if (json != null)
                {
                    await File.WriteAllTextAsync(filePath, json);
                    ScheduleCollection.FileName = filePath;
                    IsFileModified = false;
                    HasUnsavedChanges = false;
                    this.RaisePropertyChanged(nameof(WindowTitle));
                }
            }
            catch (Exception ex)
            {
                // TODO: Show error dialog
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }
    }

    private async Task AddSchedule()
    {
        // TODO: Show dialog to get schedule name
        var name = $"Schedule {ScheduleCollection.Schedules.Count + 1}";
        ScheduleCollection.AddSchedule(name);
        IsFileModified = true;
        this.RaisePropertyChanged(nameof(WindowTitle));
    }

    private void RemoveSchedule()
    {
        if (SelectedSchedule != null)
        {
            ScheduleCollection.RemoveSchedule(SelectedSchedule);
            SelectedSchedule = null;
            IsFileModified = true;
            this.RaisePropertyChanged(nameof(WindowTitle));
        }
    }

    private async Task AddEvent()
    {
        if (SelectedSchedule == null) return;

        // TODO: Show dialog to select event type and configure it
        // For now, create a simple daily event
        var dailyEvent = new DailyScheduleEvent(
            DateTime.UtcNow.Date.AddHours(12), // Noon UTC
            "true"
        );

        SelectedSchedule.AddEvent(dailyEvent);
        IsFileModified = true;
        this.RaisePropertyChanged(nameof(WindowTitle));
    }

    private void RemoveEvent()
    {
        if (SelectedSchedule != null && SelectedEvent != null)
        {
            SelectedSchedule.RemoveEvent(SelectedEvent);
            SelectedEvent = null;
            IsFileModified = true;
            this.RaisePropertyChanged(nameof(WindowTitle));
        }
    }

    private async Task EditEvent()
    {
        if (SelectedEvent == null) return;

        // TODO: Show event edit dialog
        Console.WriteLine($"Edit event: {SelectedEvent.EventType}");
    }

    private async Task SaveChanges()
    {
        // Apply all pending changes to the underlying schedule objects
        ApplyAllPendingChanges();

        HasUnsavedChanges = false;
        IsFileModified = true;
        this.RaisePropertyChanged(nameof(WindowTitle));
    }

    private void ApplyAllPendingChanges()
    {
        // Apply changes to all schedules, not just the selected one
        foreach (var schedule in ScheduleCollection.Schedules)
        {
            schedule.ApplyChanges();
        }
    }

    private async Task RefreshSerialPorts()
    {
        try
        {
            // Use Meadow connection manager to get Meadow-specific ports
            var meadowPorts = await MeadowConnectionManager.GetSerialPorts();
            var currentSelection = SelectedSerialPort;

            SerialPorts.Clear();

            foreach (var portName in meadowPorts.OrderBy(p => p))
            {
                var portModel = new SerialPortModel(portName);
                var isSelected = portName == currentSelection;
                if (isSelected)
                {
                    portModel.IsSelected = true;
                }
                SerialPorts.Add(portModel);
            }

            // If the previously selected port is no longer available, clear the selection
            if (!string.IsNullOrEmpty(currentSelection) && !meadowPorts.Contains(currentSelection))
            {
                SelectedSerialPort = null;
            }

            // Notify the UI to update the menu
            SerialPortsUpdated?.Invoke(SerialPorts.ToList(), SelectedSerialPort);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing serial ports: {ex.Message}");

            // Notify the UI with empty list on error
            SerialPortsUpdated?.Invoke(new List<SerialPortModel>(), SelectedSerialPort);
        }
    }

    private void SelectSerialPort(string portName)
    {
        // Clear all selections
        foreach (var port in SerialPorts)
        {
            port.IsSelected = false;
        }

        // Select the clicked port
        var selectedPort = SerialPorts.FirstOrDefault(p => p.PortName == portName);
        if (selectedPort != null)
        {
            selectedPort.IsSelected = true;
            SelectedSerialPort = portName;
        }

        // Notify the UI to update the menu
        SerialPortsUpdated?.Invoke(SerialPorts.ToList(), SelectedSerialPort);
    }

    private async Task LoadFromDevice()
    {
        if (string.IsNullOrEmpty(SelectedSerialPort) || _connectionManager == null)
            return;

        try
        {
            // Show confirmation dialog if there are unsaved changes
            if (IsFileModified)
            {
                // TODO: Show confirmation dialog
                Console.WriteLine("Warning: Unsaved changes will be lost");
            }

            Console.WriteLine($"Loading {DefaultFileName} from device on {SelectedSerialPort}...");

            var connection = _connectionManager.GetConnectionForRoute(SelectedSerialPort);
            if (connection == null)
            {
                Console.WriteLine("Unable to create connection to device");
                return;
            }

            // Attach to the device
            var device = await connection.Attach();
            if (device == null)
            {
                Console.WriteLine("Unable to attach to device");
                return;
            }

            // Read the schedule file from the device
            var jsonContent = await connection.ReadFileString(DefaultFileName);

            if (string.IsNullOrEmpty(jsonContent))
            {
                Console.WriteLine($"No {DefaultFileName} file found on device or file is empty");
                return;
            }

            // Deserialize the schedule from JSON
            var collection = ScheduleSerializer.DeserializeScheduleCollection(jsonContent);

            ScheduleCollection = new ScheduleCollectionModel(collection)
            {
                FileName = $"{DefaultFileName} (from device)"
            };

            IsFileModified = false;
            this.RaisePropertyChanged(nameof(WindowTitle));

            Console.WriteLine("Schedule loaded successfully from device");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading from device: {ex.Message}");
        }
    }

    private async Task SaveToDevice()
    {
        if (string.IsNullOrEmpty(SelectedSerialPort) || _connectionManager == null)
            return;

        try
        {
            Console.WriteLine($"Saving {DefaultFileName} to device on {SelectedSerialPort}...");

            // Apply any pending changes before saving
            ApplyAllPendingChanges();

            var connection = _connectionManager.GetConnectionForRoute(SelectedSerialPort);
            if (connection == null)
            {
                Console.WriteLine("Unable to create connection to device");
                return;
            }

            // Attach to the device
            var device = await connection.Attach();
            if (device == null)
            {
                Console.WriteLine("Unable to attach to device");
                return;
            }

            // Serialize the schedule to JSON
            var json = ScheduleSerializer.SerializeScheduleCollection(ScheduleCollection.ScheduleCollection);
            if (json == null)
            {
                Console.WriteLine("Failed to serialize schedule");
                return;
            }

            // Save the JSON to a temporary local file first
            var tempPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempPath, json);

            try
            {
                // Write the schedule file to the device
                var result = await connection.WriteFile(tempPath, DefaultFileName);

                if (result)
                {
                    Console.WriteLine("Schedule saved successfully to device");
                }
                else
                {
                    Console.WriteLine("Failed to save schedule to device");
                }
            }
            finally
            {
                // Clean up the temporary file
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to device: {ex.Message}");
        }
    }

    private void OnScheduleCollectionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        IsFileModified = true;
        this.RaisePropertyChanged(nameof(WindowTitle));
    }

    private void OnSelectedSchedulePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ScheduleModel.Events))
        {
            HasUnsavedChanges = true;
        }
    }
}