namespace Meadow.Foundation.Scheduling.Unit.Tests;

// Test implementations of interfaces
public class TestTimeProvider : ITimeProvider
{
    public DateTimeOffset Now { get; set; } = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero); // Saturday, June 15, 2024, noon UTC

    public ValueTask<DateTimeOffset> GetUtcNow()
    {
        return new ValueTask<DateTimeOffset>(Now);
    }

    public ValueTask<(DateTimeOffset sunrise, DateTimeOffset sunset)> GetUtcSunriseAndSunset()
    {
        var date = Now.Date;
        var sunrise = new DateTimeOffset(date.Year, date.Month, date.Day, 6, 30, 0, TimeSpan.Zero); // 6:30 AM UTC
        var sunset = new DateTimeOffset(date.Year, date.Month, date.Day, 18, 30, 0, TimeSpan.Zero); // 6:30 PM UTC
        return new ValueTask<(DateTimeOffset, DateTimeOffset)>((sunrise, sunset));
    }
}

public class TestCircuitStateController : ICircuitStateController
{
    private readonly Dictionary<string, bool> _states = new Dictionary<string, bool>();
    public List<(string CircuitName, bool State, DateTimeOffset Time)> StateChanges { get; } = new List<(string, bool, DateTimeOffset)>();

    public ValueTask<bool> GetCircuitState(string circuitName)
    {
        return new ValueTask<bool>(_states.GetValueOrDefault(circuitName, false));
    }

    public Task SetCircuitState(string circuitName, bool state)
    {
        _states[circuitName] = state;
        StateChanges.Add((circuitName, state, DateTimeOffset.UtcNow));
        return Task.CompletedTask;
    }

    public void ClearHistory()
    {
        StateChanges.Clear();
    }

    public Dictionary<string, bool> GetAllStates()
    {
        return new Dictionary<string, bool>(_states);
    }
}

public class ScheduleServiceTests : IDisposable
{
    private readonly TestTimeProvider _timeProvider;
    private readonly TestCircuitStateController _circuitController;
    private readonly ScheduleService _scheduleService;

    public ScheduleServiceTests()
    {
        _timeProvider = new TestTimeProvider();
        _circuitController = new TestCircuitStateController();
        _scheduleService = new ScheduleService(_timeProvider, _circuitController);
    }

    [Fact]
    public async Task ApplySchedule_WithSunriseOffsetBugFix_ShouldApplyOffsetCorrectly()
    {
        // This test specifically validates the bug fix for sunrise/sunset offset calculation

        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        // 30 minutes AFTER sunrise (6:30 + 0:30 = 7:00)
                        new SunriseOffsetScheduleEvent(TimeSpan.FromMinutes(30), true),
                        // 45 minutes BEFORE sunset (18:30 - 0:45 = 17:45)  
                        new SunsetOffsetScheduleEvent(TimeSpan.FromMinutes(-45), false)
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);

        // Test sunrise offset: Should trigger at 7:00 (6:30 + 30 min), NOT at 6:30
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 6, 30, 0, TimeSpan.Zero); // Exact sunrise
        _circuitController.ClearHistory();
        _scheduleService.ApplySchedule();
        Assert.Empty(_circuitController.StateChanges); // Should NOT trigger at raw sunrise time

        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 7, 0, 0, TimeSpan.Zero); // 30 min after sunrise
        _circuitController.ClearHistory();
        _scheduleService.ApplySchedule();
        Assert.Single(_circuitController.StateChanges); // SHOULD trigger at offset time
        Assert.True(_circuitController.StateChanges[0].State);

        // Test sunset offset: Should trigger at 17:45 (18:30 - 45 min), NOT at 18:30
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 18, 30, 0, TimeSpan.Zero); // Exact sunset
        _circuitController.ClearHistory();
        _scheduleService.ApplySchedule();
        Assert.Empty(_circuitController.StateChanges); // Should NOT trigger at raw sunset time

        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 17, 45, 0, TimeSpan.Zero); // 45 min before sunset
        _circuitController.ClearHistory();
        _scheduleService.ApplySchedule();
        Assert.Single(_circuitController.StateChanges); // SHOULD trigger at offset time
        Assert.False(_circuitController.StateChanges[0].State);
    }

    [Fact]
    public void Constructor_WithDefaultParameters_ShouldNotThrow()
    {
        // Test parameterless constructor
        using var service1 = new ScheduleService();

        // Test constructor with only circuit controller
        using var service2 = new ScheduleService(_circuitController);

        // Both should construct successfully
        Assert.NotNull(service1);
        Assert.NotNull(service2);
    }

    [Fact]
    public async Task SetSchedule_WithValidSchedule_ShouldNotThrow()
    {
        // Arrange
        var schedule = CreateTestSchedule();

        // Act & Assert
        await _scheduleService.SetSchedule(schedule);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithDailyEvent_ShouldTriggerAtCorrectTime()
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), true) // Noon
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 30, TimeSpan.Zero); // 12:00:30 UTC (same minute)

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_circuitController.StateChanges);
        Assert.Equal("TestLight", _circuitController.StateChanges[0].CircuitName);
        Assert.True(_circuitController.StateChanges[0].State);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithDailyEvent_ShouldNotTriggerAtWrongTime()
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), true) // Noon
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 1, 0, TimeSpan.Zero); // 12:01 UTC (different minute)

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_circuitController.StateChanges);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithWeekdayEvent_ShouldTriggerOnCorrectDay()
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new WeekdayScheduleEvent(
                            new DateTime(2024, 1, 1, 12, 0, 0),
                            true,
                            new[] { DayOfWeek.Saturday }) // Saturday only
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero); // Saturday, June 15, noon UTC

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_circuitController.StateChanges);
        Assert.True(_circuitController.StateChanges[0].State);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithWeekdayEvent_ShouldNotTriggerOnWrongDay()
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new WeekdayScheduleEvent(
                            new DateTime(2024, 1, 1, 12, 0, 0),
                            true,
                            new[] { DayOfWeek.Monday }) // Monday only
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero); // Saturday, June 15, noon UTC

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_circuitController.StateChanges);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithSunriseOffset_ShouldTriggerAtCorrectTime()
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        // 30 minutes after sunrise
                        new SunriseOffsetScheduleEvent(TimeSpan.FromMinutes(30), true)
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        // Sunrise is at 6:30 UTC, so 30 minutes after is 7:00 UTC
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 7, 0, 0, TimeSpan.Zero);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_circuitController.StateChanges);
        Assert.True(_circuitController.StateChanges[0].State);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithSunsetOffset_ShouldTriggerAtCorrectTime()
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        // 30 minutes before sunset
                        new SunsetOffsetScheduleEvent(TimeSpan.FromMinutes(-30), true)
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        // Sunset is at 18:30 UTC, so 30 minutes before is 18:00 UTC
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 18, 0, 0, TimeSpan.Zero);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_circuitController.StateChanges);
        Assert.True(_circuitController.StateChanges[0].State);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithDisabledEvent_ShouldNotTrigger()
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), true)
                        {
                            IsDisabled = true
                        }
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_circuitController.StateChanges);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithMultipleEventsAtSameTime_ShouldUseHighestPriority()
    {
        // Arrange - Weekday event should have higher priority than Daily event
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), false), // Daily = lower priority
                        new WeekdayScheduleEvent(
                            new DateTime(2024, 1, 1, 12, 0, 0),
                            true,
                            new[] { DayOfWeek.Saturday }) // Weekday = higher priority
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero); // Saturday

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_circuitController.StateChanges);
        Assert.True(_circuitController.StateChanges[0].State); // Should use weekday event (true), not daily (false)
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithSameStateChange_ShouldNotTriggerStateChange()
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), true)
                    }
                }
            }
        };

        // Set initial state to true BEFORE setting the schedule
        // This way the service will cache the correct initial state
        await _circuitController.SetCircuitState("TestLight", true);

        await _scheduleService.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Clear history after setup to only track changes from ApplyScheduleAsync
        _circuitController.ClearHistory();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert - No state change should occur since state is already correct
        Assert.Empty(_circuitController.StateChanges);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithExternalStateChange_DemonstratesCachedStateBehavior()
    {
        // This test demonstrates the current behavior when external code changes circuit state
        // after the service has cached the initial state

        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), true)
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule); // Service caches state as false
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // External code changes the circuit state (service doesn't know about this)
        await _circuitController.SetCircuitState("TestLight", true);
        _circuitController.ClearHistory();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert - Service will trigger a state change because it thinks state is still false
        // This demonstrates why it's important to set initial states before calling SetSchedule
        Assert.Single(_circuitController.StateChanges);
        Assert.True(_circuitController.StateChanges[0].State);

        // Note: In a real application, you would either:
        // 1. Set all initial states before calling SetSchedule, or
        // 2. Add a method to sync the service's known states with actual states
    }

    [Fact]
    public async Task SyncCircuitState_AfterExternalChange_ShouldPreventUnnecessaryStateChange()
    {
        // This test shows how to use SyncCircuitState to handle external state changes

        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), true)
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule); // Service caches state as false
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // External code changes the circuit state
        await _circuitController.SetCircuitState("TestLight", true);

        // Sync the service's knowledge of the circuit state
        await _scheduleService.SyncCircuitState("TestLight");

        _circuitController.ClearHistory();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert - No state change should occur because service now knows the correct state
        Assert.Empty(_circuitController.StateChanges);
    }

    [Fact]
    public async Task SyncAllCircuitStates_AfterMultipleExternalChanges_ShouldSyncAllStates()
    {
        // This test shows how to use SyncAllCircuitStates for multiple circuits

        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), true)
                    }
                },
                new Schedule
                {
                    CircuitName = "TestFan",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), false)
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule); // Both circuits cached as false
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // External code changes both circuit states to their desired states
        await _circuitController.SetCircuitState("TestLight", true);
        await _circuitController.SetCircuitState("TestFan", false);

        // Sync all circuit states at once
        await _scheduleService.SyncAllCircuitStates();

        _circuitController.ClearHistory();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert - No state changes should occur because service knows the correct states
        Assert.Empty(_circuitController.StateChanges);
    }

    [Fact]
    public async Task SyncCircuitState_WithNullController_ShouldNotThrow()
    {
        // Test that sync methods handle null circuit controller gracefully
        using var serviceWithoutController = new ScheduleService(_timeProvider, null);

        // Act & Assert - Should not throw
        await serviceWithoutController.SyncCircuitState("TestLight");
        await serviceWithoutController.SyncAllCircuitStates();
    }

    [Fact]
    public async Task ScheduleEventTriggered_ShouldFireWhenEventTriggers()
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), true)
                    }
                }
            }
        };

        ScheduleEventTriggeredEventArgs eventArgs = null;
        _scheduleService.ScheduleEventTriggered += (sender, e) => eventArgs = e;

        await _scheduleService.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.NotNull(eventArgs);
        Assert.Equal("TestLight", eventArgs.CircuitName);
        Assert.True(eventArgs.NewState);
        Assert.Equal(ScheduleEventType.Daily, eventArgs.ScheduleEvent.EventType);
    }

    [Theory]
    [InlineData(6, 30, 0, true)]  // Exact sunrise time
    [InlineData(7, 0, 0, true)]   // 30 minutes after sunrise  
    [InlineData(6, 0, 0, true)]   // 30 minutes before sunrise
    [InlineData(8, 0, 0, false)]  // Wrong time
    public async Task ApplySchedule_WithSunriseOffsetEvents_ShouldTriggerCorrectly(int hour, int minute, int second, bool shouldTrigger)
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new SunriseOffsetScheduleEvent(TimeSpan.Zero, true), // Exact sunrise
                        new SunriseOffsetScheduleEvent(TimeSpan.FromMinutes(30), true), // 30 min after
                        new SunriseOffsetScheduleEvent(TimeSpan.FromMinutes(-30), true), // 30 min before
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, hour, minute, second, TimeSpan.Zero);

        // Act
        _scheduleService.ApplySchedule();

        // Assert
        if (shouldTrigger)
        {
            Assert.NotEmpty(_circuitController.StateChanges);
        }
        else
        {
            Assert.Empty(_circuitController.StateChanges);
        }
    }

    [Fact]
    public async Task ApplySchedule_WithSunOffsetAndDaysOfWeek_ShouldRespectDayRestrictions()
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new SunriseOffsetScheduleEvent(
                            TimeSpan.Zero,
                            true,
                            new[] { DayOfWeek.Monday }) // Monday only
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 6, 30, 0, TimeSpan.Zero); // Saturday at sunrise

        // Act
        _scheduleService.ApplySchedule();

        // Assert
        Assert.Empty(_circuitController.StateChanges); // Should not trigger on Saturday
    }

    [Fact]
    public async Task ApplySchedule_WithNullCircuitController_ShouldStillFireEvents()
    {
        // Arrange - Service without circuit controller
        using var serviceWithoutController = new ScheduleService(_timeProvider, null);

        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), true)
                    }
                }
            }
        };

        ScheduleEventTriggeredEventArgs eventArgs = null;
        serviceWithoutController.ScheduleEventTriggered += (sender, e) => eventArgs = e;

        await serviceWithoutController.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Act
        serviceWithoutController.ApplySchedule();

        // Assert
        Assert.NotNull(eventArgs);
        Assert.Equal("TestLight", eventArgs.CircuitName);
        Assert.True(eventArgs.NewState);
    }

    [Theory]
    [InlineData("-00:30:00", -30)] // 30 minutes before
    [InlineData("01:00:00", 60)]   // 1 hour after
    [InlineData("00:15:00", 15)]   // 15 minutes after
    [InlineData("-02:00:00", -120)] // 2 hours before
    public async Task ApplySchedule_WithValidOffsets_ShouldCalculateCorrectTriggerTime(string offsetString, int expectedMinutes)
    {
        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new SunriseOffsetScheduleEvent(TimeSpan.FromMinutes(expectedMinutes), true)
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        // Sunrise is at 6:30, so calculate expected trigger time
        var sunriseTime = new TimeSpan(6, 30, 0);
        var expectedTriggerTime = sunriseTime.Add(TimeSpan.FromMinutes(expectedMinutes));
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero).Add(expectedTriggerTime);

        // Act
        _scheduleService.ApplySchedule();

        // Assert
        Assert.Single(_circuitController.StateChanges);
        Assert.True(_circuitController.StateChanges[0].State);
    }

    [Fact]
    public async Task Start_ShouldInitializeTimer()
    {
        // Arrange & Act
        await _scheduleService.Start();

        // Assert - Timer should be initialized (we can't directly test the timer, but Start() should not throw)
        // In a real scenario, we might wait and verify the timer ticks, but that's harder to test reliably
    }

    [Fact]
    public async Task ApplySchedule_SyncVersion_ShouldWork()
    {
        // Test the synchronous version for backwards compatibility

        // Arrange
        var schedule = new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), true)
                    }
                }
            }
        };

        await _scheduleService.SetSchedule(schedule);
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Act - using synchronous version
        _scheduleService.ApplySchedule();

        // Assert
        Assert.Single(_circuitController.StateChanges);
        Assert.True(_circuitController.StateChanges[0].State);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Act & Assert
        _scheduleService.Dispose();
    }

    private MasterSchedule CreateTestSchedule()
    {
        return new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "Light1",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 18, 0, 0), true),
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 22, 0, 0), false)
                    }
                }
            }
        };
    }

    public void Dispose()
    {
        _scheduleService?.Dispose();
    }
}