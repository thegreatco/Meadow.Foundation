using System;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Represents a schedule event that triggers on specific days of the week at a specific time.
/// </summary>
public class WeekdayScheduleEvent : IScheduleEvent
{
    /// <summary>
    /// Gets the type of this schedule event.
    /// </summary>
    public ScheduleEventType EventType => ScheduleEventType.Weekday;

    /// <summary>
    /// Gets or sets a value indicating whether this event is disabled.
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Gets the desired state that should be set when this event triggers.
    /// </summary>
    public bool DesiredState { get; }

    /// <summary>
    /// Gets the time when this event should trigger (in UTC).
    /// </summary>
    public DateTime EventTime { get; }

    /// <summary>
    /// Gets the days of the week when this event should be active.
    /// </summary>
    public DayOfWeek[] DaysOfWeek { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeekdayScheduleEvent"/> class.
    /// </summary>
    /// <param name="eventTimeUtc">The time when this event should trigger (in UTC).</param>
    /// <param name="desiredState">The desired state that should be set when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active.</param>
    public WeekdayScheduleEvent(DateTime eventTimeUtc, bool desiredState, DayOfWeek[] daysOfWeek)
    {
        EventTime = eventTimeUtc;
        DesiredState = desiredState;
        DaysOfWeek = daysOfWeek;
    }
}