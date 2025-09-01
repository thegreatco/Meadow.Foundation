using Meadow.Units;

namespace Meadow.Foundation.Batteries.Voltaic;

public interface IV10x
{
    Voltage BatteryVoltage { get; }
    Voltage InputVoltage { get; }
    Current InputCurrent { get; }
    Voltage LoadVoltage { get; }
    Current LoadCurrent { get; }
    Temperature EnvironmentTemp { get; }
    Temperature ControllerTemp { get; }
}
