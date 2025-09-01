using Meadow.Units;

namespace Meadow.Foundation.Batteries.Voltaic;

public class SimulatedV10x : IV10x
{
    private readonly Voltage _batteryVoltage;
    private readonly Voltage _inputVoltage;
    private readonly Current _inputCurrent;
    private readonly Voltage _loadVoltage;
    private readonly Current _loadCurrent;
    private readonly Temperature _environmentTemp;
    private readonly Temperature _controllerTemp;

    public SimulatedV10x()
    {
        _batteryVoltage = new Voltage(12.6, Voltage.UnitType.Volts);
        _inputVoltage = new Voltage(14.2, Voltage.UnitType.Volts);
        _inputCurrent = new Current(0.8, Current.UnitType.Amps);
        _loadVoltage = new Voltage(12.4, Voltage.UnitType.Volts);
        _loadCurrent = new Current(0.5, Current.UnitType.Amps);
        _environmentTemp = new Temperature(25.0, Temperature.UnitType.Celsius);
        _controllerTemp = new Temperature(35.0, Temperature.UnitType.Celsius);
    }

    public Voltage BatteryVoltage => _batteryVoltage;

    public Voltage InputVoltage => _inputVoltage;

    public Current InputCurrent => _inputCurrent;

    public Voltage LoadVoltage => _loadVoltage;

    public Current LoadCurrent => _loadCurrent;

    public Temperature EnvironmentTemp => _environmentTemp;

    public Temperature ControllerTemp => _controllerTemp;
}
