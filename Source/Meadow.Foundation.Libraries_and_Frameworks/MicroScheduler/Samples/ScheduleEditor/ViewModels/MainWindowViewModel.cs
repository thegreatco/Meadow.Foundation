using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ScheduleEditor.Models;
using Meadow.Foundation.Scheduling;

namespace ScheduleEditor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ScheduleCollectionModel _scheduleCollection;
    private ScheduleModel? _selectedSchedule;
    private ScheduleEventModel? _selectedEvent;
    private bool _isFileModified;
    private bool _hasUnsavedChanges;

    public MainWindowViewModel()
    {
        _scheduleCollection = new ScheduleCollectionModel();
        
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
            var json = ScheduleSerializer.SerializeScheduleCollection(ScheduleCollection.ScheduleCollection);
            if (json != null)
            {
                await File.WriteAllTextAsync(ScheduleCollection.FileName, json);
                IsFileModified = false;
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
            SuggestedFileName = "schedule.json",
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
                var json = ScheduleSerializer.SerializeScheduleCollection(ScheduleCollection.ScheduleCollection);
                if (json != null)
                {
                    await File.WriteAllTextAsync(filePath, json);
                    ScheduleCollection.FileName = filePath;
                    IsFileModified = false;
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
        // Mark all events as clean
        if (SelectedSchedule != null)
        {
            foreach (var eventModel in SelectedSchedule.Events)
            {
                eventModel.MarkClean();
            }
        }
        
        HasUnsavedChanges = false;
        IsFileModified = true;
        this.RaisePropertyChanged(nameof(WindowTitle));
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