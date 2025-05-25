using Meadow.Foundation.Scheduling;
using Meadow.Foundation.Serialization;

using System.Linq;

/// <summary>
/// Represents a collection of schedules that can be managed as a single unit.
/// </summary>
public class MasterSchedule
{
    /// <summary>
    /// Gets or sets the array of schedules managed by this master schedule.
    /// </summary>
    public Schedule[] Schedules { get; set; }

    /// <summary>
    /// Gets a value indicating whether any of the schedules contain sunrise or sunset events.
    /// This property is used to determine if sunrise/sunset calculations are needed.
    /// </summary>
    [JsonIgnore]
    public bool ContainsSunriseOrSunsetEvents => Schedules.Any(s => s.ContainsSunriseOrSunsetEvents);
}