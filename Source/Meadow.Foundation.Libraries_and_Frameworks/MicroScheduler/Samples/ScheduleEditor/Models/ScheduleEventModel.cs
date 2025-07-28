using Meadow.Foundation.Scheduling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ScheduleEditor.Models;

/// <summary>
/// UI model for a schedule event with timezone-aware time display.
/// </summary>
public class ScheduleEventModel : INotifyPropertyChanged
{
    private readonly IScheduleEvent _scheduleEvent;
    private readonly TimezoneInfo _timezone;
    private bool _isDirty;
    private string? _selectedEventType;
    private int? _offsetMinutes;
    private bool? _isOffsetBefore;
    private Dictionary<DayOfWeek, bool>? _daySelections;
    private TimeSpan? _eventTime;

    public ScheduleEventModel(IScheduleEvent scheduleEvent, TimezoneInfo timezone)
    {
        _scheduleEvent = scheduleEvent;
        _timezone = timezone;
        _selectedEventType = scheduleEvent.EventType.ToString(); // Initialize with actual event type
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

    /// <summary>
    /// Creates a new schedule event object with the current UI values
    /// </summary>
    public IScheduleEvent CreateUpdatedEvent()
    {
        var selectedType = _selectedEventType ?? _scheduleEvent.EventType.ToString();
        var eventData = Data ?? "true"; // Default to "true" if no data

        // Get current days of week if applicable
        var daysOfWeek = GetSelectedDaysOfWeek() ?? GetOriginalDaysOfWeek();

        return selectedType switch
        {
            "Daily" => new DailyScheduleEvent(
                GetEventDateTime(),
                eventData)
            {
                IsDisabled = IsDisabled
            },

            "Weekday" => new WeekdayScheduleEvent(
                GetEventDateTime(),
                eventData,
                daysOfWeek ?? new[] { DayOfWeek.Monday })
            {
                IsDisabled = IsDisabled
            },

            "SunriseOffset" => new SunriseOffsetScheduleEvent(
                GetOffsetTimeSpan(),
                eventData,
                daysOfWeek ?? new[] { DayOfWeek.Monday })
            {
                IsDisabled = IsDisabled
            },

            "SunsetOffset" => new SunsetOffsetScheduleEvent(
                GetOffsetTimeSpan(),
                eventData,
                daysOfWeek ?? new[] { DayOfWeek.Monday })
            {
                IsDisabled = IsDisabled
            },

            _ => _scheduleEvent // Fallback to original event
        };
    }

    private DateTimeOffset GetEventDateTime()
    {
        // Use the EventTime if available (from TimePicker), otherwise use current event time
        if (EventTime.HasValue)
        {
            var baseDate = new DateTime(1989, 6, 3); // Use a consistent base date
            return baseDate.Date.Add(EventTime.Value);
        }

        // Fallback to original event time
        return _scheduleEvent switch
        {
            DailyScheduleEvent daily => daily.EventTime,
            WeekdayScheduleEvent weekday => weekday.EventTime,
            _ => DateTime.UtcNow.Date.AddHours(12) // Default to noon
        };
    }

    private TimeSpan GetOffsetTimeSpan()
    {
        var minutes = OffsetMinutes;
        return IsOffsetBefore ? TimeSpan.FromMinutes(-minutes) : TimeSpan.FromMinutes(minutes);
    }

    private DayOfWeek[]? GetSelectedDaysOfWeek()
    {
        // Only return UI selections if any changes were made
        if (_daySelections != null && _daySelections.Count > 0)
        {
            var days = new List<DayOfWeek>();

            if (MondaySelected) days.Add(DayOfWeek.Monday);
            if (TuesdaySelected) days.Add(DayOfWeek.Tuesday);
            if (WednesdaySelected) days.Add(DayOfWeek.Wednesday);
            if (ThursdaySelected) days.Add(DayOfWeek.Thursday);
            if (FridaySelected) days.Add(DayOfWeek.Friday);
            if (SaturdaySelected) days.Add(DayOfWeek.Saturday);
            if (SundaySelected) days.Add(DayOfWeek.Sunday);

            return days.ToArray(); // Return empty array if no days selected, not null
        }

        return null; // No UI changes, preserve original
    }

    private DayOfWeek[]? GetOriginalDaysOfWeek()
    {
        return _scheduleEvent switch
        {
            WeekdayScheduleEvent weekday => weekday.DaysOfWeek,
            SunriseOffsetScheduleEvent sunrise => sunrise.DaysOfWeek,
            SunsetOffsetScheduleEvent sunset => sunset.DaysOfWeek,
            _ => null
        };
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
        get => _selectedEventType ?? _scheduleEvent.EventType.ToString();
        set
        {
            if (_selectedEventType != value)
            {
                Console.WriteLine($"EventType changed from {_selectedEventType} to {value}");
                _selectedEventType = value;
                // Note: Changing event type would require recreating the event object
                // For now, we'll just track the change but not implement the conversion
                IsDirty = true;
                OnPropertyChanged(nameof(EventType));
                OnPropertyChanged(nameof(TimeDisplay)); // Time display might change based on event type
                OnPropertyChanged(nameof(IsWeekdayEvent)); // Update checkbox visibility
                OnPropertyChanged(nameof(SupportsTimePicker)); // Update time picker visibility
                OnPropertyChanged(nameof(SupportsOffsetEditor)); // Update offset editor visibility
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
    /// Gets or sets the offset minutes for sunrise/sunset events
    /// </summary>
    public int OffsetMinutes
    {
        get
        {
            if (_offsetMinutes.HasValue)
                return _offsetMinutes.Value;

            return _scheduleEvent switch
            {
                SunriseOffsetScheduleEvent sunrise => Math.Abs((int)sunrise.Offset.TotalMinutes),
                SunsetOffsetScheduleEvent sunset => Math.Abs((int)sunset.Offset.TotalMinutes),
                _ => 0
            };
        }
        set
        {
            Console.WriteLine($"OffsetMinutes setter called with value: {value}");
            _offsetMinutes = value;
            IsDirty = true;
            OnPropertyChanged(nameof(OffsetMinutes));
            OnPropertyChanged(nameof(TimeDisplay));
        }
    }

    /// <summary>
    /// Gets or sets whether the offset is before (true) or after (false) sunrise/sunset
    /// </summary>
    public bool IsOffsetBefore
    {
        get
        {
            if (_isOffsetBefore.HasValue)
                return _isOffsetBefore.Value;

            return _scheduleEvent switch
            {
                SunriseOffsetScheduleEvent sunrise => sunrise.Offset < TimeSpan.Zero,
                SunsetOffsetScheduleEvent sunset => sunset.Offset < TimeSpan.Zero,
                _ => false
            };
        }
        set
        {
            Console.WriteLine($"IsOffsetBefore setter called with value: {value}");
            _isOffsetBefore = value;
            IsDirty = true;
            OnPropertyChanged(nameof(IsOffsetBefore));
            OnPropertyChanged(nameof(OffsetBeforeAfterText));
            OnPropertyChanged(nameof(TimeDisplay));
        }
    }

    /// <summary>
    /// Gets the text for the before/after selector
    /// </summary>
    public string OffsetBeforeAfterText => IsOffsetBefore ? "before" : "after";

    /// <summary>
    /// Available options for before/after selector
    /// </summary>
    public List<string> AvailableOffsetOptions => new() { "before", "after" };

    /// <summary>
    /// Gets or sets the before/after text for binding to ComboBox
    /// </summary>
    public string OffsetBeforeAfterSelection
    {
        get => OffsetBeforeAfterText;
        set
        {
            var isBefore = value == "before";
            if (IsOffsetBefore != isBefore)
            {
                IsOffsetBefore = isBefore;
            }
        }
    }

    /// <summary>
    /// Gets or sets the event time as a TimeSpan for TimePicker binding
    /// </summary>
    public TimeSpan? EventTime
    {
        get
        {
            if (_eventTime.HasValue)
                return _eventTime.Value;

            return _scheduleEvent switch
            {
                DailyScheduleEvent daily => daily.EventTime.TimeOfDay,
                WeekdayScheduleEvent weekday => weekday.EventTime.TimeOfDay,
                _ => null
            };
        }
        set
        {
            if (value.HasValue)
            {
                Console.WriteLine($"EventTime setter called with value: {value.Value}");
                _eventTime = value.Value;
                IsDirty = true;
                OnPropertyChanged(nameof(EventTime));
                OnPropertyChanged(nameof(TimeDisplay));
            }
        }
    }

    /// <summary>
    /// Gets whether this event supports days of the week selection (Weekday, SunriseOffset, SunsetOffset)
    /// </summary>
    public bool SupportsDaysOfWeek => _scheduleEvent.EventType is ScheduleEventType.Weekday or ScheduleEventType.SunriseOffset or ScheduleEventType.SunsetOffset;

    /// <summary>
    /// Gets whether this event shows day-of-week checkboxes (Weekday, SunriseOffset, SunsetOffset)
    /// </summary>
    public bool IsWeekdayEvent => (_selectedEventType ?? _scheduleEvent.EventType.ToString()) is "Weekday" or "SunriseOffset" or "SunsetOffset";

    /// <summary>
    /// Gets whether this event supports time picker editing (Daily and Weekday events)
    /// </summary>
    public bool SupportsTimePicker => (_selectedEventType ?? _scheduleEvent.EventType.ToString()) is "Daily" or "Weekday";

    /// <summary>
    /// Gets whether this event supports offset editing (Sunrise/Sunset events)
    /// </summary>
    public bool SupportsOffsetEditor => (_selectedEventType ?? _scheduleEvent.EventType.ToString()) is "SunriseOffset" or "SunsetOffset";

    /// <summary>
    /// Gets or sets whether Monday is selected for this event
    /// </summary>
    public bool MondaySelected
    {
        get => GetDaySelected(DayOfWeek.Monday);
        set => SetDaySelected(DayOfWeek.Monday, value);
    }

    /// <summary>
    /// Gets or sets whether Tuesday is selected for this event
    /// </summary>
    public bool TuesdaySelected
    {
        get => GetDaySelected(DayOfWeek.Tuesday);
        set => SetDaySelected(DayOfWeek.Tuesday, value);
    }

    /// <summary>
    /// Gets or sets whether Wednesday is selected for this event
    /// </summary>
    public bool WednesdaySelected
    {
        get => GetDaySelected(DayOfWeek.Wednesday);
        set => SetDaySelected(DayOfWeek.Wednesday, value);
    }

    /// <summary>
    /// Gets or sets whether Thursday is selected for this event
    /// </summary>
    public bool ThursdaySelected
    {
        get => GetDaySelected(DayOfWeek.Thursday);
        set => SetDaySelected(DayOfWeek.Thursday, value);
    }

    /// <summary>
    /// Gets or sets whether Friday is selected for this event
    /// </summary>
    public bool FridaySelected
    {
        get => GetDaySelected(DayOfWeek.Friday);
        set => SetDaySelected(DayOfWeek.Friday, value);
    }

    /// <summary>
    /// Gets or sets whether Saturday is selected for this event
    /// </summary>
    public bool SaturdaySelected
    {
        get => GetDaySelected(DayOfWeek.Saturday);
        set => SetDaySelected(DayOfWeek.Saturday, value);
    }

    /// <summary>
    /// Gets or sets whether Sunday is selected for this event
    /// </summary>
    public bool SundaySelected
    {
        get => GetDaySelected(DayOfWeek.Sunday);
        set => SetDaySelected(DayOfWeek.Sunday, value);
    }

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

    private string FormatTime(DateTimeOffset utcTime)
    {
        return utcTime.ToString("HH:mm");
    }

    private string FormatLocalTime(DateTimeOffset utcTime)
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

    private bool GetDaySelected(DayOfWeek day)
    {
        // Check if we have a UI override first
        if (_daySelections?.ContainsKey(day) == true)
            return _daySelections[day];

        // Fall back to original event
        return _scheduleEvent switch
        {
            WeekdayScheduleEvent weekday => weekday.DaysOfWeek?.Contains(day) ?? false,
            SunriseOffsetScheduleEvent sunrise => sunrise.DaysOfWeek?.Contains(day) ?? false,
            SunsetOffsetScheduleEvent sunset => sunset.DaysOfWeek?.Contains(day) ?? false,
            _ => false
        };
    }

    private void SetDaySelected(DayOfWeek day, bool selected)
    {
        Console.WriteLine($"SetDaySelected: {day} = {selected} for {_scheduleEvent.EventType}");

        // Initialize the dictionary if needed
        if (_daySelections == null)
            _daySelections = new Dictionary<DayOfWeek, bool>();

        // Store the selection
        _daySelections[day] = selected;

        IsDirty = true;
        OnPropertyChanged($"{day}Selected");
        OnPropertyChanged(nameof(TimeDisplay)); // Update time display as it shows days
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
