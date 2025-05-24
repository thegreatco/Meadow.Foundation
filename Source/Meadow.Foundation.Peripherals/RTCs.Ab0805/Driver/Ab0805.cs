using Meadow.Hardware;
using System;

namespace Meadow.Foundation.RTCs;

/// <summary>
/// Represents a Ab0805 real-time clock
/// </summary>
public partial class Ab0805 : II2cPeripheral, IRealTimeClock
{
    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    public bool IsRunning { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    private byte[] txBuffer = new byte[16];
    private byte[] rxBuffer = new byte[16];

    private I2cCommunications i2CCommunications;

    /// <summary>
    /// Creates a new Ab0805 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    public Ab0805(II2cBus i2cBus)
    {
        i2CCommunications = new I2cCommunications(i2cBus, (byte)Addresses.Default, 20);
        Initialize();
    }

    private void Initialize()
    {
        // stop the clock
        i2CCommunications.WriteRegister((byte)Registers.CONTROL1, 0x91);
        i2CCommunications.WriteRegister((byte)Registers.CONFIG_KEY, 0xA1);
        i2CCommunications.WriteRegister((byte)Registers.OSC_CONTROL, 0x08);
    }

    public DateTimeOffset GetTime()
    {
        throw new NotImplementedException();
    }

    public void SetTime(DateTimeOffset time)
    {
        throw new NotImplementedException();
    }
}