namespace Meadow.Foundation.RTCs;

internal enum Registers : byte
{
    HUNDREDTHS = 0x00,
    SECONDS = 0x01,
    MINUTES = 0x02,
    HOURS = 0x03,
    DATE = 0x04,          // Day of Month
    MONTH = 0x05,
    YEAR = 0x06,          // Years since 2000 in BCD
    DAY_OF_WEEK = 0x07,   // Day of week (1-7, where 1=Mon or Sun, 7=Sun or Sat - C says 1-7)

    HUNDREDTHS_ALARM = 0x08,
    SECONDS_ALARM = 0x09,
    MINUTES_ALARM = 0x0A,
    HOURS_ALARM = 0x0B,
    DATE_ALARM = 0x0C,
    MONTH_ALARM = 0x0D,
    WEEKDAYS_ALARM = 0x0E,

    STATUS = 0x0F,
    CONTROL1 = 0x10,
    CONTROL2 = 0x11,      
                         
    OSC_CONTROL = 0x1C,
    OSC_STATUS = 0x1D,

    CONFIG_KEY = 0x1F, 
    ID0 = 0x28,            
    ID1 = 0x29,            
}

internal static class Control1Bits
{
    public const byte STOP = 7;
    public const byte HourFormat_12_24 = 6; // Hour Format (0 = 24h, 1 = 12h)
    public const byte WRTC = 0; 
}

internal static class DayOfWeekBits
{
    public const byte Mask = 0x07; // To read bits 2,1,0
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