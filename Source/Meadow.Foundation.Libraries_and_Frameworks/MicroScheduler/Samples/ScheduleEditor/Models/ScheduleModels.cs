using Meadow.Foundation.Scheduling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

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
        foreach (var schedule in _scheduleCollection.Schedules)
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
        _scheduleCollection.Schedules.Add(schedule);
        Schedules.Add(new ScheduleModel(schedule, _scheduleCollection.Timezone));
    }

    public void RemoveSchedule(ScheduleModel scheduleModel)
    {
        _scheduleCollection.Schedules.Remove(scheduleModel.Schedule);
        Schedules.Remove(scheduleModel);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// UI model for a single schedule with timezone-aware event display.
/// </summary>
public class ScheduleModel : INotifyPropertyChanged
{
    private readonly Schedule _schedule;
    private readonly TimezoneInfo _timezone;

    public ScheduleModel(Schedule schedule, TimezoneInfo timezone)
    {
        _schedule = schedule;
        _timezone = timezone;
        Events = new ObservableCollection<ScheduleEventModel>();
        RefreshEvents();
    }

    public Schedule Schedule => _schedule;

    public string Name
    {
        get => _schedule.Name;
        set
        {
            _schedule.Name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public ObservableCollection<ScheduleEventModel> Events { get; }

    private void RefreshEvents()
    {
        Events.Clear();
        foreach (var evt in _schedule.Events)
        {
            var eventModel = new ScheduleEventModel(evt, _timezone);
            eventModel.Changed += OnEventChanged;
            Events.Add(eventModel);
        }
    }

    private void OnEventChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Events));
    }

    public void RefreshEvents(TimezoneInfo timezone)
    {
        Events.Clear();
        foreach (var evt in _schedule.Events)
        {
            var eventModel = new ScheduleEventModel(evt, timezone);
            eventModel.Changed += OnEventChanged;
            Events.Add(eventModel);
        }
    }

    public void AddEvent(IScheduleEvent evt)
    {
        _schedule.Events.Add(evt);
        var eventModel = new ScheduleEventModel(evt, _timezone);
        eventModel.Changed += OnEventChanged;
        Events.Add(eventModel);
    }

    public void RemoveEvent(ScheduleEventModel eventModel)
    {
        _schedule.Events.Remove(eventModel.ScheduleEvent);
        Events.Remove(eventModel);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// UI model for a schedule event with timezone-aware time display.
/// </summary>
public class ScheduleEventModel : INotifyPropertyChanged
{
    private readonly IScheduleEvent _scheduleEvent;
    private readonly TimezoneInfo _timezone;
    private bool _isDirty;

    public ScheduleEventModel(IScheduleEvent scheduleEvent, TimezoneInfo timezone)
    {
        _scheduleEvent = scheduleEvent;
        _timezone = timezone;
    }

    public event EventHandler? Changed;

    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            _isDirty = value;
            OnPropertyChanged(nameof(IsDirty));
            if (value) Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void MarkClean()
    {
        _isDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    public IScheduleEvent ScheduleEvent => _scheduleEvent;

    public List<string> AvailableEventTypes => new()
    {
        "Daily",
        "Weekday", 
        "SunriseOffset",
        "SunsetOffset"
    };

    public string EventType 
    { 
        get => _scheduleEvent.EventType.ToString();
        set
        {
            if (_scheduleEvent.EventType.ToString() != value)
            {
                Console.WriteLine($"EventType changed from {_scheduleEvent.EventType} to {value}");
                // Note: Changing event type would require recreating the event object
                // For now, we'll just track the change but not implement the conversion
                IsDirty = true;
                OnPropertyChanged(nameof(EventType));
                OnPropertyChanged(nameof(TimeDisplay)); // Time display might change based on event type
            }
        }
    }

    public bool IsDisabled
    {
        get => _scheduleEvent.IsDisabled;
        set
        {
            _scheduleEvent.IsDisabled = value;
            IsDirty = true;
            OnPropertyChanged(nameof(IsDisabled));
        }
    }

    public string? Data
    {
        get => _scheduleEvent.Data;
        set
        {
            _scheduleEvent.Data = value;
            IsDirty = true;
            OnPropertyChanged(nameof(Data));
            OnPropertyChanged(nameof(ActionText));
        }
    }

    /// <summary>
    /// Gets or sets the action text for display/editing (Turn On/Turn Off)
    /// </summary>
    public string ActionText
    {
        get => Data?.ToLower() switch
        {
            "true" => "Turn On",
            "false" => "Turn Off", 
            _ => Data ?? ""
        };
        set
        {
            Console.WriteLine($"ActionText setter called with value: {value}");
            var newData = value switch
            {
                "Turn On" => "true",
                "Turn Off" => "false",
                _ => value
            };
            if (Data != newData)
            {
                Console.WriteLine($"ActionText changing Data from {Data} to {newData}");
                Data = newData; // This will trigger IsDirty = true
            }
        }
    }

    public List<string> AvailableActions => new() { "Turn On", "Turn Off" };

    /// <summary>
    /// Gets the time display string for this event. Shows both UTC and local time where applicable.
    /// </summary>
    public string TimeDisplay
    {
        get
        {
            return _scheduleEvent switch
            {
                DailyScheduleEvent daily => $"Daily at {FormatTime(daily.EventTime)} ({FormatLocalTime(daily.EventTime)})",
                WeekdayScheduleEvent weekday => $"{string.Join(", ", weekday.DaysOfWeek?.Select(d => d.ToString().Substring(0, 3)) ?? Array.Empty<string>())} at {FormatTime(weekday.EventTime)} ({FormatLocalTime(weekday.EventTime)})",
                SunriseOffsetScheduleEvent sunrise => $"Sunrise {FormatOffset(sunrise.Offset)} on {FormatDaysOfWeek(sunrise.DaysOfWeek)}",
                SunsetOffsetScheduleEvent sunset => $"Sunset {FormatOffset(sunset.Offset)} on {FormatDaysOfWeek(sunset.DaysOfWeek)}",
                _ => "Unknown event type"
            };
        }
    }

    private string FormatTime(DateTime utcTime)
    {
        return utcTime.ToString("HH:mm");
    }

    private string FormatLocalTime(DateTime utcTime)
    {
        var localTime = _timezone.ConvertUtcToLocal(utcTime);
        var isDst = _timezone.IsDaylightSavingTimeActive(utcTime);
        var offsetStr = isDst ? $"UTC{_timezone.GetTotalUtcOffset(utcTime):+0.0;-0.0}" : $"UTC{_timezone.UtcOffsetHours:+0.0;-0.0}";
        return $"{localTime:HH:mm} {offsetStr}";
    }

    private string FormatOffset(TimeSpan offset)
    {
        if (offset == TimeSpan.Zero) return "exactly";
        return offset < TimeSpan.Zero ? $"{offset.Negate():hh\\:mm} before" : $"{offset:hh\\:mm} after";
    }

    private string FormatDaysOfWeek(DayOfWeek[]? daysOfWeek)
    {
        if (daysOfWeek == null || daysOfWeek.Length == 0) return "daily";
        if (daysOfWeek.Length == 7) return "daily";
        return string.Join(", ", daysOfWeek.Select(d => d.ToString().Substring(0, 3)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}