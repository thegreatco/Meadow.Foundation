using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Provides a service for managing and executing scheduled events on circuits.
/// The service evaluates schedules periodically and triggers circuit state changes as needed.
/// </summary>
public class ScheduleService : IDisposable
{
    private readonly ITimeProvider _timeProvider;
    private readonly ICircuitStateController? _circuitStateController;
    private Timer? _timer;
    private readonly Dictionary<string, bool> _lastKnownStates;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    private MasterSchedule _masterSchedule;
    private bool _isDisposed;

    /// <summary>
    /// Occurs when a schedule event is triggered and a circuit state is changed.
    /// </summary>
    public event EventHandler<ScheduleEventTriggeredEventArgs> ScheduleEventTriggered;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleService"/> class using the system time provider.
    /// </summary>
    public ScheduleService()
        : this(new SystemTimeProvider(), null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleService"/> class with a circuit state controller.
    /// </summary>
    /// <param name="circuitStateController">The circuit state controller to use for managing circuit states.</param>
    public ScheduleService(ICircuitStateController? circuitStateController)
        : this(new SystemTimeProvider(), circuitStateController)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleService"/> class with custom providers.
    /// </summary>
    /// <param name="timeProvider">The time provider to use for getting current time and sunrise/sunset information.</param>
    /// <param name="circuitStateController">The circuit state controller to use for managing circuit states.</param>
    public ScheduleService(
        ITimeProvider timeProvider,
        ICircuitStateController? circuitStateController
    )
    {
        _timeProvider = timeProvider;
        _circuitStateController = circuitStateController;
        _lastKnownStates = new Dictionary<string, bool>();
    }

    /// <summary>
    /// Starts the schedule service timer to begin evaluating schedules.
    /// The timer runs every minute, starting at the next minute boundary.
    /// </summary>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public async Task Start()
    {
        if (_timer == null)
        {
            // Create timer that ticks every minute, starting at the next minute boundary
            var now = await _timeProvider.GetUtcNow();
            var nextMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);
            var initialDelay = nextMinute - now;

            _timer = new Timer(OnTimerTickCallback, null, initialDelay, TimeSpan.FromMinutes(1));
        }
    }

    /// <summary>
    /// Sets the master schedule that this service will evaluate.
    /// </summary>
    /// <param name="masterSchedule">The master schedule containing all circuit schedules.</param>
    /// <returns>A task that represents the asynchronous set operation.</returns>
    public async Task SetSchedule(MasterSchedule masterSchedule)
    {
        await _lock.WaitAsync();
        try
        {
            _masterSchedule = masterSchedule;

            if (_circuitStateController != null)
            {
                // Initialize known states for all circuits
                if (_masterSchedule?.Schedules != null)
                {
                    foreach (var schedule in _masterSchedule.Schedules)
                    {
                        if (!_lastKnownStates.ContainsKey(schedule.CircuitName))
                        {
                            _lastKnownStates[schedule.CircuitName] = await _circuitStateController.GetCircuitState(schedule.CircuitName);
                        }
                    }
                }
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Updates the service's cached state for a circuit to match the actual current state.
    /// Use this when external code has changed a circuit state that the service should know about.
    /// </summary>
    /// <param name="circuitName">The name of the circuit to synchronize.</param>
    /// <returns>A task that represents the asynchronous synchronization operation.</returns>
    public async Task SyncCircuitState(string circuitName)
    {
        if (_circuitStateController == null)
            return;

        await _lock.WaitAsync();
        try
        {
            _lastKnownStates[circuitName] = await _circuitStateController.GetCircuitState(circuitName);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Updates the service's cached states for all circuits to match their actual current states.
    /// Use this when external code may have changed circuit states that the service should know about.
    /// </summary>
    /// <returns>A task that represents the asynchronous synchronization operation.</returns>
    public async Task SyncAllCircuitStates()
    {
        if (_circuitStateController == null || _masterSchedule?.Schedules == null)
            return;

        await _lock.WaitAsync();
        try
        {
            foreach (var schedule in _masterSchedule.Schedules)
            {
                _lastKnownStates[schedule.CircuitName] = await _circuitStateController.GetCircuitState(schedule.CircuitName);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Manually applies the current schedule immediately, evaluating all schedules against the current time.
    /// </summary>
    /// <returns>A task that represents the asynchronous apply operation.</returns>
    public async Task ApplyScheduleAsync()
    {
        await OnTimerTick();
    }

    /// <summary>
    /// Manually applies the current schedule immediately, evaluating all schedules against the current time.
    /// This is a synchronous version for backwards compatibility.
    /// </summary>
    public void ApplySchedule()
    {
        // Synchronous version for backwards compatibility
        OnTimerTick().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Timer callback method that triggers schedule evaluation.
    /// </summary>
    /// <param name="_">Unused timer parameter.</param>
    private void OnTimerTickCallback(object? _)
    {
        // Fire and forget for timer callbacks
        _ = Task.Run(async () => await OnTimerTick());
    }

    /// <summary>
    /// Performs the main schedule evaluation logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous evaluation operation.</returns>
    private async Task OnTimerTick()
    {
        if (_isDisposed)
            return;

        await _lock.WaitAsync();
        try
        {
            await EvaluateSchedules();
        }
        catch (Exception ex)
        {
            // Log error but don't let it kill the timer
            OnScheduleError(ex);
        }
        finally
        {
            _lock.Release(); // CRITICAL FIX: Always release the semaphore
        }
    }

    /// <summary>
    /// Evaluates all schedules against the current time and updates circuit states as needed.
    /// </summary>
    /// <returns>A task that represents the asynchronous evaluation operation.</returns>
    private async Task EvaluateSchedules()
    {
        if (_masterSchedule?.Schedules == null)
            return;

        var currentTime = await _timeProvider.GetUtcNow();
        (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes = null;

        if (_masterSchedule.ContainsSunriseOrSunsetEvents)
        {
            sunTimes = await _timeProvider.GetUtcSunriseAndSunset();
        }

        foreach (var schedule in _masterSchedule.Schedules)
        {
            var desiredState = EvaluateCircuitState(schedule, currentTime, sunTimes);

            if (desiredState.HasValue)
            {
                var raiseEvent = false;

                if (_circuitStateController == null)
                {
                    raiseEvent = true;
                }
                else
                {
                    var currentState = _lastKnownStates.GetValueOrDefault(schedule.CircuitName, false);

                    if (currentState != desiredState.Value)
                    {
                        await _circuitStateController.SetCircuitState(schedule.CircuitName, desiredState.Value);
                        _lastKnownStates[schedule.CircuitName] = desiredState.Value;
                        raiseEvent = true;
                    }
                }

                if (raiseEvent)
                {
                    // Find the triggering event for logging/events
                    var triggeringEvent = FindTriggeringEvent(schedule, currentTime, sunTimes);
                    if (triggeringEvent != null)
                    {
                        OnScheduleEventTriggered(new ScheduleEventTriggeredEventArgs(
                            schedule.CircuitName,
                            triggeringEvent,
                            desiredState.Value,
                            currentTime));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Evaluates the desired state for a specific circuit based on its schedule and current time.
    /// </summary>
    /// <param name="schedule">The schedule to evaluate.</param>
    /// <param name="currentTime">The current time.</param>
    /// <param name="sunTimes">The sunrise and sunset times for the current day, if needed.</param>
    /// <returns>The desired state for the circuit, or null if no events are active.</returns>
    private bool? EvaluateCircuitState(Schedule schedule, DateTimeOffset currentTime, (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes)
    {
        if (schedule.Events == null || !schedule.Events.Any())
            return null;

        // Find all events that should be active right now
        var activeEvents = new List<(IScheduleEvent Event, int Priority)>();

        foreach (var scheduleEvent in schedule.Events)
        {
            if (scheduleEvent.IsDisabled)
                continue;

            if (IsEventActive(scheduleEvent, currentTime, sunTimes))
            {
                var priority = GetEventPriority(scheduleEvent);
                activeEvents.Add((scheduleEvent, priority));
            }
        }

        if (!activeEvents.Any())
            return null;

        // Use the highest priority event (lowest number = highest priority)
        var highestPriorityEvent = activeEvents
            .OrderBy(e => e.Priority)
            .ThenBy(e => GetEventSpecificity(e.Event)) // More specific events win
            .First();

        return GetEventDesiredState(highestPriorityEvent.Event);
    }

    /// <summary>
    /// Determines if a schedule event is currently active based on the current time.
    /// </summary>
    /// <param name="scheduleEvent">The schedule event to check.</param>
    /// <param name="currentTime">The current time.</param>
    /// <param name="sunTimes">The sunrise and sunset times for the current day, if needed.</param>
    /// <returns>True if the event is active; otherwise, false.</returns>
    private bool IsEventActive(IScheduleEvent scheduleEvent, DateTimeOffset currentTime, (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes)
    {
        var currentTimeOfDay = currentTime.TimeOfDay;
        var currentDayOfWeek = currentTime.DayOfWeek;

        return scheduleEvent switch
        {
            DailyScheduleEvent daily =>
                IsTimeMatch(daily.EventTime.TimeOfDay, currentTimeOfDay),

            WeekdayScheduleEvent weekday =>
                weekday.DaysOfWeek.Contains(currentDayOfWeek) &&
                IsTimeMatch(weekday.EventTime.TimeOfDay, currentTimeOfDay),

            SunriseOffsetScheduleEvent sunrise =>
                (sunrise.DaysOfWeek == null || sunrise.DaysOfWeek.Contains(currentDayOfWeek)) &&
                IsTimeMatch(sunTimes!.Value.Sunrise.Add(sunrise.Offset).TimeOfDay, currentTimeOfDay),

            SunsetOffsetScheduleEvent sunset =>
                (sunset.DaysOfWeek == null || sunset.DaysOfWeek.Contains(currentDayOfWeek)) &&
                IsTimeMatch(sunTimes!.Value.Sunset.Add(sunset.Offset).TimeOfDay, currentTimeOfDay),

            _ => false
        };
    }

    /// <summary>
    /// Determines if two times match within the same minute.
    /// </summary>
    /// <param name="eventTime">The event time to match.</param>
    /// <param name="currentTime">The current time.</param>
    /// <returns>True if the times are within the same minute; otherwise, false.</returns>
    private bool IsTimeMatch(TimeSpan eventTime, TimeSpan currentTime)
    {
        // Match if we're within the same minute
        var eventMinutes = (int)eventTime.TotalMinutes;
        var currentMinutes = (int)currentTime.TotalMinutes;
        return eventMinutes == currentMinutes;
    }

    /// <summary>
    /// Gets the priority level for a schedule event type.
    /// Lower numbers indicate higher priority.
    /// </summary>
    /// <param name="scheduleEvent">The schedule event to get priority for.</param>
    /// <returns>The priority level as an integer.</returns>
    private int GetEventPriority(IScheduleEvent scheduleEvent)
    {
        // Lower numbers = higher priority
        return scheduleEvent switch
        {
            WeekdayScheduleEvent => 1,     // Highest priority - most specific
            SunriseOffsetScheduleEvent => 2, // Medium-high priority
            SunsetOffsetScheduleEvent => 2,  // Medium-high priority  
            DailyScheduleEvent => 3,       // Lowest priority - most general
            _ => 999
        };
    }

    /// <summary>
    /// Gets the specificity level for a schedule event, used as a tie-breaker for same priority events.
    /// Lower numbers indicate more specific events.
    /// </summary>
    /// <param name="scheduleEvent">The schedule event to get specificity for.</param>
    /// <returns>The specificity level as an integer.</returns>
    private int GetEventSpecificity(IScheduleEvent scheduleEvent)
    {
        // Used as tie-breaker for same priority events
        // Lower numbers = more specific
        return scheduleEvent switch
        {
            WeekdayScheduleEvent weekday => weekday.DaysOfWeek?.Length ?? 7,
            SunriseOffsetScheduleEvent sunrise => sunrise.DaysOfWeek?.Length ?? 7,
            SunsetOffsetScheduleEvent sunset => sunset.DaysOfWeek?.Length ?? 7,
            DailyScheduleEvent => 7,
            _ => 999
        };
    }

    /// <summary>
    /// Gets the desired state from a schedule event.
    /// </summary>
    /// <param name="scheduleEvent">The schedule event to get the desired state from.</param>
    /// <returns>The desired state as a boolean value.</returns>
    private bool GetEventDesiredState(IScheduleEvent scheduleEvent)
    {
        return scheduleEvent switch
        {
            DailyScheduleEvent daily => daily.DesiredState,
            WeekdayScheduleEvent weekday => weekday.DesiredState,
            SunriseOffsetScheduleEvent sunrise => sunrise.DesiredState,
            SunsetOffsetScheduleEvent sunset => sunset.DesiredState,
            _ => false
        };
    }

    /// <summary>
    /// Finds the schedule event that is currently triggering for a given schedule.
    /// </summary>
    /// <param name="schedule">The schedule to check.</param>
    /// <param name="currentTime">The current time.</param>
    /// <param name="sunTimes">The sunrise and sunset times for the current day, if needed.</param>
    /// <returns>The triggering event, or null if no event is currently triggering.</returns>
    private IScheduleEvent? FindTriggeringEvent(Schedule schedule, DateTimeOffset currentTime, (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes)
    {
        foreach (var scheduleEvent in schedule.Events)
        {
            if (scheduleEvent.IsDisabled)
                continue;

            if (IsEventActive(scheduleEvent, currentTime, sunTimes))
            {
                return scheduleEvent;
            }
        }
        return null;
    }

    /// <summary>
    /// Raises the ScheduleEventTriggered event.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected virtual void OnScheduleEventTriggered(ScheduleEventTriggeredEventArgs e)
    {
        ScheduleEventTriggered?.Invoke(this, e);
    }

    /// <summary>
    /// Handles schedule errors. Override this method to implement custom error handling/logging.
    /// </summary>
    /// <param name="exception">The exception that occurred during schedule evaluation.</param>
    protected virtual void OnScheduleError(Exception exception)
    {
        // Override this method to implement custom error handling/logging
        // For now, we'll just swallow the exception to keep the timer running
    }

    /// <summary>
    /// Releases all resources used by the ScheduleService.
    /// </summary>
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _timer?.Dispose();
            _lock?.Dispose();
            _isDisposed = true;
        }
    }
}