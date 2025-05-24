namespace Meadow.Foundation.RTCs;

internal enum Registers : byte
{
    HUNDREDTHS = 0X00,
    SECONDS = 0x01,
    MINUTES = 0x02,
    HOURS = 0x03,
    DATE = 0x04,          // Day of Month
    MONTH = 0x05,
    YEAR = 0x06,          // Years since 2000 in BCD
    DAY_OF_WEEK = 0X07,   // Day of week (1-7, where 1=Mon or Sun, 7=Sun or Sat - C says 1-7)

    HUNDREDTHS_ALARM = 0X08,
    SECONDS_ALARM = 0X09,
    MINUTES_ALARM = 0X0A,
    HOURS_ALARM = 0X0B,
    DATE_ALARM = 0X0C,
    MONTH_ALARM = 0X0D,
    WEEKDAYS_ALARM = 0X0E,

    STATUS = 0X0F,
    CONTROL1 = 0x10,
    CONTROL2 = 0x11,      
                         
    OSC_CONTROL = 0x1C,
    OSC_STATUS = 0x1D,

    CONFIG_KEY = 0X1F,    // Write 0xA1 to access OSC_CONTROL, 0x3C for Software Reset
    ID0 = 0X28,            
    ID1 = 0X29,            
}


/// <summary>
/// Represents the unit of delay time
/// </summary>
public enum DelayTimeUnit
{
    /// <summary>
    /// Delay time in seconds
    /// </summary>
    Seconds,
    /// <summary>
    /// Delay time in minutes
    /// </summary>
    Minutes,
    /// <summary>
    /// Delay time in hours
    /// </summary>
    Hours
}

public partial class Ab0805
{
    /// <summary>
    /// Valid I2C addresses for the sensor
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary>
        /// Bus address 0x69
        /// </summary>
        Address_0x69 = 0x69,
        /// <summary>
        /// Default bus address
        /// </summary>
        Default = Address_0x69
    }
}