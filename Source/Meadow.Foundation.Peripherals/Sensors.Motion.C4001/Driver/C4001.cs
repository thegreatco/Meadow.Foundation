namespace Meadow.Foundation.Sensors.Motion;

/// <summary>
/// Create a new C4001 object
/// </summary>
public partial class C4001
{
    private readonly CommunicationType communication;

    private PrivateData _buffer = new PrivateData();
    private int _flashNumber = 0;

    /// <summary>
    /// Get the current status of the sensor
    /// </summary>
    public SensorStatus GetStatus()
    {
        return GetStatusI2c();
    }

    internal float GetTargetSpeed()
    {
        return _buffer.Speed;
    }

    internal float GetTargetRange()
    {
        return _buffer.Range;
    }

    internal uint GetTargetEnergy()
    {
        return _buffer.Energy;
    }


}