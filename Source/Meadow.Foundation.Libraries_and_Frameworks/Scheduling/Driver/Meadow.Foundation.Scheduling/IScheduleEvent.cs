namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Defines the basic properties that all schedule events must implement.
/// </summary>
public interface IScheduleEvent
{
    /// <summary>
    /// Gets the type of this schedule event.
    /// </summary>
    ScheduleEventType EventType { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this event is disabled.
    /// When disabled, the event will not trigger or affect circuit states.
    /// </summary>
    bool IsDisabled { get; set; }
}