using Meadow.Foundation.Scheduling;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ScheduleEditor.Models;

/// <summary>
/// UI model for a schedule collection with timezone-aware display properties.
/// </summary>
public class ScheduleCollectionModel : INotifyPropertyChanged
{
    private ScheduleCollection _scheduleCollection;
    private string _fileName = string.Empty;

    public ScheduleCollectionModel()
    {
        _scheduleCollection = new ScheduleCollection();
        Schedules = new ObservableCollection<ScheduleModel>();
        RefreshSchedules();
    }

    public ScheduleCollectionModel(ScheduleCollection collection)
    {
        _scheduleCollection = collection;
        Schedules = new ObservableCollection<ScheduleModel>();
        RefreshSchedules();
    }

    public ScheduleCollection ScheduleCollection
    {
        get => _scheduleCollection;
        set
        {
            _scheduleCollection = value;
            RefreshSchedules();
            OnPropertyChanged(nameof(ScheduleCollection));
            OnPropertyChanged(nameof(TimezoneName));
            OnPropertyChanged(nameof(UtcOffsetHours));
            OnPropertyChanged(nameof(HasDaylightSavingTime));
        }
    }

    public string FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
            OnPropertyChanged(nameof(FileName));
        }
    }

    public ObservableCollection<ScheduleModel> Schedules { get; }

    public string TimezoneName
    {
        get => _scheduleCollection.Timezone.TimezoneName;
        set
        {
            _scheduleCollection.Timezone.TimezoneName = value;
            OnPropertyChanged(nameof(TimezoneName));
        }
    }

    public double UtcOffsetHours
    {
        get => _scheduleCollection.Timezone.UtcOffsetHours;
        set
        {
            _scheduleCollection.Timezone.UtcOffsetHours = value;
            OnPropertyChanged(nameof(UtcOffsetHours));
            RefreshAllEventDisplays(); // Refresh to update local time displays
        }
    }

    public bool HasDaylightSavingTime
    {
        get => _scheduleCollection.Timezone.DaylightSavingTime != null;
        set
        {
            if (value && _scheduleCollection.Timezone.DaylightSavingTime == null)
            {
                _scheduleCollection.Timezone.DaylightSavingTime = new DaylightSavingTimeInfo();
            }
            else if (!value)
            {
                _scheduleCollection.Timezone.DaylightSavingTime = null;
            }
            OnPropertyChanged(nameof(HasDaylightSavingTime));
            RefreshAllEventDisplays(); // Refresh to update local time displays
        }
    }

    public DaylightSavingTimeInfo? DaylightSavingTime => _scheduleCollection.Timezone.DaylightSavingTime;

    private void RefreshSchedules()
    {
        Schedules.Clear();
        foreach (var schedule in _scheduleCollection)
        {
            var scheduleModel = new ScheduleModel(schedule, _scheduleCollection.Timezone);
            Schedules.Add(scheduleModel);
        }
    }

    private void RefreshAllEventDisplays()
    {
        foreach (var scheduleModel in Schedules)
        {
            scheduleModel.RefreshEvents(_scheduleCollection.Timezone);
        }
    }

    public void AddSchedule(string name)
    {
        var schedule = new Schedule { Name = name };
        _scheduleCollection.Add(schedule);
        Schedules.Add(new ScheduleModel(schedule, _scheduleCollection.Timezone));
    }

    public void RemoveSchedule(ScheduleModel scheduleModel)
    {
        _scheduleCollection.Remove(scheduleModel.Schedule);
        Schedules.Remove(scheduleModel);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
