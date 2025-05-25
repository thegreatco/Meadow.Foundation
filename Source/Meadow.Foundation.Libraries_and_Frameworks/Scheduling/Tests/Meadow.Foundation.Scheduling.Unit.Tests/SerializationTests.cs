namespace Meadow.Foundation.Scheduling.Unit.Tests;

public class SerializationTests
{
    private static string GetTestInputPath(string filename)
    {
        return Path.Combine("inputs", filename);
    }

    private static string LoadTestInput(string filename)
    {
        var path = GetTestInputPath(filename);
        return File.ReadAllText(path);
    }

    [Fact]
    public void DeserializeMasterSchedule_WithProvidedTestFile_ShouldWorkCorrectly()
    {
        // This test specifically validates the provided test_schedule_1.json file
        // to ensure our serializer correctly handles the expected format

        // Arrange
        var json = LoadTestInput("test_schedule_1.json");

        // Act
        var result = ScheduleSerializer.DeserializeMasterSchedule(json);

        // Assert - Basic structure
        Assert.NotNull(result);
        Assert.NotNull(result.Schedules);
        Assert.Equal(2, result.Schedules.Length);

        // Validate light schedule
        var lightSchedule = result.Schedules[0];
        Assert.Equal("light", lightSchedule.CircuitName);
        Assert.Equal(5, lightSchedule.Events.Count);

        // Validate fountain schedule
        var fountainSchedule = result.Schedules[1];
        Assert.Equal("fountain", fountainSchedule.CircuitName);
        Assert.Equal(5, fountainSchedule.Events.Count);

        // Test round-trip to ensure our serializer produces compatible JSON
        var serializedJson = ScheduleSerializer.SerializeMasterSchedule(result);
        var roundTripResult = ScheduleSerializer.DeserializeMasterSchedule(serializedJson);

        Assert.Equal(result.Schedules.Length, roundTripResult.Schedules.Length);
        Assert.Equal(lightSchedule.CircuitName, roundTripResult.Schedules[0].CircuitName);
        Assert.Equal(fountainSchedule.CircuitName, roundTripResult.Schedules[1].CircuitName);
    }

    [Fact]
    public void DeserializeMasterSchedule_WithValidJson_ShouldReturnCorrectObject()
    {
        // Arrange
        var json = LoadTestInput("test_schedule_1.json");

        // Act
        var result = ScheduleSerializer.DeserializeMasterSchedule(json);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Schedules);
        Assert.Equal(2, result.Schedules.Length);

        // Test first schedule - light
        var lightSchedule = result.Schedules[0];
        Assert.Equal("light", lightSchedule.CircuitName);
        Assert.Equal(5, lightSchedule.Events.Count);

        // Test daily event
        var dailyEvent = lightSchedule.Events[0] as DailyScheduleEvent;
        Assert.NotNull(dailyEvent);
        Assert.Equal(ScheduleEventType.Daily, dailyEvent.EventType);
        Assert.False(dailyEvent.IsDisabled);
        Assert.True(dailyEvent.DesiredState);
        Assert.Equal(new DateTime(2024, 1, 1, 18, 0, 0), dailyEvent.EventTime);

        // Test weekday event
        var weekdayEvent = lightSchedule.Events[2] as WeekdayScheduleEvent;
        Assert.NotNull(weekdayEvent);
        Assert.Equal(ScheduleEventType.Weekday, weekdayEvent.EventType);
        Assert.False(weekdayEvent.IsDisabled);
        Assert.True(weekdayEvent.DesiredState);
        Assert.Equal(new DateTime(2024, 1, 1, 17, 30, 0), weekdayEvent.EventTime);
        Assert.Equal(5, weekdayEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Monday, weekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Friday, weekdayEvent.DaysOfWeek);

        // Test sunset offset event
        var sunsetEvent = lightSchedule.Events[3] as SunsetOffsetScheduleEvent;
        Assert.NotNull(sunsetEvent);
        Assert.Equal(ScheduleEventType.SunsetOffset, sunsetEvent.EventType);
        Assert.False(sunsetEvent.IsDisabled);
        Assert.True(sunsetEvent.DesiredState);
        Assert.Equal(TimeSpan.FromMinutes(-30), sunsetEvent.Offset); // 30 minutes before sunset
        Assert.Equal(2, sunsetEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Saturday, sunsetEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Sunday, sunsetEvent.DaysOfWeek);

        // Test sunrise offset event
        var sunriseEvent = lightSchedule.Events[4] as SunriseOffsetScheduleEvent;
        Assert.NotNull(sunriseEvent);
        Assert.Equal(ScheduleEventType.SunriseOffset, sunriseEvent.EventType);
        Assert.True(sunriseEvent.IsDisabled);
        Assert.False(sunriseEvent.DesiredState);
        Assert.Equal(TimeSpan.FromHours(1), sunriseEvent.Offset); // 1 hour after sunrise
        Assert.Null(sunriseEvent.DaysOfWeek);

        // Test second schedule - fountain
        var fountainSchedule = result.Schedules[1];
        Assert.Equal("fountain", fountainSchedule.CircuitName);
        Assert.Equal(5, fountainSchedule.Events.Count);
    }

    [Fact]
    public void SerializeAndDeserialize_RoundTrip_ShouldReturnEquivalentObject()
    {
        // Arrange
        var originalSchedule = CreateTestMasterSchedule();

        // Act
        var json = ScheduleSerializer.SerializeMasterSchedule(originalSchedule);
        var roundTripSchedule = ScheduleSerializer.DeserializeMasterSchedule(json);

        // Assert
        Assert.NotNull(roundTripSchedule);
        Assert.Equal(originalSchedule.Schedules.Length, roundTripSchedule.Schedules.Length);

        for (int i = 0; i < originalSchedule.Schedules.Length; i++)
        {
            var original = originalSchedule.Schedules[i];
            var roundTrip = roundTripSchedule.Schedules[i];

            Assert.Equal(original.CircuitName, roundTrip.CircuitName);
            Assert.Equal(original.Events.Count, roundTrip.Events.Count);

            for (int j = 0; j < original.Events.Count; j++)
            {
                AssertEventsEqual(original.Events[j], roundTrip.Events[j]);
            }
        }
    }

    [Fact]
    public void DeserializeMasterSchedule_WithEmptySchedules_ShouldReturnEmptyArray()
    {
        // Arrange
        var json = """{"schedules": []}""";

        // Act
        var result = ScheduleSerializer.DeserializeMasterSchedule(json);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Schedules);
        Assert.Empty(result.Schedules);
    }

    [Fact]
    public void DeserializeMasterSchedule_WithNullSchedules_ShouldHandleGracefully()
    {
        // Arrange
        var json = """{"schedules": null}""";

        // Act
        var result = ScheduleSerializer.DeserializeMasterSchedule(json);

        // Assert
        Assert.NotNull(result);
        // Should handle null schedules gracefully
    }

    [Fact]
    public void DeserializeMasterSchedule_WithEmptyEvents_ShouldReturnEmptyEventsList()
    {
        // Arrange
        var json = """
        {
          "schedules": [
            {
              "circuitName": "test",
              "events": []
            }
          ]
        }
        """;

        // Act
        var result = ScheduleSerializer.DeserializeMasterSchedule(json);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Schedules);
        Assert.Equal("test", result.Schedules[0].CircuitName);
        Assert.Empty(result.Schedules[0].Events);
    }

    [Fact]
    public void DeserializeMasterSchedule_WithInvalidEventType_ShouldThrowException()
    {
        // Arrange
        var json = """
        {
          "schedules": [
            {
              "circuitName": "test",
              "events": [
                {
                  "eventType": "InvalidType",
                  "isDisabled": false,
                  "desiredState": true
                }
              ]
            }
          ]
        }
        """;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ScheduleSerializer.DeserializeMasterSchedule(json));
    }

    [Theory]
    [InlineData("Daily")]
    [InlineData("Weekday")]
    [InlineData("SunriseOffset")]
    [InlineData("SunsetOffset")]
    public void DeserializeMasterSchedule_WithSpecificEventType_ShouldCreateCorrectType(string eventTypeName)
    {
        // Arrange
        var json = CreateJsonForEventType(eventTypeName);

        // Act
        var result = ScheduleSerializer.DeserializeMasterSchedule(json);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Schedules);
        Assert.Single(result.Schedules[0].Events);

        var eventObj = result.Schedules[0].Events[0];
        var expectedType = Enum.Parse<ScheduleEventType>(eventTypeName);
        Assert.Equal(expectedType, eventObj.EventType);
    }

    [Fact]
    public void SerializeMasterSchedule_WithComplexSchedule_ShouldProduceValidJson()
    {
        // Arrange
        var schedule = CreateTestMasterSchedule();

        // Act
        var json = ScheduleSerializer.SerializeMasterSchedule(schedule);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("schedules", json);
        Assert.Contains("circuitName", json);
        Assert.Contains("eventType", json);

        // Should be able to deserialize the result
        var roundTrip = ScheduleSerializer.DeserializeMasterSchedule(json);
        Assert.NotNull(roundTrip);
    }

    [Theory]
    [InlineData("-00:30:00", -30)] // 30 minutes before
    [InlineData("01:00:00", 60)]   // 1 hour after
    [InlineData("00:15:00", 15)]   // 15 minutes after
    [InlineData("-02:00:00", -120)] // 2 hours before
    public void ParseTimeSpanOffset_WithValidOffsets_ShouldReturnCorrectTimeSpan(string offsetString, int expectedMinutes)
    {
        // Arrange
        var json = $@"{{
            ""schedules"": [
            {{
                ""circuitName"": ""test"",
                ""events"": [
                {{
                    ""eventType"": ""SunriseOffset"",
                    ""isDisabled"": false,
                    ""desiredState"": true,
                    ""offset"": ""{offsetString}"",
                    ""daysOfWeek"": null
                }}
                ]
            }}
            ]
        }}";

        // Act
        var result = ScheduleSerializer.DeserializeMasterSchedule(json);

        // Assert
        var sunriseEvent = result.Schedules[0].Events[0] as SunriseOffsetScheduleEvent;
        Assert.NotNull(sunriseEvent);
        Assert.Equal(TimeSpan.FromMinutes(expectedMinutes), sunriseEvent.Offset);
    }

    private static MasterSchedule CreateTestMasterSchedule()
    {
        return new MasterSchedule
        {
            Schedules = new[]
            {
                new Schedule
                {
                    CircuitName = "TestLight",
                    Events = new List<IScheduleEvent>
                    {
                        new DailyScheduleEvent(new DateTime(2024, 1, 1, 19, 0, 0), true),
                        new WeekdayScheduleEvent(
                            new DateTime(2024, 1, 1, 22, 0, 0),
                            false,
                            new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }),
                        new SunriseOffsetScheduleEvent(
                            TimeSpan.FromMinutes(30),
                            true,
                            new[] { DayOfWeek.Saturday }),
                        new SunsetOffsetScheduleEvent(
                            TimeSpan.FromMinutes(-45),
                            false,
                            null)
                    }
                }
            }
        };
    }

    private static string CreateJsonForEventType(string eventType)
    {
        return eventType switch
        {
            "Daily" => """
                {
                  "schedules": [
                    {
                      "circuitName": "test",
                      "events": [
                        {
                          "eventType": "Daily",
                          "isDisabled": false,
                          "desiredState": true,
                          "eventTime": "2024-01-01T18:00:00"
                        }
                      ]
                    }
                  ]
                }
                """,
            "Weekday" => """
                {
                  "schedules": [
                    {
                      "circuitName": "test",
                      "events": [
                        {
                          "eventType": "Weekday",
                          "isDisabled": false,
                          "desiredState": true,
                          "eventTime": "2024-01-01T18:00:00",
                          "daysOfWeek": ["Monday", "Tuesday"]
                        }
                      ]
                    }
                  ]
                }
                """,
            "SunriseOffset" => """
                {
                  "schedules": [
                    {
                      "circuitName": "test",
                      "events": [
                        {
                          "eventType": "SunriseOffset",
                          "isDisabled": false,
                          "desiredState": true,
                          "offset": "01:00:00",
                          "daysOfWeek": ["Saturday"]
                        }
                      ]
                    }
                  ]
                }
                """,
            "SunsetOffset" => """
                {
                  "schedules": [
                    {
                      "circuitName": "test",
                      "events": [
                        {
                          "eventType": "SunsetOffset",
                          "isDisabled": false,
                          "desiredState": true,
                          "offset": "-00:30:00",
                          "daysOfWeek": null
                        }
                      ]
                    }
                  ]
                }
                """,
            _ => throw new ArgumentException($"Unknown event type: {eventType}")
        };
    }

    private static void AssertEventsEqual(IScheduleEvent original, IScheduleEvent roundTrip)
    {
        Assert.Equal(original.EventType, roundTrip.EventType);
        Assert.Equal(original.IsDisabled, roundTrip.IsDisabled);

        switch (original, roundTrip)
        {
            case (DailyScheduleEvent orig, DailyScheduleEvent rt):
                Assert.Equal(orig.DesiredState, rt.DesiredState);
                Assert.Equal(orig.EventTime, rt.EventTime);
                break;

            case (WeekdayScheduleEvent orig, WeekdayScheduleEvent rt):
                Assert.Equal(orig.DesiredState, rt.DesiredState);
                Assert.Equal(orig.EventTime, rt.EventTime);
                Assert.Equal(orig.DaysOfWeek, rt.DaysOfWeek);
                break;

            case (SunriseOffsetScheduleEvent orig, SunriseOffsetScheduleEvent rt):
                Assert.Equal(orig.DesiredState, rt.DesiredState);
                Assert.Equal(orig.Offset, rt.Offset);
                Assert.Equal(orig.DaysOfWeek, rt.DaysOfWeek);
                break;

            case (SunsetOffsetScheduleEvent orig, SunsetOffsetScheduleEvent rt):
                Assert.Equal(orig.DesiredState, rt.DesiredState);
                Assert.Equal(orig.Offset, rt.Offset);
                Assert.Equal(orig.DaysOfWeek, rt.DaysOfWeek);
                break;

            default:
                Assert.True(false, $"Unexpected event type combination: {original.GetType()} vs {roundTrip.GetType()}");
                break;
        }
    }
}