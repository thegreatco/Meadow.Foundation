using Meadow.Foundation.Scheduling;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ScheduleEditor.Models;

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

    /// <summary>
    /// Applies all pending changes from dirty events to the underlying schedule
    /// </summary>
    public void ApplyChanges()
    {
        var eventsToUpdate = Events.Where(e => e.IsDirty).ToList();

        foreach (var eventModel in eventsToUpdate)
        {
            // Remove the old event
            _schedule.Events.Remove(eventModel.ScheduleEvent);

            // Create and add the updated event
            var updatedEvent = eventModel.CreateUpdatedEvent();
            _schedule.Events.Add(updatedEvent);

            // Mark as clean
            eventModel.MarkClean();
        }

        // Refresh the UI models to reflect the new underlying events
        if (eventsToUpdate.Count > 0)
        {
            RefreshEvents();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
