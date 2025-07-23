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

    public float GetTargetSpeed()
    {
        return _buffer.Speed;
    }

    public float GetTargetRange()
    {
        return _buffer.Range;
    }

    public uint GetTargetEnergy()
    {
        return _buffer.Energy;
    }

    public bool IsMotionDetected()
    {
        if (communication == CommunicationType.I2C)
        {
            return IsMotionDetectedI2c();
        }
        else
        {
            return IsMotionDetectedSerial();
        }
    }




}