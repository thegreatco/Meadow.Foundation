using System;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Provides data for the ScheduleEventTriggered event.
/// </summary>
public class ScheduleEventTriggeredEventArgs : EventArgs
{
    /// <summary>
    /// Gets the Schedule that was affected by the schedule event.
    /// </summary>
    public Schedule Schedule { get; }

    /// <summary>
    /// Gets the schedule event that was triggered.
    /// </summary>
    public IScheduleEvent ScheduleEvent { get; }

    /// <summary>
    /// Gets the data for this event
    /// </summary>
    public string? Data { get; }

    /// <summary>
    /// Gets the date and time when the event was triggered.
    /// </summary>
    public DateTimeOffset TriggeredAt { get; }

    /// <summary>
    /// Gets the name of the circuit that was affected by the schedule event.
    /// This is a convenience property that returns Schedule.Name.
    /// </summary>
    public string CircuitName => Schedule.Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleEventTriggeredEventArgs"/> class.
    /// </summary>
    /// <param name="schedule">The schedule that was affected.</param>
    /// <param name="scheduleEvent">The schedule event that was triggered.</param>
    /// <param name="newState">The new state that the circuit was set to.</param>
    /// <param name="data">Data associated with this event.</param>
    public ScheduleEventTriggeredEventArgs(Schedule schedule, IScheduleEvent scheduleEvent, string? data, DateTimeOffset triggeredAt)
    {
        Schedule = schedule;
        ScheduleEvent = scheduleEvent;
        Data = data;
        TriggeredAt = triggeredAt;
    }
}