using System;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Represents a schedule event that triggers daily at a specific time.
/// </summary>
public class DailyScheduleEvent : IScheduleEvent
{
    /// <summary>
    /// Gets the type of this schedule event.
    /// </summary>
    public ScheduleEventType EventType => ScheduleEventType.Daily;

    /// <summary>
    /// Gets or sets a value indicating whether this event is disabled.
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Gets the desired state that should be set when this event triggers.
    /// </summary>
    public bool DesiredState { get; }

    /// <summary>
    /// Gets the time when this event should trigger daily (in UTC).
    /// </summary>
    public DateTime EventTime { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DailyScheduleEvent"/> class.
    /// </summary>
    /// <param name="eventTimeUtc">The time when this event should trigger daily (in UTC).</param>
    /// <param name="desiredState">The desired state that should be set when this event triggers.</param>
    public DailyScheduleEvent(DateTime eventTimeUtc, bool desiredState)
    {
        EventTime = eventTimeUtc;
        DesiredState = desiredState;
    }
}